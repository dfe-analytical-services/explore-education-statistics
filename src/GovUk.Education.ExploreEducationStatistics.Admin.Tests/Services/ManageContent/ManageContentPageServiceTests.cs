﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

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
                DataSource = "Data Source",
                Description = "Description",
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Legacy 2017/18",
                        Order = 0,
                        Url = "http://legacy-2017-18"
                    },
                    new LegacyRelease
                    {
                        Description = "Legacy 2018/19",
                        Order = 1,
                        Url = "http://legacy-2018-19"
                    },
                },
                Methodology = new Methodology
                {
                    Title = "Methodology"
                },
                Slug = "test-publication",
                Summary = "Summary",
                Title = "Publication",
                Topic = new Topic
                {
                    Theme = new Theme
                    {
                        Title = "Theme"
                    }
                }
            };

            var releaseType = new ReleaseType
            {
                Title = "Release Type"
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
                    new Link
                    {
                        Description = "Related 1",
                        Url = "http://related-1"
                    }
                },
                Slug = "2020-21",
                TimePeriodCoverage = AcademicYear,
                Type = releaseType,
                Updates = new List<Update>()
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
                Type = releaseType
            };

            var availableDataBlocks = new List<DataBlock>
            {
                new DataBlock()
            };

            var ancillaryFileId = Guid.NewGuid();
            var dataFileId = Guid.NewGuid();
            var files = new List<FileInfo>
            {
                new FileInfo
                {
                    Id = ancillaryFileId,
                    FileName = "ancillary.pdf",
                    Name = "Ancillary File",
                    Path = AdminReleasePath(release.Id, ReleaseFileTypes.Ancillary, ancillaryFileId),
                    Size = "10 Kb",
                    Type = ReleaseFileTypes.Ancillary
                },
                new FileInfo
                {
                    Id = dataFileId,
                    FileName = "data.csv",
                    Name = "Subject File",
                    Path = AdminReleasePath(release.Id, ReleaseFileTypes.Data, dataFileId),
                    Size = "20 Kb",
                    Type = ReleaseFileTypes.Data
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
                        ContentSection = new ContentSection
                        {
                            Type = ContentSectionType.Headlines
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ContentSection
                        {
                            Type = ContentSectionType.KeyStatistics
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ContentSection
                        {
                            Type = ContentSectionType.KeyStatisticsSecondary
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ContentSection
                        {
                            Type = ContentSectionType.ReleaseSummary
                        }
                    },
                    new ReleaseContentSection
                    {
                        Release = release,
                        ContentSection = new ContentSection
                        {
                            Type = ContentSectionType.Generic
                        }
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            contentService.Setup(mock =>
                    mock.GetUnattachedContentBlocksAsync<DataBlock>(release.Id))
                .ReturnsAsync(availableDataBlocks);

            releaseFileService.Setup(mock =>
                    mock.ListAll(release.Id, ReleaseFileTypes.Ancillary, ReleaseFileTypes.Data))
                .ReturnsAsync(files);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupManageContentPageService(contentDbContext: contentDbContext,
                    contentService: contentService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await service.GetManageContentPageViewModel(release.Id);

                Assert.True(result.IsRight);

                contentService.Verify(mock =>
                    mock.GetUnattachedContentBlocksAsync<DataBlock>(release.Id), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.ListAll(release.Id, ReleaseFileTypes.Ancillary, ReleaseFileTypes.Data), Times.Once);

                Assert.Equal(availableDataBlocks, result.Right.AvailableDataBlocks);

                var contentRelease = result.Right.Release;

                Assert.NotNull(contentRelease);
                Assert.Equal(release.Id, contentRelease.Id);
                Assert.Equal("Academic Year", contentRelease.CoverageTitle);
                Assert.NotNull(contentRelease.DataLastPublished);
                Assert.True(contentRelease.HasMetaGuidance);
                Assert.True(contentRelease.HasPreReleaseAccessList);
                Assert.Equal(release.HeadlinesSection.Id, contentRelease.HeadlinesSection.Id);
                Assert.Equal(release.KeyStatisticsSection.Id, contentRelease.KeyStatisticsSection.Id);
                Assert.Equal(release.KeyStatisticsSecondarySection.Id, contentRelease.KeyStatisticsSecondarySection.Id);
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
                Assert.Equal("Academic Year 2020/21", contentRelease.Title);
                Assert.Equal(release.Type, contentRelease.Type);
                Assert.Equal("2020/21", contentRelease.YearTitle);
                Assert.Empty(contentRelease.Updates);

                var contentDownloadFiles = contentRelease.DownloadFiles.ToList();
                Assert.Equal(2, contentDownloadFiles.Count);
                Assert.Equal(files[0].Id, contentDownloadFiles[0].Id);
                Assert.Equal("pdf", contentDownloadFiles[0].Extension);
                Assert.Equal("ancillary.pdf", contentDownloadFiles[0].FileName);
                Assert.Equal("Ancillary File", contentDownloadFiles[0].Name);
                Assert.Equal(AdminReleasePath(release.Id, ReleaseFileTypes.Ancillary, ancillaryFileId),
                    contentDownloadFiles[0].Path);
                Assert.Equal("10 Kb", contentDownloadFiles[0].Size);
                Assert.Equal(ReleaseFileTypes.Ancillary, contentDownloadFiles[0].Type);
                Assert.Equal(files[1].Id, contentDownloadFiles[1].Id);
                Assert.Equal("csv", contentDownloadFiles[1].Extension);
                Assert.Equal("data.csv", contentDownloadFiles[1].FileName);
                Assert.Equal("Subject File", contentDownloadFiles[1].Name);
                Assert.Equal(AdminReleasePath(release.Id, ReleaseFileTypes.Data, dataFileId),
                    contentDownloadFiles[1].Path);
                Assert.Equal("20 Kb", contentDownloadFiles[1].Size);
                Assert.Equal(ReleaseFileTypes.Data, contentDownloadFiles[1].Type);

                var contentRelatedInformation = contentRelease.RelatedInformation;
                Assert.Single(contentRelatedInformation);
                Assert.Equal("Related 1", contentRelatedInformation[0].Description);
                Assert.Equal("http://related-1", contentRelatedInformation[0].Url);

                var contentReleaseContent = contentRelease.Content;
                Assert.Single(contentReleaseContent);
                Assert.Equal(release.GenericContent.First().Id, contentReleaseContent[0].Id);

                var contentPublication = contentRelease.Publication;

                Assert.NotNull(contentPublication);
                Assert.Equal(publication.Id, contentPublication.Id);
                Assert.Equal(publication.Contact.Id, contentPublication.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, contentPublication.Contact.ContactName);
                Assert.Equal(publication.Contact.TeamEmail, contentPublication.Contact.TeamEmail);
                Assert.Equal(publication.Contact.ContactTelNo, contentPublication.Contact.ContactTelNo);
                Assert.Equal("Data Source", contentPublication.DataSource);
                Assert.Equal("Description", contentPublication.Description);
                Assert.Null(contentPublication.ExternalMethodology);
                Assert.Equal(publication.Methodology.Id, contentPublication.Methodology.Id);
                Assert.Equal("Methodology", contentPublication.Methodology.Title);
                Assert.Equal("test-publication", contentPublication.Slug);
                Assert.Equal("Summary", contentPublication.Summary);
                Assert.Equal("Publication", contentPublication.Title);
                Assert.Equal("Theme", contentPublication.Topic.Theme.Title);
                Assert.NotNull(contentPublication.Topic);
                Assert.NotNull(contentPublication.Topic.Theme);

                var contentPublicationLegacyReleases = contentPublication.LegacyReleases.ToList();
                Assert.Equal(2, contentPublicationLegacyReleases.Count);
                Assert.Equal("Legacy 2018/19", contentPublicationLegacyReleases[0].Description);
                Assert.Equal("http://legacy-2018-19", contentPublicationLegacyReleases[0].Url);
                Assert.Equal("Legacy 2017/18", contentPublicationLegacyReleases[1].Description);
                Assert.Equal("http://legacy-2017-18", contentPublicationLegacyReleases[1].Url);

                var contentPublicationOtherReleases = contentPublication.OtherReleases;
                Assert.Single(contentPublicationOtherReleases);
                Assert.Equal(otherRelease.Id, contentPublicationOtherReleases[0].Id);
                Assert.Equal("2019-20", contentPublicationOtherReleases[0].Slug);
                Assert.Equal("Academic Year 2019/20", contentPublicationOtherReleases[0].Title);
            }
        }

        private static ManageContentPageService SetupManageContentPageService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IContentService contentService = null,
            IMapper mapper = null,
            IReleaseFileService releaseFileService = null,
            IUserService userService = null)
        {
            return new ManageContentPageService(
                mapper ?? MapperUtils.AdminMapper(),
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                contentService ?? new Mock<IContentService>().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}