#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyApprovalServiceTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task UpdateApprovalStatus_MethodologyHasImages()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>
                {
                    new()
                    {
                        Body = $@"
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile2.File.Id}""/>"
                    }
                });

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal("Test approval", updatedMethodologyVersion.InternalReleaseNote);
                Assert.Null(updatedMethodologyVersion.Published);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingMethodologyWithUnusedImages()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            imageService.Setup(mock =>
                    mock.Delete(methodologyVersion.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false))
                .ReturnsAsync(Unit.Instance);

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                imageService.Verify(mock =>
                    mock.Delete(methodologyVersion.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false), Times.Once);

                VerifyAllMocks(contentService, imageService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal("Test approval", updatedMethodologyVersion.InternalReleaseNote);
                Assert.Null(updatedMethodologyVersion.Published);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy_NotCurrentlyPubliclyVisible()
        {
            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request))
                    .AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal("Test approval", updatedMethodologyVersion.InternalReleaseNote);
                Assert.False(updatedMethodologyVersion.Published.HasValue);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);
                Assert.Equal(request.LatestInternalReleaseNote, updatedMethodologyVersion.InternalReleaseNote);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.False(updatedMethodology.Published.HasValue);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(request.LatestInternalReleaseNote, updatedMethodology.InternalReleaseNote);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy()
        {
            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);
            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(true);

            publishingService.Setup(mock => mock.PublishMethodologyFiles(methodologyVersion.Id))
                .ReturnsAsync(Unit.Instance);

            methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                .ReturnsAsync(
                    new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publishingService: publishingService.Object,
                    methodologyCacheService: methodologyCacheService.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(
                    contentService, 
                    methodologyVersionRepository, 
                    publishingService, 
                    methodologyCacheService);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal("Test approval", updatedMethodologyVersion.InternalReleaseNote);
                Assert.True(updatedMethodologyVersion.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodologyVersion.Published!.Value).Milliseconds, 0, 1500);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);
                Assert.Equal(request.LatestInternalReleaseNote, updatedMethodologyVersion.InternalReleaseNote);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.True(updatedMethodology.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Published!.Value).Milliseconds, 0, 1500);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(request.LatestInternalReleaseNote, updatedMethodology.InternalReleaseNote);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy_ScheduledWithReleaseIsCleared()
        {
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                Status = Approved,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                },
                // Existing ScheduledWithRelease should be cleared
                ScheduledWithRelease = scheduledWithRelease
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                // Requested id should be ignored
                WithReleaseId = scheduledWithRelease.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                // Existing ScheduledWithReleaseId is cleared as requested publishing strategy is not WithRelease
                Assert.Null(updatedMethodology.ScheduledWithReleaseId);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_NonLiveRelease()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
                Assert.Equal("Test approval", updatedMethodologyVersion.InternalReleaseNote);
                Assert.Null(updatedMethodologyVersion.Published);
                Assert.Equal(WithRelease, updatedMethodologyVersion.PublishingStrategy);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);

                Assert.NotNull(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(scheduledWithRelease.Id, updatedMethodologyVersion.ScheduledWithRelease!.Id);
                Assert.Equal(scheduledWithRelease.Title, updatedMethodologyVersion.ScheduledWithRelease.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(WithRelease, updatedMethodology.PublishingStrategy);
                Assert.Equal(scheduledWithRelease.Id, updatedMethodology.ScheduledWithReleaseId);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseIdMissing()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            // Create a request with a release Id that is null
            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = null,
                Status = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context);

                var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseIdNotFound()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            // Create a request with a random release Id that won't exist
            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = Guid.NewGuid(),
                Status = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context);


                var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseAlreadyPublished()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            // Create a release that is already published which the methodology cannot be made dependant on
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021",
                Published = DateTime.UtcNow
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context);

                var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
                result.AssertBadRequest(MethodologyCannotDependOnPublishedRelease);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseNotRelated()
        {
            // Release is not from the same publication as the one linked to the methodology
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication(),
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context);

                var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
                result.AssertBadRequest(MethodologyCannotDependOnRelease);
            }
        }

        [Fact]
        public async Task UpdateApprovalStatus_UnapprovingMethodology()
        {
            var methodologyVersion = new MethodologyVersion
            {
                InternalReleaseNote = "Test approval",
                Published = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyApprovalUpdateRequest
            {
                LatestInternalReleaseNote = "A release note to be ignored",
                PublishingStrategy = Immediately,
                Status = Draft
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Un-approving is allowed for users that can approve the methodology providing it's not publicly accessible
                // Test that un-approving alters the status
                var updatedMethodologyVersion = (await service.UpdateApprovalStatus(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);

                // Original release note is cleared if unapproving
                Assert.Null(updatedMethodologyVersion.InternalReleaseNote);
                Assert.Null(updatedMethodologyVersion.Published);
                Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
                Assert.Null(updatedMethodologyVersion.ScheduledWithRelease);
                Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Null(updatedMethodology.InternalReleaseNote);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        private static MethodologyApprovalService SetupService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IMethodologyContentService? methodologyContentService = null,
            IMethodologyFileRepository? methodologyFileRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IPublishingService? publishingService = null,
            IUserService? userService = null,
            IMethodologyCacheService? methodologyCacheService = null)
        {
            return new(
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                contentDbContext,
                methodologyContentService ?? Mock.Of<IMethodologyContentService>(Strict),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                methodologyImageService ?? Mock.Of<IMethodologyImageService>(Strict),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                userService ?? AlwaysTrueUserService(UserId).Object,
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict));
        }
    }
}
