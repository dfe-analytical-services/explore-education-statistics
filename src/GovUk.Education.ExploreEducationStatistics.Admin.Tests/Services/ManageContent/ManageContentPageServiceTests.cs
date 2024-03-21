#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ManageContentPageServiceTests
    {
        [Fact]
        public async Task GetManageContentPageViewModel()
        {
            var releaseId = Guid.NewGuid();
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = releaseVersionId,
                Contact = new Contact
                {
                    ContactName = "Name",
                    ContactTelNo = "01234 567890",
                    TeamEmail = "test@test.com",
                    TeamName = "Team Name"
                },
                ReleaseSeries = new List<ReleaseSeriesItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ReleaseId = releaseId,
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        LegacyLinkDescription = "Legacy 2018/19",
                        LegacyLinkUrl = "https://legacy-2018-19"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        LegacyLinkDescription = "Legacy 2017/18",
                        LegacyLinkUrl = "https://legacy-2017-18"
                    },
                },
                Slug = "test-publication",
                Title = "Publication",
                Topic = new Topic
                {
                    Theme = new Theme
                    {
                        Title = "Theme"
                    }
                }
            };

            var releaseVersion = new ReleaseVersion
            {
                Id = releaseVersionId,
                NextReleaseDate = new PartialDate {Day = "9", Month = "9", Year = "2040"},
                PreReleaseAccessList = "Test access list",
                Publication = publication,
                Release = new Release { Id = releaseId },
                PublishScheduled = DateTime.Parse("2020-09-08T23:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = DateTime.UtcNow,
                ReleaseName = "2020",
                RelatedInformation = new List<Link>
                {
                    new()
                    {
                        Description = "Related 1",
                        Url = "https://related-1"
                    }
                },
                Slug = "2020-21",
                TimePeriodCoverage = AcademicYear,
                Type = ReleaseType.OfficialStatistics,
                Updates = new List<Update>(),
            };

            var unattachedDataBlockParent = new DataBlockParent
            {
                LatestPublishedVersion = new DataBlockVersion
                {
                    ReleaseVersionId = releaseVersion.Id,
                    Id = Guid.NewGuid()
                }
            };

            var keyStatDataBlockParent = new DataBlockParent
            {
                LatestPublishedVersion = new DataBlockVersion
                {
                    ReleaseVersionId = releaseVersion.Id,
                    Id = Guid.NewGuid(),
                }
            };

            var inContentDataBlockVersionId = Guid.NewGuid();

            var inContentDataBlockParent = new DataBlockParent
            {
                LatestPublishedVersion = new DataBlockVersion
                {
                    ReleaseVersionId = releaseVersion.Id,
                    Id = inContentDataBlockVersionId,
                    ContentBlock = new DataBlock
                    {
                        Id = inContentDataBlockVersionId,
                        Order = 1
                    }
                }
            };

            releaseVersion.KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticText {Order = 1},
                new KeyStatisticDataBlock
                {
                    Order = 0,
                    DataBlockId = keyStatDataBlockParent.LatestPublishedVersion!.Id
                }
            };

            var otherReleaseVersion = new ReleaseVersion
            {
                NextReleaseDate = new PartialDate {Day = "8", Month = "8", Year = "2040"},
                Publication = publication,
                PublishScheduled = DateTime.Parse("2020-08-07T23:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = null,
                ReleaseName = "2019",
                Slug = "2019-20",
                TimePeriodCoverage = AcademicYear,
                Type = ReleaseType.OfficialStatistics,
            };

            var unattachedDataBlocks = new List<DataBlockViewModel>
            {
                new()
                {
                    Id = unattachedDataBlockParent.LatestPublishedVersion!.Id
                }
            };

            var ancillaryFileId = Guid.NewGuid();
            var dataFileId = Guid.NewGuid();
            var files = new List<FileInfo>
            {
                new()
                {
                    Id = ancillaryFileId,
                    FileName = "ancillary.pdf",
                    Name = "Ancillary File",
                    Size = "10 Kb",
                    Type = Ancillary
                },
                new()
                {
                    Id = dataFileId,
                    FileName = "data.csv",
                    Name = "Subject File",
                    Size = "20 Kb",
                    Type = FileType.Data
                }
            };

            var methodologies = AsList(
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology 1 title",
                    Status = MethodologyApprovalStatus.Approved,
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology 2 title",
                    Status = MethodologyApprovalStatus.Draft,
                }
            );

            var genericContentSection = new ContentSection
            {
                Heading = "Test section 1",
                Type = ContentSectionType.Generic,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Order = 0,
                        Body = "Test block 1"
                    },
                    inContentDataBlockParent.LatestPublishedVersion!.ContentBlock
                },
                ReleaseVersion = releaseVersion
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion, otherReleaseVersion);
                contentDbContext.DataBlockParents.AddRange(
                    unattachedDataBlockParent, keyStatDataBlockParent, inContentDataBlockParent);
                contentDbContext.ContentSections.AddRange(
                    new ContentSection
                    {
                        ReleaseVersion = releaseVersion,
                        Type = ContentSectionType.Headlines
                    },
                    new ContentSection
                    {
                        ReleaseVersion = releaseVersion,
                        Type = ContentSectionType.KeyStatisticsSecondary
                    },
                    new ContentSection
                    {
                        ReleaseVersion = releaseVersion,
                        Type = ContentSectionType.ReleaseSummary
                    },
                    new ContentSection
                    {
                        ReleaseVersion = releaseVersion,
                        Type = ContentSectionType.RelatedDashboards
                    },
                    genericContentSection);

                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            dataBlockService.Setup(mock =>
                    mock.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(unattachedDataBlocks);

            methodologyVersionRepository.Setup(mock =>
                    mock.GetLatestVersionByPublication(publication.Id))
                .ReturnsAsync(methodologies);

            releaseFileService.Setup(mock =>
                    mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data))
                .ReturnsAsync(files);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(releaseVersion.Id);

                var viewModel = result.AssertRight();

                dataBlockService.Verify(mock =>
                    mock.GetUnattachedDataBlocks(releaseVersion.Id), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data), Times.Once);

                Assert.Equal(unattachedDataBlocks, viewModel.UnattachedDataBlocks);

                var contentRelease = viewModel.Release;

                Assert.NotNull(contentRelease);
                Assert.Equal(releaseVersion.Id, contentRelease.Id);
                Assert.Equal("Academic year", contentRelease.CoverageTitle);
                Assert.True(contentRelease.HasDataGuidance);
                Assert.True(contentRelease.HasPreReleaseAccessList);

                Assert.Equal(2, contentRelease.KeyStatistics.Count);
                Assert.Equal(releaseVersion.KeyStatistics[1].Id, contentRelease.KeyStatistics[0].Id);
                Assert.Equal(0, contentRelease.KeyStatistics[0].Order);
                var originalKeyStatDataBlock = (releaseVersion.KeyStatistics[1] as KeyStatisticDataBlock)!;
                var keyStatDataBlockViewModel = Assert.IsType<KeyStatisticDataBlockViewModel>(contentRelease.KeyStatistics[0]);
                Assert.Equal(originalKeyStatDataBlock.DataBlockId, keyStatDataBlockViewModel.DataBlockId);
                Assert.Equal(
                    keyStatDataBlockParent.LatestPublishedVersion!.DataBlockParentId,
                    keyStatDataBlockViewModel.DataBlockParentId);

                Assert.Equal(releaseVersion.KeyStatistics[0].Id, contentRelease.KeyStatistics[1].Id);
                Assert.Equal(1, contentRelease.KeyStatistics[1].Order);
                Assert.IsType<KeyStatisticTextViewModel>(contentRelease.KeyStatistics[1]);

                Assert.Equal(releaseVersion.KeyStatisticsSecondarySection.Id,
                    contentRelease.KeyStatisticsSecondarySection.Id);
                Assert.Equal(releaseVersion.HeadlinesSection.Id, contentRelease.HeadlinesSection.Id);
                Assert.Equal(releaseVersion.RelatedDashboardsSection.Id,
                    contentRelease.RelatedDashboardsSection.Id);
                Assert.True(contentRelease.LatestRelease);
                Assert.Equal("9", contentRelease.NextReleaseDate.Day);
                Assert.Equal("9", contentRelease.NextReleaseDate.Month);
                Assert.Equal("2040", contentRelease.NextReleaseDate.Year);
                Assert.Equal("2020", contentRelease.ReleaseName);
                Assert.NotNull(contentRelease.Published);
                Assert.InRange(DateTime.UtcNow.Subtract(contentRelease.Published!.Value).Milliseconds,
                    0, 1500);
                Assert.Equal(publication.Id, contentRelease.PublicationId);
                Assert.Equal(DateTime.Parse("2020-09-09T00:00:00.00"), contentRelease.PublishScheduled);
                Assert.Equal("2020-21", contentRelease.Slug);
                Assert.Equal(releaseVersion.SummarySection.Id, contentRelease.SummarySection.Id);
                Assert.Equal("Academic year 2020/21", contentRelease.Title);
                Assert.Equal(ReleaseType.OfficialStatistics, contentRelease.Type);
                Assert.Equal("2020/21", contentRelease.YearTitle);
                Assert.Empty(contentRelease.Updates);

                var contentDownloadFiles = contentRelease.DownloadFiles.ToList();
                Assert.Equal(2, contentDownloadFiles.Count);
                Assert.Equal(files[0].Id, contentDownloadFiles[0].Id);
                Assert.Equal("pdf", contentDownloadFiles[0].Extension);
                Assert.Equal("ancillary.pdf", contentDownloadFiles[0].FileName);
                Assert.Equal("Ancillary File", contentDownloadFiles[0].Name);
                Assert.Equal("10 Kb", contentDownloadFiles[0].Size);
                Assert.Equal(Ancillary, contentDownloadFiles[0].Type);
                Assert.Equal(files[1].Id, contentDownloadFiles[1].Id);
                Assert.Equal("csv", contentDownloadFiles[1].Extension);
                Assert.Equal("data.csv", contentDownloadFiles[1].FileName);
                Assert.Equal("Subject File", contentDownloadFiles[1].Name);
                Assert.Equal("20 Kb", contentDownloadFiles[1].Size);
                Assert.Equal(FileType.Data, contentDownloadFiles[1].Type);

                var contentRelatedInformation = contentRelease.RelatedInformation;
                Assert.Single(contentRelatedInformation);
                Assert.Equal("Related 1", contentRelatedInformation[0].Description);
                Assert.Equal("https://related-1", contentRelatedInformation[0].Url);

                var contentSections = contentRelease.Content;
                Assert.Single(contentSections);
                Assert.Equal(genericContentSection.Id, contentSections[0].Id);
                Assert.Equal("Test section 1", contentSections[0].Heading);

                Assert.Equal(2, contentSections[0].Content.Count);

                var htmlBlockViewModel = Assert.IsType<HtmlBlockViewModel>(contentSections[0].Content[0]);
                Assert.Equal("Test block 1", htmlBlockViewModel.Body);

                var dataBlockViewModel = Assert.IsType<DataBlockViewModel>(contentSections[0].Content[1]);
                Assert.Equal(inContentDataBlockVersionId, dataBlockViewModel.Id);
                Assert.Equal(
                    inContentDataBlockParent.LatestPublishedVersion!.DataBlockParentId,
                    dataBlockViewModel.DataBlockParentId);

                var contentPublication = contentRelease.Publication;

                Assert.NotNull(contentPublication);
                Assert.Equal(publication.Id, contentPublication.Id);
                Assert.Equal(publication.Contact.Id, contentPublication.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, contentPublication.Contact.ContactName);
                Assert.Equal(publication.Contact.TeamEmail, contentPublication.Contact.TeamEmail);
                Assert.Equal(publication.Contact.ContactTelNo, contentPublication.Contact.ContactTelNo);
                Assert.Null(contentPublication.ExternalMethodology);
                Assert.Equal("test-publication", contentPublication.Slug);
                Assert.Equal("Publication", contentPublication.Title);
                Assert.Equal("Theme", contentPublication.Topic.Theme.Title);
                Assert.NotNull(contentPublication.Topic);
                Assert.NotNull(contentPublication.Topic.Theme);

                var contentPublicationReleaseSeries = contentPublication.ReleaseSeries;
                Assert.Equal(3, contentPublicationReleaseSeries.Count);

                Assert.False(contentPublicationReleaseSeries[0].IsLegacyLink);
                Assert.Equal(releaseId, contentPublicationReleaseSeries[0].ReleaseId);
                Assert.Equal(releaseVersion.Slug, contentPublicationReleaseSeries[0].ReleaseSlug);
                Assert.Equal(releaseVersion.Title, contentPublicationReleaseSeries[0].Description);
                Assert.Null(contentPublicationReleaseSeries[0].LegacyLinkUrl);

                Assert.True(contentPublicationReleaseSeries[1].IsLegacyLink);
                Assert.Null(contentPublicationReleaseSeries[1].ReleaseId);
                Assert.Null(contentPublicationReleaseSeries[1].ReleaseSlug);
                Assert.Equal("Legacy 2018/19", contentPublicationReleaseSeries[1].Description);
                Assert.Equal("https://legacy-2018-19", contentPublicationReleaseSeries[1].LegacyLinkUrl);

                Assert.True(contentPublicationReleaseSeries[2].IsLegacyLink);
                Assert.Null(contentPublicationReleaseSeries[2].ReleaseId);
                Assert.Null(contentPublicationReleaseSeries[2].ReleaseSlug);
                Assert.Equal("Legacy 2017/18", contentPublicationReleaseSeries[2].Description);
                Assert.Equal("https://legacy-2017-18", contentPublicationReleaseSeries[2].LegacyLinkUrl);

                var contentPublicationReleases = contentPublication.Releases;
                Assert.Single(contentPublicationReleases);
                Assert.Equal(otherReleaseVersion.Id, contentPublicationReleases[0].Id);
                Assert.Equal("2019-20", contentPublicationReleases[0].Slug);
                Assert.Equal("Academic year 2019/20", contentPublicationReleases[0].Title);

                Assert.Equal(2, contentPublication.Methodologies.Count);
                Assert.Equal(methodologies[0].Id, contentPublication.Methodologies[0].Id);
                Assert.Equal("Methodology 1 title", contentPublication.Methodologies[0].Title);
                Assert.Equal(methodologies[1].Id, contentPublication.Methodologies[1].Id);
                Assert.Equal("Methodology 2 title", contentPublication.Methodologies[1].Title);
            }

            MockUtils.VerifyAllMocks(dataBlockService, methodologyVersionRepository, releaseFileService);
        }

        [Fact]
        public async Task GetManageContentPageViewModel_IsPrerelease()
        {
            var publication = new Publication
            {
                Contact = new Contact(),
                Slug = "test-publication",
                Title = "Publication",
                Topic = new Topic
                {
                    Theme = new Theme(),
                }
            };

            var releaseVersion = new ReleaseVersion
            {
                NextReleaseDate = new PartialDate {Day = "9", Month = "9", Year = "2040"},
                PreReleaseAccessList = "Test access list",
                Publication = publication,
                PublishScheduled = DateTime.Parse("2020-09-08T23:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = null,
                ReleaseName = "2020",
                Slug = "2020-21",
                TimePeriodCoverage = AcademicYear,
                Type = ReleaseType.OfficialStatistics,
            };

            var previousMethodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 3 title",
                // Previous versions should always be approved - so no status set
            };

            var methodologyVersions = AsList(
                new MethodologyVersion
                {
                    // in result because approved
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology 1 title",
                    Status = MethodologyApprovalStatus.Approved,
                },
                new MethodologyVersion
                {
                    // in result because approved
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology 2 title",
                    Status = MethodologyApprovalStatus.Approved,
                },
                previousMethodologyVersion, // in result because amendment of this version is not Approved
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology should be filtered 1",
                    Status = MethodologyApprovalStatus.Draft,
                    PreviousVersion = previousMethodologyVersion,
                },
                new MethodologyVersion
                {
                    // not in result because not Approved and no previous version
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology should be filtered 2",
                    Status = MethodologyApprovalStatus.HigherLevelReview,
                    PreviousVersion = null,
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.MethodologyVersions.AddRange(methodologyVersions);
                contentDbContext.ContentSections.AddRange(
                    new()
                    {
                        Type = ContentSectionType.Headlines,
                        ReleaseVersion = releaseVersion
                    },
                    new()
                    {
                        Type = ContentSectionType.KeyStatisticsSecondary,
                        ReleaseVersion = releaseVersion
                    },
                    new()
                    {
                        Type = ContentSectionType.ReleaseSummary,
                        ReleaseVersion = releaseVersion
                    },
                    new()
                    {
                        Type = ContentSectionType.RelatedDashboards,
                        ReleaseVersion = releaseVersion
                    },
                    new()
                    {
                        Type = ContentSectionType.Generic,
                        ReleaseVersion = releaseVersion
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            dataBlockService.Setup(mock =>
                    mock.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(new List<DataBlockViewModel>());

            methodologyVersionRepository.Setup(mock =>
                    mock.GetLatestVersionByPublication(publication.Id))
                .ReturnsAsync(methodologyVersions);

            releaseFileService.Setup(mock =>
                    mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data))
                .ReturnsAsync(new List<FileInfo>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(
                    releaseVersion.Id, isPrerelease: true);

                var viewModel = result.AssertRight();

                var contentRelease = viewModel.Release;

                var contentPublication = contentRelease.Publication;
                Assert.NotNull(contentPublication);

                Assert.Equal(3, contentPublication.Methodologies.Count);
                Assert.Equal(methodologyVersions[0].Id, contentPublication.Methodologies[0].Id);
                Assert.Equal("Methodology 1 title", contentPublication.Methodologies[0].Title);
                Assert.Equal(methodologyVersions[1].Id, contentPublication.Methodologies[1].Id);
                Assert.Equal("Methodology 2 title", contentPublication.Methodologies[1].Title);
                Assert.Equal(methodologyVersions[2].Id, contentPublication.Methodologies[2].Id);
                Assert.Equal("Methodology 3 title", contentPublication.Methodologies[2].Title);
            }

            MockUtils.VerifyAllMocks(dataBlockService, methodologyVersionRepository, releaseFileService);
        }

        [Fact]
        public async Task GetManageContentPageViewModel_MapsBlocksCorrectly()
        {
            var user1 = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com"
            };
            var user2 = new User
            {
                FirstName = "Rob",
                LastName = "Rowe",
                Email = "rob@test.com"
            };

            var publication = new Publication
            {
                Contact = new Contact
                {
                    ContactName = "Name",
                    ContactTelNo = "01234 567890",
                    TeamEmail = "test@test.com",
                    TeamName = "Team Name"
                },
                Slug = "test-publication",
                Title = "Publication",
                Topic = new Topic
                {
                    Theme = new Theme
                    {
                        Title = "Theme"
                    }
                }
            };

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2020",
                Slug = "2020-21",
                TimePeriodCoverage = AcademicYear,
                Type = ReleaseType.OfficialStatistics,
            };

            var summaryContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion,
                Type = ContentSectionType.ReleaseSummary,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = "Test block 1",
                        Comments = ListOf(
                            new Comment
                            {
                                Content = "Comment 1",
                                Created = DateTime.Parse("2022-03-16T12:00:00Z"),
                                CreatedBy = user1
                            },
                            new Comment
                            {
                                Content = "Comment 2",
                                Created = DateTime.Parse("2022-03-12T12:00:00Z"),
                                CreatedBy = user2,
                                Resolved = DateTime.Parse("2022-03-14T12:00:00Z"),
                                ResolvedBy = user1
                            }
                        )
                    }
                }
            };
            var genericContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion,
                Heading = "Test section 1",
                Type = ContentSectionType.Generic,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = "Test block 2",
                        Locked = DateTime.Parse("2022-03-16T12:00:00Z"),
                        LockedBy = user1,
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ContentSections.AddRange(summaryContentSection, genericContentSection);

                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            dataBlockService.Setup(mock =>
                    mock.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(new List<DataBlockViewModel>());

            methodologyVersionRepository.Setup(mock =>
                    mock.GetLatestVersionByPublication(publication.Id))
                .ReturnsAsync(new List<MethodologyVersion>());

            releaseFileService.Setup(mock =>
                    mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data))
                .ReturnsAsync(new Either<ActionResult, IEnumerable<FileInfo>>(new List<FileInfo>()));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(
                    contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(releaseVersion.Id);

                var viewModel = result.AssertRight();

                dataBlockService.Verify(mock =>
                    mock.GetUnattachedDataBlocks(releaseVersion.Id), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data), Times.Once);

                var contentRelease = viewModel.Release;

                var contentReleaseSummary = contentRelease.SummarySection;

                Assert.Equal(summaryContentSection.Id, contentReleaseSummary.Id);
                Assert.Null(summaryContentSection.Heading);
                Assert.Single(summaryContentSection.Content);

                var summarySectionBlock = Assert.IsType<HtmlBlockViewModel>(contentReleaseSummary.Content[0]);
                Assert.Equal(summaryContentSection.Content[0].Id, summarySectionBlock.Id);
                Assert.Equal("Test block 1", summarySectionBlock.Body);

                Assert.Equal(2, summarySectionBlock.Comments.Count);

                // Comments are ordered in ascending order by created date (oldest first)

                Assert.Equal("Comment 2", summarySectionBlock.Comments[0].Content);

                Assert.Equal(DateTime.Parse("2022-03-12T12:00:00Z"), summarySectionBlock.Comments[0].Created);
                Assert.Equal(user2.Id, summarySectionBlock.Comments[0].CreatedBy.Id);
                Assert.Equal("Rob Rowe", summarySectionBlock.Comments[0].CreatedBy.DisplayName);
                Assert.Equal("rob@test.com", summarySectionBlock.Comments[0].CreatedBy.Email);

                Assert.Equal(DateTime.Parse("2022-03-14T12:00:00Z"), summarySectionBlock.Comments[0].Resolved);
                Assert.Equal(user1.Id, summarySectionBlock.Comments[0].ResolvedBy.Id);
                Assert.Equal("Jane Doe", summarySectionBlock.Comments[0].ResolvedBy.DisplayName);
                Assert.Equal("jane@test.com", summarySectionBlock.Comments[0].ResolvedBy.Email);

                Assert.Equal("Comment 1", summarySectionBlock.Comments[1].Content);

                Assert.Equal(DateTime.Parse("2022-03-16T12:00:00Z"), summarySectionBlock.Comments[1].Created);
                Assert.Equal(user1.Id, summarySectionBlock.Comments[1].CreatedBy.Id);
                Assert.Equal("Jane Doe", summarySectionBlock.Comments[1].CreatedBy.DisplayName);
                Assert.Equal("jane@test.com", summarySectionBlock.Comments[1].CreatedBy.Email);

                var contentReleaseContent = contentRelease.Content;
                Assert.Single(contentReleaseContent);
                Assert.Equal(genericContentSection.Id, contentReleaseContent[0].Id);
                Assert.Equal("Test section 1", contentReleaseContent[0].Heading);

                Assert.Single(contentReleaseContent[0].Content);

                var contentBlock = Assert.IsType<HtmlBlockViewModel>(contentReleaseContent[0].Content[0]);
                Assert.Equal("Test block 2", contentBlock.Body);

                Assert.Equal(DateTime.Parse("2022-03-16T12:00:00Z"), contentBlock.Locked);
                Assert.Equal(user1.Id, contentBlock.LockedBy!.Id);
                Assert.Equal("Jane Doe", contentBlock.LockedBy!.DisplayName);
                Assert.Equal("jane@test.com", contentBlock.LockedBy!.Email);
            }

            MockUtils.VerifyAllMocks(dataBlockService, methodologyVersionRepository, releaseFileService);
        }

        private static ManageContentPageService SetupManageContentPageService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IMapper? mapper = null,
            IDataBlockService? dataBlockService = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.AdminMapper(contentDbContext),
                dataBlockService ?? new Mock<IDataBlockService>().Object,
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
