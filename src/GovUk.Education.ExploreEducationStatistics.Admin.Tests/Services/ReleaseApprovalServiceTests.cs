#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseApprovalServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public async Task CreateReleaseStatus_Amendment_NoUniqueSlugFailure()
        {
            var releaseType = new ReleaseType {Title = "Ad Hoc"};

            var publication = new Publication();

            var initialReleaseId = Guid.NewGuid();
            var initialRelease = new Release
            {
                Id = initialReleaseId,
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.TaxYear,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = initialReleaseId
            };

            var amendedRelease = new Release
            {
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Publication = publication,
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 1,
                PreviousVersionId = initialRelease.Id
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(releaseType);
                await context.AddRangeAsync(amendedRelease, initialRelease);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(amendedRelease.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(
                    context,
                    contentService: contentService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        amendedRelease.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishScheduled = "2051-06-30",
                            ApprovalStatus = ReleaseApprovalStatus.Draft
                        }
                    );

                VerifyAllMocks(contentService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == amendedRelease.Id);

                Assert.Equal("2030", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.CalendarYear, saved.TimePeriodCoverage);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus()
        {
            var releaseId = Guid.NewGuid();
            var adHocReleaseType = new ReleaseType {Title = "Ad Hoc"};
            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                TimePeriodCoverage = TimeIdentifier.March,
                Slug = "2030-march",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Access list",
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(adHocReleaseType, release);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(release.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(
                    context,
                    contentService: contentService.Object);

                await releaseService
                    .CreateReleaseStatus(
                        releaseId,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishMethod = PublishMethod.Scheduled,
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = nextReleaseDateEdited,
                            ApprovalStatus = ReleaseApprovalStatus.Draft,
                            LatestInternalReleaseNote = "Test internal note"
                        }
                    );

                VerifyAllMocks(contentService);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == releaseId);

                Assert.Equal(release.Publication.Id, saved.PublicationId);
                Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc),
                    saved.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, saved.NextReleaseDate);
                Assert.False(saved.NotifySubscribers);
                Assert.Equal(adHocReleaseType.Id, saved.TypeId);
                Assert.Equal("2030-march", saved.Slug);
                Assert.Equal("2030", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
                Assert.Equal("Access list", saved.PreReleaseAccessList);

                Assert.Single(saved.ReleaseStatuses);
                var status = saved.ReleaseStatuses[0];
                Assert.NotNull(status.Created);
                Assert.InRange(DateTime.UtcNow
                    .Subtract(status.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(release.Id, status.ReleaseId);
                Assert.Equal(ReleaseApprovalStatus.Draft, status.ApprovalStatus);
                Assert.Equal(_userId, status.CreatedById);
                Assert.Equal("Test internal note", status.InternalReleaseNote);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Approved_FailsOnChecklistErrors()
        {
            var release = new Release
            {
                Type = new ReleaseType {Title = "Ad Hoc"},
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                releaseChecklistService
                    .Setup(s =>
                            s.GetErrors(It.Is<Release>(r => r.Id == release.Id)))
                    .ReturnsAsync(
                        new List<ReleaseChecklistIssue>
                        {
                            new(DataFileImportsMustBeCompleted),
                            new(DataFileReplacementsMustBeCompleted)
                        }
                    );

                var releaseService = BuildService(
                    context,
                    releaseChecklistService: releaseChecklistService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            LatestInternalReleaseNote = "Test note",
                            PublishMethod = PublishMethod.Scheduled,
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = new PartialDate {Month="12", Year="2000"}
                        }
                    );

                VerifyAllMocks(releaseChecklistService);

                result.AssertBadRequest(
                    DataFileImportsMustBeCompleted, 
                    DataFileReplacementsMustBeCompleted);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Approved_FailsNoPublishScheduledDate()
        {
            var release = new Release
            {
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                },
                ReleaseName = "2030",
                Slug = "2030",
                Published = DateTime.Now,
                PublishScheduled = DateTime.UtcNow,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(context);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            LatestInternalReleaseNote = "Test note",
                            PublishMethod = PublishMethod.Scheduled,
                            NextReleaseDate = new PartialDate {Month="12", Year="2000"}
                        }
                    );

                result.AssertBadRequest(ApprovedReleaseMustHavePublishScheduledDate);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Approved_FailsChangingToDraft()
        {
            var release = new Release
            {
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                },
                ReleaseName = "2030",
                Slug = "2030",
                Published = DateTime.Now,
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(context);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft,
                        }
                    );

                result.AssertBadRequest(PublishedReleaseCannotBeUnapproved);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Approved_SendPreReleaseEmails()
        {
            var release = new Release
            {
                Type = new ReleaseType {Title = "Ad Hoc"},
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
            var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                releaseChecklistService
                    .Setup(s =>
                        s.GetErrors(It.Is<Release>(r => r.Id == release.Id)))
                    .ReturnsAsync(
                        new List<ReleaseChecklistIssue>()
                    );

                publishingService
                    .Setup(s => s.ReleaseChanged(
                        It.Is<Guid>(g => g == release.Id),
                        It.IsAny<Guid>(),
                        It.IsAny<bool>()
                    ))
                    .ReturnsAsync(Unit.Instance);

                contentService.Setup(mock =>
                        mock.GetContentBlocks<HtmlBlock>(release.Id))
                    .ReturnsAsync(new List<HtmlBlock>());

                preReleaseUserService.Setup(mock =>
                        mock.SendPreReleaseUserInviteEmails(release.Id))
                    .ReturnsAsync(Unit.Instance);

            var releaseService = BuildService(contentDbContext: context,
                    releaseChecklistService: releaseChecklistService.Object,
                    publishingService: publishingService.Object,
                    contentService: contentService.Object,
                    preReleaseUserService: preReleaseUserService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            LatestInternalReleaseNote = "Test note",
                            PublishMethod = PublishMethod.Scheduled,
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = new PartialDate {Month="12", Year="2000"}
                        }
                    );

                VerifyAllMocks(releaseChecklistService, contentService, preReleaseUserService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Approved_NotifySubscribers()
        {
            var release = new Release
            {
                Type = new ReleaseType {Title = "Ad Hoc"},
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
            var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                releaseChecklistService
                    .Setup(s =>
                        s.GetErrors(It.Is<Release>(r => r.Id == release.Id)))
                    .ReturnsAsync(
                        new List<ReleaseChecklistIssue>()
                    );

                publishingService
                    .Setup(s => s.ReleaseChanged(
                        It.Is<Guid>(g => g == release.Id),
                        It.IsAny<Guid>(),
                        It.IsAny<bool>()
                    ))
                    .ReturnsAsync(Unit.Instance);

                contentService.Setup(mock =>
                        mock.GetContentBlocks<HtmlBlock>(release.Id))
                    .ReturnsAsync(new List<HtmlBlock>());

                preReleaseUserService.Setup(mock =>
                        mock.SendPreReleaseUserInviteEmails(It.Is<Release>(r => r.Id == release.Id)))
                    .ReturnsAsync(Unit.Instance);

                var releaseService = BuildService(contentDbContext: context,
                    releaseChecklistService: releaseChecklistService.Object,
                    publishingService: publishingService.Object,
                    contentService: contentService.Object,
                    preReleaseUserService: preReleaseUserService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            LatestInternalReleaseNote = "Test note",
                            // No need to include NotifySubscribers - should default to true for original releases
                            PublishMethod = PublishMethod.Scheduled,
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = new PartialDate { Month = "12", Year = "2000" }
                        }
                    );

                VerifyAllMocks(releaseChecklistService, contentService);

                result.AssertRight();
            }
            
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == release.Id);

                Assert.True(saved.NotifySubscribers);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Amendment_NotifySubscribers_False()
        {
            var releaseType = new ReleaseType {Title = "Ad Hoc"};

            var publication = new Publication();

            var initialReleaseId = Guid.NewGuid();
            var initialRelease = new Release
            {
                Id = initialReleaseId,
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.TaxYear,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = initialReleaseId
            };

            var amendedRelease = new Release
            {
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.TaxYear,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 1,
                PreviousVersionId = initialReleaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(releaseType);
                await context.AddRangeAsync(amendedRelease, initialRelease);
                await context.SaveChangesAsync();
            }

            var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
            var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);

            releaseChecklistService
                .Setup(s =>
                    s.GetErrors(It.Is<Release>(r => r.Id == amendedRelease.Id)))
                .ReturnsAsync(
                    new List<ReleaseChecklistIssue>()
                );

            publishingService
                .Setup(s => s.ReleaseChanged(
                    It.Is<Guid>(g => g == amendedRelease.Id),
                    It.IsAny<Guid>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(Unit.Instance);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(amendedRelease.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            preReleaseUserService.Setup(mock =>
                    mock.SendPreReleaseUserInviteEmails(It.Is<Release>(r => r.Id == amendedRelease.Id)))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(
                    context,
                    releaseChecklistService: releaseChecklistService.Object,
                    publishingService: publishingService.Object,
                    contentService: contentService.Object,
                    preReleaseUserService: preReleaseUserService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        amendedRelease.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishScheduled = "2051-06-30",
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            NotifySubscribers = false,
                        }
                    );

                VerifyAllMocks(contentService, releaseChecklistService);

                result.AssertRight();
            }
            
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == amendedRelease.Id);

                Assert.False(saved.NotifySubscribers);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Amendment_NotifySubscribers_True()
        {
            var releaseType = new ReleaseType {Title = "Ad Hoc"};

            var publication = new Publication();

            var initialReleaseId = Guid.NewGuid();
            var initialRelease = new Release
            {
                Id = initialReleaseId,
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.TaxYear,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = initialReleaseId
            };

            var amendedRelease = new Release
            {
                Type = releaseType,
                TimePeriodCoverage = TimeIdentifier.TaxYear,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 1,
                PreviousVersionId = initialReleaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(releaseType);
                await context.AddRangeAsync(amendedRelease, initialRelease);
                await context.SaveChangesAsync();
            }

            var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
            var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);

            releaseChecklistService
                .Setup(s =>
                    s.GetErrors(It.Is<Release>(r => r.Id == amendedRelease.Id)))
                .ReturnsAsync(
                    new List<ReleaseChecklistIssue>()
                );

            publishingService
                .Setup(s => s.ReleaseChanged(
                    It.Is<Guid>(g => g == amendedRelease.Id),
                    It.IsAny<Guid>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(Unit.Instance);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(amendedRelease.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            preReleaseUserService.Setup(mock =>
                    mock.SendPreReleaseUserInviteEmails(It.Is<Release>(r => r.Id == amendedRelease.Id)))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(contentDbContext: context,
                    releaseChecklistService: releaseChecklistService.Object,
                    publishingService: publishingService.Object,
                    contentService: contentService.Object,
                    preReleaseUserService: preReleaseUserService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        amendedRelease.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishScheduled = "2051-06-30",
                            ApprovalStatus = ReleaseApprovalStatus.Approved,
                            NotifySubscribers = true,
                        }
                    );

                VerifyAllMocks(contentService, releaseChecklistService);

                result.AssertRight();
            }
            
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == amendedRelease.Id);

                Assert.True(saved.NotifySubscribers);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_Draft_DoNotSendPreReleaseEmails()
        {
            var release = new Release
            {
                Type = new ReleaseType {Title = "Ad Hoc"},
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                Slug = "2030",
                Version = 0,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                contentService.Setup(mock =>
                        mock.GetContentBlocks<HtmlBlock>(release.Id))
                    .ReturnsAsync(new List<HtmlBlock>());

                var releaseService = BuildService(
                    context,
                    contentService: contentService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        release.Id,
                        new ReleaseStatusCreateViewModel
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft,
                            LatestInternalReleaseNote = "Test note",
                        }
                    );

                VerifyAllMocks(contentService);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_ReleaseHasImages()
        {
            var releaseId = Guid.NewGuid();

            var adHocReleaseType = new ReleaseType {Title = "Ad Hoc"};

            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                TimePeriodCoverage = TimeIdentifier.March,
                Slug = "2030-march",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Access list",
                Version = 0,
                PreviousVersionId = releaseId
            };

            var imageFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(adHocReleaseType, release, imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(release.Id))
                .ReturnsAsync(new List<HtmlBlock>
                {
                    new HtmlBlock
                    {
                        Body = $@"
    <img src=""/api/releases/{{releaseId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/releases/{{releaseId}}/images/{imageFile2.File.Id}""/>"
                    }
                });

            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(
                    context,
                    contentService: contentService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        releaseId,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = nextReleaseDateEdited,
                            ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                            LatestInternalReleaseNote = "Internal note"
                        }
                    );

                VerifyAllMocks(contentService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == releaseId);

                Assert.Equal(release.Publication.Id, saved.PublicationId);
                Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc), saved.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, saved.NextReleaseDate);
                Assert.False(saved.NotifySubscribers);
                Assert.Equal(adHocReleaseType.Id, saved.TypeId);
                Assert.Equal("2030-march", saved.Slug);
                Assert.Equal("2030", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
                Assert.Equal("Access list", saved.PreReleaseAccessList);

                Assert.Single(saved.ReleaseStatuses);
                var savedStatus = saved.ReleaseStatuses[0];
                Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, savedStatus.ApprovalStatus);
                Assert.Equal("Internal note", savedStatus.InternalReleaseNote);
                Assert.NotNull(savedStatus.Created);
                Assert.InRange(DateTime.UtcNow
                    .Subtract(savedStatus.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_userId, savedStatus.CreatedById);
            }
        }

        [Fact]
        public async Task CreateReleaseStatus_ReleaseHasUnusedImages()
        {
            var releaseId = Guid.NewGuid();

            var adHocReleaseType = new ReleaseType {Title = "Ad Hoc"};

            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
                Publication = new Publication {Title = "Old publication"},
                ReleaseName = "2030",
                TimePeriodCoverage = TimeIdentifier.March,
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Access list",
                Version = 0,
                PreviousVersionId = releaseId
            };

            var imageFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release, adHocReleaseType, imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(release.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            releaseFileService.Setup(mock =>
                    mock.Delete(release.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false))
                .ReturnsAsync(Unit.Instance);

            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(contentDbContext: context,
                    contentService: contentService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await releaseService
                    .CreateReleaseStatus(
                        releaseId,
                        new ReleaseStatusCreateViewModel
                        {
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = nextReleaseDateEdited,
                            ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                            LatestInternalReleaseNote = "Test internal note"
                        }
                    );

                releaseFileService.Verify(mock =>
                    mock.Delete(release.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false), Times.Once);

                VerifyAllMocks(contentService, releaseFileService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == releaseId);

                Assert.Equal(release.Publication.Id, saved.PublicationId);
                Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc), saved.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, saved.NextReleaseDate);
                Assert.False(saved.NotifySubscribers);
                Assert.Equal(adHocReleaseType.Id, saved.TypeId);
                Assert.Equal("2030", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
                Assert.Equal("Access list", saved.PreReleaseAccessList);

                Assert.Single(saved.ReleaseStatuses);
                var savedStatus = saved.ReleaseStatuses[0];
                Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, savedStatus.ApprovalStatus);
                Assert.Equal("Test internal note", savedStatus.InternalReleaseNote);
                Assert.False(savedStatus.NotifySubscribers);
                Assert.NotNull(savedStatus.Created);
                Assert.InRange(DateTime.UtcNow
                    .Subtract(savedStatus.Created ?? new DateTime()).Milliseconds, 0, 1500);
                Assert.Equal(_userId, savedStatus.CreatedById);
            }
        }

        [Fact]
        public async Task GetReleaseStatuses()
        {
            var release = new Release
            {
                PublicationId = Guid.NewGuid(),
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Note1 - old",
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Created = new DateTime(2001, 1, 1),
                        CreatedBy = new User { Email = "note1@test.com" }
                    },
                    new()
                    {
                        InternalReleaseNote = "Note2 - latest",
                        ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                        Created = new DateTime(2002, 2, 2),
                        CreatedBy = new User { Email = "note2@test.com" }
                    },
                    new()
                    {
                        InternalReleaseNote = "Note3 - null created",
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = null, // A migrated ReleaseStatus has null Created and CreatedBy
                        CreatedBy = null
                    }
                }
            };
            var ignoredRelease = new Release
            {
                PublicationId = Guid.NewGuid(),
                Version = 3,
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Note4 - different release",
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = new DateTime(2012, 12, 12),
                        CreatedBy = new User { Email = "note4@test.com" }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(release, ignoredRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseRepository = new ReleaseRepository(contentDbContext,
                    Mock.Of<StatisticsDbContext>(),
                    Mock.Of<IMapper>());
                var releaseService = BuildService(contentDbContext, releaseRepository: releaseRepository);
                var result = await releaseService.GetReleaseStatuses(release.Id);

                var resultStatuses = result.AssertRight();
                Assert.Equal(3, resultStatuses.Count);

                var orderedReleaseStatuses = release.ReleaseStatuses
                    .OrderByDescending(rs => rs.Created)
                    .ToList();

                Assert.Equal(orderedReleaseStatuses[0].InternalReleaseNote, resultStatuses[0].InternalReleaseNote);
                Assert.Equal(orderedReleaseStatuses[0].ApprovalStatus,resultStatuses[0].ApprovalStatus);
                Assert.Equal(orderedReleaseStatuses[0].Created, resultStatuses[0].Created);
                Assert.Equal(orderedReleaseStatuses[0].CreatedBy?.Email, resultStatuses[0].CreatedByEmail);
                Assert.Equal(orderedReleaseStatuses[0].Release.Version, resultStatuses[0].ReleaseVersion);

                Assert.Equal(orderedReleaseStatuses[1].InternalReleaseNote, resultStatuses[1].InternalReleaseNote);
                Assert.Equal(orderedReleaseStatuses[1].ApprovalStatus,resultStatuses[1].ApprovalStatus);
                Assert.Equal(orderedReleaseStatuses[1].Created, resultStatuses[1].Created);
                Assert.Equal(orderedReleaseStatuses[1].CreatedBy?.Email, resultStatuses[1].CreatedByEmail);
                Assert.Equal(orderedReleaseStatuses[1].Release.Version, resultStatuses[1].ReleaseVersion);

                Assert.Equal(orderedReleaseStatuses[2].InternalReleaseNote, resultStatuses[2].InternalReleaseNote);
                Assert.Equal(orderedReleaseStatuses[2].ApprovalStatus,resultStatuses[2].ApprovalStatus);
                Assert.Equal(orderedReleaseStatuses[2].Created, resultStatuses[2].Created);
                Assert.Equal(orderedReleaseStatuses[2].CreatedBy?.Email, resultStatuses[2].CreatedByEmail);
                Assert.Equal(orderedReleaseStatuses[2].Release.Version, resultStatuses[2].ReleaseVersion);
            }
        }

        [Fact]
        public async Task GetReleasesStatuses_PreviousReleaseVersions()
        {
            var originalRelease = new Release
            {
                PreviousVersionId = null,
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "First",
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Created = new DateTime(2000, 12, 12),
                        CreatedBy = new User {Email = "first@test.com"}
                    },
                    new()
                    {
                        InternalReleaseNote = "Second",
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Created = new DateTime(2000, 12, 13),
                        CreatedBy = new User {Email = "second@test.com"}
                    },
                }
            };
            var amendedRelease = new Release
            {
                PreviousVersion = originalRelease,
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Fourth",
                        ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                        Created = new DateTime(2001, 12, 11),
                        CreatedBy = new User {Email = "fourth@test.com"}
                    },
                    new()
                    {
                        InternalReleaseNote = "Third",
                        ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                        Created = new DateTime(2001, 11, 11),
                        CreatedBy = new User {Email = "third@test.com"}
                    },
                }
            };

            var ignoredRelease1 = new Release
            {
                PreviousVersionId = null,
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Ignored",
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        Created = new DateTime(2000, 10, 10),
                        CreatedBy = new User {Email = "ignored@test.com"}
                    }
                }
            };

            var ignoredRelease2 = new Release
            {
                PreviousVersionId = Guid.NewGuid(),
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Ignored",
                        ApprovalStatus = ReleaseApprovalStatus.Draft,
                        Created = new DateTime(2000, 9, 9),
                        CreatedBy = new User {Email = "ignored@test.com"}
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddRangeAsync(
                    originalRelease, amendedRelease, ignoredRelease1, ignoredRelease2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseRepository = new ReleaseRepository(contentDbContext,
                    Mock.Of<StatisticsDbContext>(),
                    Mock.Of<IMapper>());
                var releaseService = BuildService(contentDbContext, releaseRepository: releaseRepository);
                var result = await releaseService.GetReleaseStatuses(amendedRelease.Id);

                var resultStatuses = result.AssertRight();
                Assert.Equal(4, resultStatuses.Count);

                // "Fourth"
                Assert.Equal(amendedRelease.ReleaseStatuses[0].InternalReleaseNote, resultStatuses[0].InternalReleaseNote);
                Assert.Equal(amendedRelease.ReleaseStatuses[0].ApprovalStatus, resultStatuses[0].ApprovalStatus);
                Assert.Equal(amendedRelease.ReleaseStatuses[0].Created, resultStatuses[0].Created);
                Assert.Equal(amendedRelease.ReleaseStatuses[0].CreatedBy?.Email, resultStatuses[0].CreatedByEmail);
                Assert.Equal(amendedRelease.Version, resultStatuses[0].ReleaseVersion);

                // "Third"
                Assert.Equal(amendedRelease.ReleaseStatuses[1].InternalReleaseNote, resultStatuses[1].InternalReleaseNote);
                Assert.Equal(amendedRelease.ReleaseStatuses[1].ApprovalStatus, resultStatuses[1].ApprovalStatus);
                Assert.Equal(amendedRelease.ReleaseStatuses[1].Created, resultStatuses[1].Created);
                Assert.Equal(amendedRelease.ReleaseStatuses[1].CreatedBy?.Email, resultStatuses[1].CreatedByEmail);
                Assert.Equal(amendedRelease.Version, resultStatuses[1].ReleaseVersion);

                // "Second"
                Assert.Equal(originalRelease.ReleaseStatuses[1].InternalReleaseNote, resultStatuses[2].InternalReleaseNote);
                Assert.Equal(originalRelease.ReleaseStatuses[1].ApprovalStatus, resultStatuses[2].ApprovalStatus);
                Assert.Equal(originalRelease.ReleaseStatuses[1].Created, resultStatuses[2].Created);
                Assert.Equal(originalRelease.ReleaseStatuses[1].CreatedBy?.Email, resultStatuses[2].CreatedByEmail);
                Assert.Equal(originalRelease.Version, resultStatuses[2].ReleaseVersion);

                // "First"
                Assert.Equal(originalRelease.ReleaseStatuses[0].InternalReleaseNote, resultStatuses[3].InternalReleaseNote);
                Assert.Equal(originalRelease.ReleaseStatuses[0].ApprovalStatus, resultStatuses[3].ApprovalStatus);
                Assert.Equal(originalRelease.ReleaseStatuses[0].Created, resultStatuses[3].Created);
                Assert.Equal(originalRelease.ReleaseStatuses[0].CreatedBy?.Email, resultStatuses[3].CreatedByEmail);
                Assert.Equal(originalRelease.Version, resultStatuses[3].ReleaseVersion);
            }
        }

        [Fact]
        public async Task GetReleaseStatuses_NoResults()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }


            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseRepository = new ReleaseRepository(
                    context,
                    Mock.Of<StatisticsDbContext>(MockBehavior.Strict),
                    Mock.Of<IMapper>(MockBehavior.Strict));
                var releaseService = BuildService(context,
                    releaseRepository: releaseRepository);
                var result = await releaseService.GetReleaseStatuses(release.Id);

                var resultStatuses = result.AssertRight();
                Assert.Empty(resultStatuses);
            }
        }

        private ReleaseApprovalService BuildService(
            ContentDbContext contentDbContext,
            IPublishingService? publishingService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseChecklistService? releaseChecklistService = null,
            IContentService? contentService = null,
            IPreReleaseUserService? preReleaseUserService = null,
            IReleaseRepository? releaseRepository = null)
        {
            var userService = AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return new ReleaseApprovalService(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService.Object,
                publishingService ?? Mock.Of<IPublishingService>(MockBehavior.Strict),
                releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(MockBehavior.Strict),
                contentService ?? Mock.Of<IContentService>(MockBehavior.Strict),
                preReleaseUserService ?? Mock.Of<IPreReleaseUserService>(MockBehavior.Strict),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                releaseFileService ?? Mock.Of<IReleaseFileService>(MockBehavior.Strict),
                releaseRepository ?? Mock.Of<IReleaseRepository>(MockBehavior.Strict));
        }
    }
}
