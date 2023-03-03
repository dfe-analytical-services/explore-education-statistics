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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ManageContentPageServiceTests
    {
        [Fact]
        public async Task GetManageContentPageViewModel()
        {
            var publication = new Publication
            {
                Contact = new Contact
                {
                    ContactName = "Name",
                    ContactTelNo = "01234 567890",
                    TeamEmail = "test@test.com",
                    TeamName = "Team Name"
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Legacy 2017/18",
                        Order = 0,
                        Url = "https://legacy-2017-18"
                    },
                    new()
                    {
                        Description = "Legacy 2018/19",
                        Order = 1,
                        Url = "https://legacy-2018-19"
                    }
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

            var release = new Release
            {
                Id = Guid.NewGuid(),
                NextReleaseDate = new PartialDate {Day = "9", Month = "9", Year = "2040"},
                PreReleaseAccessList = "Test access list",
                Publication = publication,
                PublishScheduled = DateTime.Parse("2020-09-08T23:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = null,
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
                KeyStatistics = new List<KeyStatistic>
                {
                    new KeyStatisticText { Order = 1 },
                    new KeyStatisticDataBlock
                    {
                        Order = 0,
                        DataBlockId = Guid.NewGuid(),
                    },
                },
            };

            var otherRelease = new Release
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

            var unattachedDataBlocks = new List<Admin.ViewModels.DataBlockViewModel>
            {
                new()
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
                    AlternativeTitle = "Methodology 1 title"
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    AlternativeTitle = "Methodology 2 title"
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
                        Body = "Test block 1"
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.AddRangeAsync(release, otherRelease);
                await contentDbContext.ReleaseContentSections.AddRangeAsync(
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ()
                        {
                            Type = ContentSectionType.Headlines
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ()
                        {
                            Type = ContentSectionType.KeyStatisticsSecondary
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ()
                        {
                            Type = ContentSectionType.ReleaseSummary
                        }
                    },
                    new ()
                    {
                        Release = release,
                        ContentSection = new ()
                        {
                            Type = ContentSectionType.RelatedDashboards
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = genericContentSection
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            dataBlockService.Setup(mock =>
                    mock.GetUnattachedDataBlocks(release.Id))
                .ReturnsAsync(unattachedDataBlocks);

            methodologyVersionRepository.Setup(mock =>
                    mock.GetLatestVersionByPublication(publication.Id))
                .ReturnsAsync(methodologies);

            releaseFileService.Setup(mock =>
                    mock.ListAll(release.Id, Ancillary, FileType.Data))
                .ReturnsAsync(files);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(release.Id);

                Assert.True(result.IsRight);

                dataBlockService.Verify(mock =>
                    mock.GetUnattachedDataBlocks(release.Id), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.ListAll(release.Id, Ancillary, FileType.Data), Times.Once);

                Assert.Equal(unattachedDataBlocks, result.Right.UnattachedDataBlocks);

                var contentRelease = result.Right.Release;

                Assert.NotNull(contentRelease);
                Assert.Equal(release.Id, contentRelease.Id);
                Assert.Equal("Academic year", contentRelease.CoverageTitle);
                Assert.True(contentRelease.HasDataGuidance);
                Assert.True(contentRelease.HasPreReleaseAccessList);

                Assert.Equal(2, contentRelease.KeyStatistics.Count);
                Assert.Equal(release.KeyStatistics[1].Id, contentRelease.KeyStatistics[0].Id);
                Assert.Equal(0, contentRelease.KeyStatistics[0].Order);
                var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlockViewModel>(contentRelease.KeyStatistics[0]);
                Assert.Equal(
                    (release.KeyStatistics[1] as KeyStatisticDataBlock)!.DataBlockId,
                    keyStatDataBlock.DataBlockId);

                Assert.Equal(release.KeyStatistics[0].Id, contentRelease.KeyStatistics[1].Id);
                Assert.Equal(1, contentRelease.KeyStatistics[1].Order);
                Assert.IsType<KeyStatisticTextViewModel>(contentRelease.KeyStatistics[1]);

                Assert.Equal(release.KeyStatisticsSecondarySection.Id,
                    contentRelease.KeyStatisticsSecondarySection.Id);
                Assert.Equal(release.HeadlinesSection.Id, contentRelease.HeadlinesSection.Id);
                Assert.Equal(release.RelatedDashboardsSection.Id,
                    contentRelease.RelatedDashboardsSection.Id);
                Assert.False(contentRelease.LatestRelease);
                Assert.Equal("9", contentRelease.NextReleaseDate.Day);
                Assert.Equal("9", contentRelease.NextReleaseDate.Month);
                Assert.Equal("2040", contentRelease.NextReleaseDate.Year);
                Assert.Equal("2020", contentRelease.ReleaseName);
                Assert.Null(contentRelease.Published);
                Assert.Equal(publication.Id, contentRelease.PublicationId);
                Assert.Equal(DateTime.Parse("2020-09-09T00:00:00.00"), contentRelease.PublishScheduled);
                Assert.Equal("2020-21", contentRelease.Slug);
                Assert.Equal(release.SummarySection.Id, contentRelease.SummarySection.Id);
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

                var contentReleaseContent = contentRelease.Content;
                Assert.Single(contentReleaseContent);
                Assert.Equal(genericContentSection.Id, contentReleaseContent[0].Id);
                Assert.Equal("Test section 1", contentReleaseContent[0].Heading);

                Assert.Single(contentReleaseContent[0].Content);

                var contentBlock = Assert.IsType<HtmlBlockViewModel>(contentReleaseContent[0].Content[0]);
                Assert.Equal("Test block 1", contentBlock.Body);

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

                var contentPublicationLegacyReleases = contentPublication.LegacyReleases;
                Assert.Equal(2, contentPublicationLegacyReleases.Count);
                Assert.Equal("Legacy 2018/19", contentPublicationLegacyReleases[0].Description);
                Assert.Equal("https://legacy-2018-19", contentPublicationLegacyReleases[0].Url);
                Assert.Equal("Legacy 2017/18", contentPublicationLegacyReleases[1].Description);
                Assert.Equal("https://legacy-2017-18", contentPublicationLegacyReleases[1].Url);

                var contentPublicationReleases = contentPublication.Releases;
                Assert.Single(contentPublicationReleases);
                Assert.Equal(otherRelease.Id, contentPublicationReleases[0].Id);
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

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2020",
                Slug = "2020-21",
                TimePeriodCoverage = AcademicYear,
                Type = ReleaseType.OfficialStatistics,
            };

            var unattachedDataBlocks = new List<Admin.ViewModels.DataBlockViewModel>
            {
                new()
            };

            var summaryContentSection = new ContentSection
            {
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
                await contentDbContext.AddAsync(publication);
                await contentDbContext.AddRangeAsync(release);
                await contentDbContext.ReleaseContentSections.AddRangeAsync(
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = summaryContentSection
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = genericContentSection
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            dataBlockService.Setup(mock =>
                    mock.GetUnattachedDataBlocks(release.Id))
                .ReturnsAsync(unattachedDataBlocks);

            methodologyVersionRepository.Setup(mock =>
                    mock.GetLatestVersionByPublication(publication.Id))
                .ReturnsAsync(new List<MethodologyVersion>());

            releaseFileService.Setup(mock =>
                    mock.ListAll(release.Id, Ancillary, FileType.Data))
                .ReturnsAsync(new Either<ActionResult, IEnumerable<FileInfo>>(new List<FileInfo>()));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(
                    contentDbContext: contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(release.Id);

                Assert.True(result.IsRight);

                dataBlockService.Verify(mock =>
                    mock.GetUnattachedDataBlocks(release.Id), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.ListAll(release.Id, Ancillary, FileType.Data), Times.Once);

                Assert.Equal(unattachedDataBlocks, result.Right.UnattachedDataBlocks);

                var contentRelease = result.Right.Release;

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
            IUserService? userService = null)
        {
            return new(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.AdminMapper(),
                dataBlockService ?? new Mock<IDataBlockService>().Object,
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
