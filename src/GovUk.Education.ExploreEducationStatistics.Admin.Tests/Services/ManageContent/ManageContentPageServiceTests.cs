#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ManageContentPageServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetManageContentPageViewModel()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithTheme(_dataFixture.DefaultTheme())
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .WithLegacyLinks(_dataFixture.DefaultLegacyReleaseSeriesItem().Generate(2));

        var releaseVersion = publication.Releases.Single().Versions.Single();

        releaseVersion.PublishingOrganisations = _dataFixture.DefaultOrganisation().GenerateList(2);

        releaseVersion.RelatedInformation.Add(new Link { Description = "Related 1", Url = "https://related-1" });

        var unattachedDataBlockParent = new DataBlockParent
        {
            LatestPublishedVersion = new DataBlockVersion { ReleaseVersionId = releaseVersion.Id, Id = Guid.NewGuid() },
        };

        var keyStatDataBlockParent = new DataBlockParent
        {
            LatestPublishedVersion = new DataBlockVersion { ReleaseVersionId = releaseVersion.Id, Id = Guid.NewGuid() },
        };

        var inContentDataBlockVersionId = Guid.NewGuid();

        var inContentDataBlockParent = new DataBlockParent
        {
            LatestPublishedVersion = new DataBlockVersion
            {
                ReleaseVersionId = releaseVersion.Id,
                Id = inContentDataBlockVersionId,
                ContentBlock = new DataBlock { Id = inContentDataBlockVersionId, Order = 1 },
            },
        };

        releaseVersion.KeyStatistics =
        [
            new KeyStatisticText { Order = 1 },
            new KeyStatisticDataBlock { Order = 0, DataBlockId = keyStatDataBlockParent.LatestPublishedVersion!.Id },
        ];

        List<DataBlockViewModel> unattachedDataBlocks =
        [
            new() { Id = unattachedDataBlockParent.LatestPublishedVersion!.Id },
        ];

        var files = _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFiles([_dataFixture.DefaultFile(Ancillary), _dataFixture.DefaultFile(FileType.Data)])
            .Generate(2)
            .Select(rf => rf.ToFileInfo())
            .ToList();

        var methodology = _dataFixture
            .DefaultMethodology()
            .WithMethodologyVersions(
                _dataFixture
                    .DefaultMethodologyVersion()
                    .ForIndex(0, mv => mv.SetApprovalStatus(MethodologyApprovalStatus.Approved))
                    .ForIndex(1, mv => mv.SetAlternativeTitle("Alternative title"))
                    .Generate(2)
            )
            .FinishWith(m => m.LatestPublishedVersion = m.Versions[0])
            .WithOwningPublication(publication)
            .Generate();

        var genericContentSection = new ContentSection
        {
            Heading = "Test section 1",
            Type = ContentSectionType.Generic,
            Content =
            [
                new HtmlBlock { Order = 0, Body = "Test block 1" },
                inContentDataBlockParent.LatestPublishedVersion!.ContentBlock,
            ],
            ReleaseVersion = releaseVersion,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.DataBlockParents.AddRange(
                unattachedDataBlockParent,
                keyStatDataBlockParent,
                inContentDataBlockParent
            );
            contentDbContext.ContentSections.AddRange(
                new ContentSection { ReleaseVersion = releaseVersion, Type = ContentSectionType.Headlines },
                new ContentSection
                {
                    ReleaseVersion = releaseVersion,
                    Type = ContentSectionType.KeyStatisticsSecondary,
                },
                new ContentSection { ReleaseVersion = releaseVersion, Type = ContentSectionType.ReleaseSummary },
                new ContentSection { ReleaseVersion = releaseVersion, Type = ContentSectionType.RelatedDashboards },
                genericContentSection
            );

            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

        dataBlockService
            .Setup(mock => mock.GetUnattachedDataBlocks(releaseVersion.Id))
            .ReturnsAsync(unattachedDataBlocks);

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestVersionByPublication(publication.Id))
            .ReturnsAsync(methodology.Versions);

        releaseFileService.Setup(mock => mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data)).ReturnsAsync(files);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupManageContentPageService(
                contentDbContext: contentDbContext,
                dataBlockService: dataBlockService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                releaseFileService: releaseFileService.Object
            );

            var result = await service.GetManageContentPageViewModel(releaseVersion.Id);

            MockUtils.VerifyAllMocks(dataBlockService, methodologyVersionRepository, releaseFileService);

            var viewModel = result.AssertRight();

            Assert.Equal(unattachedDataBlocks, viewModel.UnattachedDataBlocks);

            var contentRelease = viewModel.Release;

            Assert.NotNull(contentRelease);
            Assert.Equal(releaseVersion.Id, contentRelease.Id);
            Assert.Equal(releaseVersion.Release.TimePeriodCoverage.GetEnumLabel(), contentRelease.CoverageTitle);
            Assert.True(contentRelease.HasDataGuidance);
            Assert.True(contentRelease.HasPreReleaseAccessList);

            Assert.Equal(2, contentRelease.KeyStatistics.Count);
            Assert.Equal(releaseVersion.KeyStatistics[1].Id, contentRelease.KeyStatistics[0].Id);
            Assert.Equal(0, contentRelease.KeyStatistics[0].Order);
            var originalKeyStatDataBlock = (releaseVersion.KeyStatistics[1] as KeyStatisticDataBlock)!;
            var keyStatDataBlockViewModel = Assert.IsType<KeyStatisticDataBlockViewModel>(
                contentRelease.KeyStatistics[0]
            );
            Assert.Equal(originalKeyStatDataBlock.DataBlockId, keyStatDataBlockViewModel.DataBlockId);
            Assert.Equal(
                keyStatDataBlockParent.LatestPublishedVersion!.DataBlockParentId,
                keyStatDataBlockViewModel.DataBlockParentId
            );

            Assert.Equal(releaseVersion.KeyStatistics[0].Id, contentRelease.KeyStatistics[1].Id);
            Assert.Equal(1, contentRelease.KeyStatistics[1].Order);
            Assert.IsType<KeyStatisticTextViewModel>(contentRelease.KeyStatistics[1]);

            Assert.Equal(
                releaseVersion.KeyStatisticsSecondarySection!.Id,
                contentRelease.KeyStatisticsSecondarySection.Id
            );
            Assert.Equal(releaseVersion.HeadlinesSection!.Id, contentRelease.HeadlinesSection.Id);
            Assert.Equal(releaseVersion.RelatedDashboardsSection!.Id, contentRelease.RelatedDashboardsSection.Id);
            Assert.True(contentRelease.LatestRelease);
            Assert.Equal(releaseVersion.NextReleaseDate, contentRelease.NextReleaseDate);
            Assert.Equal(releaseVersion.Release.Year.ToString(), contentRelease.ReleaseName);
            Assert.Equal(releaseVersion.PublishScheduled?.ToUkDateOnly(), contentRelease.PublishScheduled);
            Assert.Equal(releaseVersion.Published, contentRelease.Published);
            Assert.Equal(publication.Id, contentRelease.PublicationId);
            Assert.Equal(releaseVersion.Release.Slug, contentRelease.Slug);
            Assert.Equal(releaseVersion.Release.Title, contentRelease.Title);
            Assert.Equal(releaseVersion.SummarySection!.Id, contentRelease.SummarySection.Id);
            Assert.Equal(releaseVersion.Type, contentRelease.Type);
            Assert.Equal(releaseVersion.Release.YearTitle, contentRelease.YearTitle);
            Assert.Empty(contentRelease.Updates);

            Assert.Equal(releaseVersion.PublishingOrganisations.Count, contentRelease.PublishingOrganisations.Count);
            Assert.All(
                releaseVersion.PublishingOrganisations,
                (expectedOrganisation, index) =>
                {
                    var actualOrganisation = contentRelease.PublishingOrganisations[index];
                    Assert.Equal(expectedOrganisation.Id, actualOrganisation.Id);
                    Assert.Equal(expectedOrganisation.Title, actualOrganisation.Title);
                    Assert.Equal(expectedOrganisation.Url, actualOrganisation.Url);
                }
            );

            var contentDownloadFiles = contentRelease.DownloadFiles.ToList();
            Assert.Equal(2, contentDownloadFiles.Count);
            Assert.Equal(files[0].Id, contentDownloadFiles[0].Id);
            Assert.Equal(files[0].Extension, contentDownloadFiles[0].Extension);
            Assert.Equal(files[0].FileName, contentDownloadFiles[0].FileName);
            Assert.Equal(files[0].Name, contentDownloadFiles[0].Name);
            Assert.Equal(files[0].Size, contentDownloadFiles[0].Size);
            Assert.Equal(files[0].Type, contentDownloadFiles[0].Type);
            Assert.Equal(files[1].Id, contentDownloadFiles[1].Id);
            Assert.Equal(files[1].Extension, contentDownloadFiles[1].Extension);
            Assert.Equal(files[1].FileName, contentDownloadFiles[1].FileName);
            Assert.Equal(files[1].Name, contentDownloadFiles[1].Name);
            Assert.Equal(files[1].Size, contentDownloadFiles[1].Size);
            Assert.Equal(files[1].Type, contentDownloadFiles[1].Type);

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
                dataBlockViewModel.DataBlockParentId
            );

            var contentPublication = contentRelease.Publication;

            Assert.NotNull(contentPublication);
            Assert.Equal(publication.Id, contentPublication.Id);
            Assert.Equal(publication.Contact.Id, contentPublication.Contact.Id);
            Assert.Equal(publication.Contact.ContactName, contentPublication.Contact.ContactName);
            Assert.Equal(publication.Contact.TeamEmail, contentPublication.Contact.TeamEmail);
            Assert.Equal(publication.Contact.ContactTelNo, contentPublication.Contact.ContactTelNo);
            Assert.Null(contentPublication.ExternalMethodology);
            Assert.Equal(publication.Slug, contentPublication.Slug);
            Assert.Equal(publication.Summary, contentPublication.Summary);
            Assert.Equal(publication.Title, contentPublication.Title);

            var contentPublicationReleaseSeries = contentPublication.ReleaseSeries;
            Assert.Equal(3, contentPublicationReleaseSeries.Count);

            Assert.False(contentPublicationReleaseSeries[0].IsLegacyLink);
            Assert.Equal(releaseVersion.Release.Id, contentPublicationReleaseSeries[0].ReleaseId);
            Assert.Equal(releaseVersion.Release.Slug, contentPublicationReleaseSeries[0].ReleaseSlug);
            Assert.Equal(releaseVersion.Release.Title, contentPublicationReleaseSeries[0].Description);
            Assert.Null(contentPublicationReleaseSeries[0].LegacyLinkUrl);

            Assert.True(contentPublicationReleaseSeries[1].IsLegacyLink);
            Assert.Null(contentPublicationReleaseSeries[1].ReleaseId);
            Assert.Null(contentPublicationReleaseSeries[1].ReleaseSlug);
            Assert.Equal(
                publication.ReleaseSeries[1].LegacyLinkDescription,
                contentPublicationReleaseSeries[1].Description
            );
            Assert.Equal(publication.ReleaseSeries[1].LegacyLinkUrl, contentPublicationReleaseSeries[1].LegacyLinkUrl);

            Assert.True(contentPublicationReleaseSeries[2].IsLegacyLink);
            Assert.Null(contentPublicationReleaseSeries[2].ReleaseId);
            Assert.Null(contentPublicationReleaseSeries[2].ReleaseSlug);
            Assert.Equal(
                publication.ReleaseSeries[2].LegacyLinkDescription,
                contentPublicationReleaseSeries[2].Description
            );
            Assert.Equal(publication.ReleaseSeries[2].LegacyLinkUrl, contentPublicationReleaseSeries[2].LegacyLinkUrl);

            Assert.Equal(2, contentPublication.Methodologies.Count);
            Assert.Equal(methodology.Versions[0].Id, contentPublication.Methodologies[0].Id);
            Assert.Equal(methodology.Versions[0].Title, contentPublication.Methodologies[0].Title);
            Assert.Equal(methodology.Versions[1].Id, contentPublication.Methodologies[1].Id);
            Assert.Equal(methodology.Versions[1].Title, contentPublication.Methodologies[1].Title);
        }
    }

    [Fact]
    public async Task GetManageContentPageViewModel_IsPrerelease()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)])
            .WithTheme(_dataFixture.DefaultTheme());

        var releaseVersion = publication.Releases.Single().Versions.Single();

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
            contentDbContext.MethodologyVersions.AddRange(methodologyVersions);
            contentDbContext.ContentSections.AddRange(
                new() { Type = ContentSectionType.Headlines, ReleaseVersion = releaseVersion },
                new() { Type = ContentSectionType.KeyStatisticsSecondary, ReleaseVersion = releaseVersion },
                new() { Type = ContentSectionType.ReleaseSummary, ReleaseVersion = releaseVersion },
                new() { Type = ContentSectionType.RelatedDashboards, ReleaseVersion = releaseVersion },
                new() { Type = ContentSectionType.Generic, ReleaseVersion = releaseVersion }
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

        dataBlockService
            .Setup(mock => mock.GetUnattachedDataBlocks(releaseVersion.Id))
            .ReturnsAsync(new List<DataBlockViewModel>());

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestVersionByPublication(publication.Id))
            .ReturnsAsync(methodologyVersions);

        releaseFileService
            .Setup(mock => mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data))
            .ReturnsAsync(new List<FileInfo>());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupManageContentPageService(
                contentDbContext: contentDbContext,
                dataBlockService: dataBlockService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                releaseFileService: releaseFileService.Object
            );

            var result = await service.GetManageContentPageViewModel(releaseVersion.Id, isPrerelease: true);

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
        User user1 = _dataFixture.DefaultUser().WithFirstName("Jane").WithLastName("Doe").WithEmail("jane@test.com");

        User user2 = _dataFixture.DefaultUser().WithFirstName("Rob").WithLastName("Rowe").WithEmail("rob@test.com");

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)])
            .WithTheme(_dataFixture.DefaultTheme());

        var releaseVersion = publication.Releases.Single().Versions.Single();

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
                            CreatedBy = user1,
                        },
                        new Comment
                        {
                            Content = "Comment 2",
                            Created = DateTime.Parse("2022-03-12T12:00:00Z"),
                            CreatedBy = user2,
                            Resolved = DateTime.Parse("2022-03-14T12:00:00Z"),
                            ResolvedBy = user1,
                        }
                    ),
                },
            },
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
                },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.ContentSections.AddRange(summaryContentSection, genericContentSection);

            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

        dataBlockService
            .Setup(mock => mock.GetUnattachedDataBlocks(releaseVersion.Id))
            .ReturnsAsync(new List<DataBlockViewModel>());

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestVersionByPublication(publication.Id))
            .ReturnsAsync(new List<MethodologyVersion>());

        releaseFileService
            .Setup(mock => mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data))
            .ReturnsAsync(new Either<ActionResult, IEnumerable<FileInfo>>(new List<FileInfo>()));

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupManageContentPageService(
                contentDbContext: contentDbContext,
                dataBlockService: dataBlockService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                releaseFileService: releaseFileService.Object
            );

            var result = await service.GetManageContentPageViewModel(releaseVersion.Id);

            var viewModel = result.AssertRight();

            dataBlockService.Verify(mock => mock.GetUnattachedDataBlocks(releaseVersion.Id), Times.Once);

            releaseFileService.Verify(mock => mock.ListAll(releaseVersion.Id, Ancillary, FileType.Data), Times.Once);

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
        IReleaseRepository? releaseRepository = null,
        IUserService? userService = null
    )
    {
        return new(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            mapper ?? MapperUtils.AdminMapper(contentDbContext),
            dataBlockService ?? new Mock<IDataBlockService>().Object,
            methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
            releaseFileService ?? new Mock<IReleaseFileService>().Object,
            releaseRepository ?? new ReleaseRepository(contentDbContext),
            userService ?? MockUtils.AlwaysTrueUserService().Object
        );
    }
}
