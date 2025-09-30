using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class ReleaseServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly Guid Release1Version1Id = Guid.NewGuid();

    private static readonly Guid DataBlock1Id = Guid.NewGuid();
    private static readonly Guid DataBlock2Id = Guid.NewGuid();

    private static readonly DataBlockVersion Release1DataBlockVersion1 = new()
    {
        Id = DataBlock1Id,
        ReleaseVersionId = Release1Version1Id,
        ContentBlock = new DataBlock
        {
            Id = DataBlock1Id
        },
        DataBlockParent = new DataBlockParent
        {
            Id = Guid.NewGuid()
        }
    };

    private static readonly DataBlockVersion Release1DataBlockVersion2 = new()
    {
        Id = DataBlock2Id,
        ReleaseVersionId = Release1Version1Id,
        ContentBlock = new DataBlock
        {
            Id = DataBlock2Id,
            Order = 3
        },
        DataBlockParent = new DataBlockParent
        {
            Id = Guid.NewGuid()
        }
    };

    private static readonly Release Release1 = new()
    {
        Id = Guid.NewGuid(),
        PublicationId = Guid.NewGuid(),
        Year = 2018,
        TimePeriodCoverage = AcademicYearQ1,
        Slug = "2018-19-q1"
    };

    private static readonly ReleaseVersion Release1V1 = new()
    {
        Id = Release1Version1Id,
        Release = Release1,
        DataGuidance = "Release 1 v1 Guidance",
        RelatedInformation = new List<Link>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Related Information",
                Url = "https://example.com"
            }
        },
        ApprovalStatus = Approved,
        Published = new DateTime(2019, 2, 1),
        Updates = new List<Update>
        {
            new()
            {
                Id = Guid.NewGuid(),
                On = new DateTime(2020, 1, 1),
                Reason = "First update"
            }
        },
        Version = 0,
        PreviousVersionId = null,
        KeyStatistics = new List<KeyStatistic>
        {
            new KeyStatisticDataBlock
            {
                Order = 1,
                DataBlock = Release1DataBlockVersion1.ContentBlock,
                DataBlockParentId = Release1DataBlockVersion1.DataBlockParent.Id
            },
            new KeyStatisticText { Order = 0 },
        },
    };

    private static readonly ReleaseVersion Release1V2Deleted = new()
    {
        Id = Guid.NewGuid(),
        Release = Release1,
        DataGuidance = "Release 1 v2 Guidance",
        RelatedInformation = new List<Link>(),
        ApprovalStatus = Approved,
        Published = null,
        Version = 1,
        PreviousVersionId = Release1V1.Id,
        SoftDeleted = true
    };

    private static readonly ReleaseVersion Release1V3NotPublished = new()
    {
        Id = Guid.NewGuid(),
        Release = Release1,
        DataGuidance = "Release 1 v3 Guidance",
        RelatedInformation = new List<Link>(),
        ApprovalStatus = Approved,
        Published = null,
        Updates = new List<Update>
        {
            new()
            {
                Id = Guid.NewGuid(),
                On = new DateTime(2020, 1, 1),
                Reason = "First update"
            },
            new()
            {
                Id = Guid.NewGuid(),
                On = new DateTime(2020, 2, 1),
                Reason = "Second update"
            }
        },
        Version = 2,
        PreviousVersionId = Release1V1.Id
    };

    private static readonly List<ReleaseVersion> ReleaseVersions = new()
    {
        Release1V1,
        Release1V2Deleted,
        Release1V3NotPublished
    };

    private static readonly ContentBlock Release1SummarySectionHtmlContentBlock1 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 summary 1 order 2</p>",
        Order = 2,
    };

    private static readonly ContentBlock Release1SummarySectionHtmlContentBlock2 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 summary 2 order 0</p>",
        Order = 0,
    };

    private static readonly ContentBlock Release1SummarySectionHtmlContentBlock3 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 summary 3 order 1</p>",
        Order = 1,
    };

    private static readonly ContentBlock Release1Section1HtmlContentBlock1 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 section 1 order 2</p>",
        Order = 2,
    };

    private static readonly ContentBlock Release1Section1HtmlContentBlock2 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 section 1 order 0</p>",
        Order = 0,
    };

    private static readonly ContentBlock Release1Section1HtmlContentBlock3 = new HtmlBlock
    {
        Id = Guid.NewGuid(),
        Body = "<p>Release 1 section 1 order 1</p>",
        Order = 1,
    };

    private static readonly ContentSection Release1SummarySection = new()
    {
        Id = Guid.NewGuid(),
        Order = 1,
        Heading = "Release 1 summary section",
        Type = ContentSectionType.ReleaseSummary,
        Content = new List<ContentBlock>
        {
            Release1SummarySectionHtmlContentBlock1,
            Release1SummarySectionHtmlContentBlock2,
            Release1SummarySectionHtmlContentBlock3,
        },
        ReleaseVersion = Release1V1
    };

    private static readonly ContentSection Release1RelatedDashboardsSection = new()
    {
        Id = Guid.NewGuid(),
        Order = 0,
        Heading = "Release 1 related dashboards section",
        Type = ContentSectionType.RelatedDashboards,
        Content = new List<ContentBlock>(),
        ReleaseVersion = Release1V1
    };


    private static readonly ContentSection Release1Section1 = new()
    {
        Id = Guid.NewGuid(),
        Order = 2,
        Heading = "Release 1 section 1 order 2",
        Type = ContentSectionType.Generic,
        Content = new List<ContentBlock>
        {
            Release1Section1HtmlContentBlock1,
            Release1Section1HtmlContentBlock2,
            Release1Section1HtmlContentBlock3,
            Release1DataBlockVersion2.ContentBlock
        },
        ReleaseVersion = Release1V1
    };

    private static readonly ContentSection Release1Section2 = new()
    {
        Id = Guid.NewGuid(),
        Order = 0,
        Heading = "Release 1 section 2 order 0",
        Type = ContentSectionType.Generic,
        ReleaseVersion = Release1V1
    };

    private static readonly ContentSection Release1Section3 = new()
    {
        Id = Guid.NewGuid(),
        Order = 1,
        Heading = "Release 1 section 3 order 1",
        Type = ContentSectionType.Generic,
        ReleaseVersion = Release1V1
    };

    private static readonly List<ContentSection> ContentSections =
        ListOf(
            Release1SummarySection,
            Release1RelatedDashboardsSection,
            Release1Section1,
            Release1Section2,
            Release1Section3);

    [Fact]
    public async Task GetRelease()
    {
        var releaseFiles = new List<ReleaseFile>
        {
            new()
            {
                ReleaseVersion = Release1V1,
                Name = "Ancillary Test File",
                File = new File
                {
                    Filename = "ancillary.pdf",
                    ContentLength = 10240,
                    Type = Ancillary
                }
            },
            new()
            {
                ReleaseVersion = Release1V1,
                Name = "Data Test File",
                File = new File
                {
                    Filename = "data.csv",
                    ContentLength = 20480,
                    Type = FileType.Data
                }
            },
            new()
            {
                ReleaseVersion = Release1V1,
                File = new File
                {
                    Filename = "chart.png",
                    Type = Chart
                }
            },
            new()
            {
                ReleaseVersion = Release1V1,
                File = new File
                {
                    Filename = "data.meta.csv",
                    Type = Metadata
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(ReleaseVersions);
            contentDbContext.DataBlockVersions.AddRange(Release1DataBlockVersion1, Release1DataBlockVersion2);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            contentDbContext.ContentSections.AddRange(ContentSections);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            var result = await service.GetRelease(Release1V1.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(Release1V1.Id, viewModel.Id);
            Assert.Equal(Release1V1.Release.Title, viewModel.Title);
            Assert.Equal(Release1V1.Published, viewModel.Published);
            Assert.Equal(Release1.Id, viewModel.ReleaseId);

            Assert.Equal(2, viewModel.KeyStatistics.Count);

            Assert.Equal(Release1V1.KeyStatistics[1].Id, viewModel.KeyStatistics[0].Id);
            Assert.Equal(0, viewModel.KeyStatistics[0].Order);
            Assert.IsType<KeyStatisticTextViewModel>(viewModel.KeyStatistics[0]);

            Assert.Equal(Release1V1.KeyStatistics[0].Id, viewModel.KeyStatistics[1].Id);
            Assert.Equal(1, viewModel.KeyStatistics[1].Order);
            var keyStatDataBlockViewModel = Assert.IsType<KeyStatisticDataBlockViewModel>(viewModel.KeyStatistics[1]);
            Assert.Equal(Release1DataBlockVersion1.Id, keyStatDataBlockViewModel.DataBlockId);
            Assert.Equal(Release1DataBlockVersion1.DataBlockParent.Id, keyStatDataBlockViewModel.DataBlockParentId);

            var summarySection = viewModel.SummarySection;
            Assert.NotNull(summarySection);
            Assert.Equal(Release1SummarySection.Id, summarySection.Id);
            var summarySectionContent = summarySection.Content;
            Assert.NotNull(summarySectionContent);
            Assert.Equal(3, summarySectionContent.Count);
            Assert.Equal(Release1SummarySectionHtmlContentBlock2.Id, summarySectionContent[0].Id);
            Assert.Equal("<p>Release 1 summary 2 order 0</p>",
                (summarySectionContent[0] as HtmlBlockViewModel)?.Body);
            Assert.Equal(Release1SummarySectionHtmlContentBlock3.Id, summarySectionContent[1].Id);
            Assert.Equal("<p>Release 1 summary 3 order 1</p>",
                (summarySectionContent[1] as HtmlBlockViewModel)?.Body);
            Assert.Equal(Release1SummarySectionHtmlContentBlock1.Id, summarySectionContent[2].Id);
            Assert.Equal("<p>Release 1 summary 1 order 2</p>",
                (summarySectionContent[2] as HtmlBlockViewModel)?.Body);

            var relatedDashboardsSection = viewModel.RelatedDashboardsSection;
            Assert.NotNull(relatedDashboardsSection);
            Assert.Equal(Release1RelatedDashboardsSection.Id, relatedDashboardsSection.Id);
            Assert.Empty(relatedDashboardsSection.Content);

            var content = viewModel.Content;
            Assert.NotNull(content);
            Assert.Equal(3, content.Count);

            Assert.Equal(Release1Section2.Id, content[0].Id);
            Assert.Equal("Release 1 section 2 order 0", content[0].Heading);
            Assert.Empty(content[0].Content);

            Assert.Equal(Release1Section3.Id, content[1].Id);
            Assert.Equal("Release 1 section 3 order 1", content[1].Heading);
            Assert.Empty(content[1].Content);

            Assert.Equal(Release1Section1.Id, content[2].Id);
            Assert.Equal("Release 1 section 1 order 2", content[2].Heading);
            Assert.Equal(4, content[2].Content.Count);
            Assert.Equal(Release1Section1HtmlContentBlock2.Id, content[2].Content[0].Id);
            Assert.Equal("<p>Release 1 section 1 order 0</p>", (content[2].Content[0] as HtmlBlockViewModel)?.Body);
            Assert.Equal(Release1Section1HtmlContentBlock3.Id, content[2].Content[1].Id);
            Assert.Equal("<p>Release 1 section 1 order 1</p>", (content[2].Content[1] as HtmlBlockViewModel)?.Body);
            Assert.Equal(Release1Section1HtmlContentBlock1.Id, content[2].Content[2].Id);
            Assert.Equal("<p>Release 1 section 1 order 2</p>", (content[2].Content[2] as HtmlBlockViewModel)?.Body);
            Assert.Equal(Release1DataBlockVersion2.Id, content[2].Content[3].Id);
            Assert.Equal(Release1DataBlockVersion2.DataBlockParent.Id, (content[2].Content[3] as DataBlockViewModel)?.DataBlockParentId);

            Assert.Single(viewModel.Updates);
            Assert.Equal(new DateTime(2020, 1, 1), viewModel.Updates[0].On);
            Assert.Equal("First update", viewModel.Updates[0].Reason);

            Assert.Equal(2, viewModel.DownloadFiles.Count);
            Assert.Equal(releaseFiles[0].File.Id, viewModel.DownloadFiles[0].Id);
            Assert.Equal("pdf", viewModel.DownloadFiles[0].Extension);
            Assert.Equal("ancillary.pdf", viewModel.DownloadFiles[0].FileName);
            Assert.Equal("Ancillary Test File", viewModel.DownloadFiles[0].Name);
            Assert.Equal("10 Kb", viewModel.DownloadFiles[0].Size);
            Assert.Equal(Ancillary, viewModel.DownloadFiles[0].Type);
            Assert.Equal(releaseFiles[1].File.Id, viewModel.DownloadFiles[1].Id);
            Assert.Equal("csv", viewModel.DownloadFiles[1].Extension);
            Assert.Equal("data.csv", viewModel.DownloadFiles[1].FileName);
            Assert.Equal("Data Test File", viewModel.DownloadFiles[1].Name);
            Assert.Equal("20 Kb", viewModel.DownloadFiles[1].Size);
            Assert.Equal(FileType.Data, viewModel.DownloadFiles[1].Type);

            Assert.Equal("Release 1 v1 Guidance", viewModel.DataGuidance);

            Assert.Single(viewModel.RelatedInformation);
            Assert.Equal(Release1V1.RelatedInformation[0].Id, viewModel.RelatedInformation[0].Id);
        }
    }

    [Fact]
    public async Task GetRelease_FiltersContent()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions[0];

        var originalContent = @"
                <p>
                    Content 1 <comment-start name=""comment-1""></comment-start>goes here<comment-end name=""comment-1""></comment-end>
                </p>
                <ul>
                    <li><comment-start name=""comment-2""/>Content 2<comment-end name=""comment-2""/></li>
                    <li><commentplaceholder-start name=""comment-3""/>Content 3<commentplaceholder-end name=""comment-3""/></li>
                    <li><resolvedcomment-start name=""comment-4""/>Content 4<resolvedcomment-end name=""comment-4""/></li>
                </ul>".TrimIndent();

        releaseVersion.Content = new List<ContentSection>
        {
            new()
            {
                Type = ContentSectionType.ReleaseSummary,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = originalContent
                    }
                }
            },
            new()
            {
                Type = ContentSectionType.RelatedDashboards,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = originalContent
                    }
                }
            },
            new()
            {
                Type = ContentSectionType.Headlines,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = originalContent
                    }
                }
            },
            new()
            {
                Type = ContentSectionType.Generic,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Body = originalContent
                    }
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            var result = await service.GetRelease(releaseVersion.Id);
            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);

            var expectedContent = @"
                    <p>
                        Content 1 goes here
                    </p>
                    <ul>
                        <li>Content 2</li>
                        <li>Content 3</li>
                        <li>Content 4</li>
                    </ul>".TrimIndent();

            var summarySection = viewModel.SummarySection;
            Assert.Single(summarySection.Content);

            Assert.Equal(expectedContent, (summarySection.Content[0] as HtmlBlockViewModel)?.Body);

            var headlines = viewModel.HeadlinesSection;
            Assert.Single(headlines.Content);
            Assert.Equal(expectedContent, (headlines.Content[0] as HtmlBlockViewModel)?.Body);

            var relatedDashboardsSection = viewModel.RelatedDashboardsSection;
            Assert.NotNull(relatedDashboardsSection);
            Assert.Single(relatedDashboardsSection.Content);
            Assert.Equal(expectedContent, (relatedDashboardsSection.Content[0] as HtmlBlockViewModel)?.Body);

            var content = viewModel.Content;
            Assert.Single(content);
            Assert.Single(content[0].Content);

            Assert.Equal(expectedContent, (content[0].Content[0] as HtmlBlockViewModel)?.Body);
        }
    }

    [Fact]
    public async Task GetRelease_NotPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null });

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            // Test scenario of the publisher getting an unpublished release to cache it in advance of publishing it
            // on a scheduled date
            var expectedPublishDate = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            var result = await service.GetRelease(releaseVersion.Id, expectedPublishDate);

            var viewModel = result.AssertRight();

            // Published date in the view model should match the expected publish date
            Assert.Equal(expectedPublishDate, viewModel.Published);
        }
    }

    [Fact]
    public async Task GetRelease_NotPublished_ExpectedPublishDateIsNull()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null });

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            // Test scenario of the publisher getting an unpublished release to cache it in advance of publishing it
            // immediately.
            var result = await service.GetRelease(releaseVersion.Id,
                expectedPublishDate: null);

            var viewModel = result.AssertRight();

            // Published date in the view model should match the date now
            viewModel.Published.AssertUtcNow();
        }
    }

    [Fact]
    public async Task GetRelease_AmendedReleaseAndUpdatePublishedDateIsFalse()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)
                .Generate(1));

        var (previousReleaseVersion, releaseVersion) = publication.ReleaseVersions.ToTuple2();
        releaseVersion.UpdatePublishedDate = false;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            // Test scenario of the publisher getting an unpublished amended release to cache it in advance of
            // publishing it.
            // An update to the published date *has not* been requested.
            var expectedPublishDate = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            var result = await service.GetRelease(releaseVersion.Id, expectedPublishDate);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);
            // Published date in the view model should match the published date of the previous version
            Assert.Equal(previousReleaseVersion.Published, viewModel.Published);
        }
    }

    [Fact]
    public async Task GetRelease_AmendedReleaseAndUpdatePublishedDateIsTrue()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)
                .Generate(1));

        var releaseVersion = publication.ReleaseVersions[1];
        releaseVersion.UpdatePublishedDate = true;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            // Test scenario of the publisher getting an unpublished amended release to cache it in advance of
            // publishing it.
            // An update to the published date *has* been requested.
            var expectedPublishDate = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            var result = await service.GetRelease(releaseVersion.Id, expectedPublishDate);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);
            // Published date in the view model should match the expected publish date
            Assert.Equal(expectedPublishDate, viewModel.Published);
        }
    }

    [Fact]
    public async Task GetRelease_AmendedReleaseNotYetPublishedHasUpdatesInDescendingOrder()
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(ReleaseVersions);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseService(contentDbContext: contentDbContext);

            var result = await service.GetRelease(Release1V3NotPublished.Id, DateTime.UtcNow);
            var viewModel = result.AssertRight();

            Assert.Equal(Release1V3NotPublished.Id, viewModel.Id);
            Assert.Equal(Release1V3NotPublished.Release.Title, viewModel.Title);

            Assert.Equal(2, viewModel.Updates.Count);
            Assert.Equal(new DateTime(2020, 2, 1), viewModel.Updates[0].On);
            Assert.Equal("Second update", viewModel.Updates[0].Reason);

            Assert.Equal(new DateTime(2020, 1, 1), viewModel.Updates[1].On);
            Assert.Equal("First update", viewModel.Updates[1].Reason);
        }
    }

    [Fact]
    public async Task List()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1)
                .Generate(2));

        var release1Version1 = publication.Releases[0].Versions[0];
        var release2Version1 = publication.Releases[1].Versions[0];

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(2, releases.Count);

            // Ordered from newest to oldest
            Assert.Equal(release2Version1.Id, releases[0].Id);
            Assert.Equal(release2Version1.Release.Title, releases[0].Title);
            Assert.Equal(release2Version1.Release.Slug, releases[0].Slug);
            Assert.Equal(release2Version1.Release.TimePeriodCoverage.GetEnumLabel(), releases[0].CoverageTitle);
            Assert.Equal(release2Version1.Release.YearTitle, releases[0].YearTitle);
            Assert.Equal(release2Version1.Published, releases[0].Published);
            Assert.Equal(release2Version1.NextReleaseDate, releases[0].NextReleaseDate);
            Assert.Equal(release2Version1.Type, releases[0].Type);
            Assert.True(releases[0].LatestRelease);

            Assert.Equal(release1Version1.Id, releases[1].Id);
            Assert.Equal(release1Version1.Release.Title, releases[1].Title);
            Assert.Equal(release1Version1.Release.Slug, releases[1].Slug);
            Assert.Equal(release1Version1.Release.TimePeriodCoverage.GetEnumLabel(), releases[1].CoverageTitle);
            Assert.Equal(release1Version1.Release.YearTitle, releases[1].YearTitle);
            Assert.Equal(release1Version1.Published, releases[1].Published);
            Assert.Equal(release1Version1.NextReleaseDate, releases[1].NextReleaseDate);
            Assert.Equal(release1Version1.Type, releases[1].Type);
            Assert.False(releases[1].LatestRelease);
        }
    }

    [Fact]
    public async Task List_FiltersPreviousReleasesForAmendments()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 3)
                .Generate(1));

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(new[]
            {
                publication.Releases[0].Versions.Single(rv => rv is { Version: 2 }).Id
            }, releases.Select(r => r.Id).ToArray());
        }
    }

    [Fact]
    public async Task List_FiltersDraftReleases()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1, draftVersion: true)
                .Generate(2));

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseService(contentDbContext);

            var result = await service.List(publication.Slug);

            var releases = result.AssertRight();

            Assert.Equal(new[]
            {
                publication.Releases.Single(r => r.Year == 2001).Versions.Single(rv => rv is { Published: not null }).Id,
                publication.Releases.Single(r => r.Year == 2000).Versions.Single(rv => rv is { Published: not null }).Id
            }, releases.Select(r => r.Id).ToArray());
        }
    }

    [Fact]
    public async Task List_PublicationNotFound()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var service = SetupReleaseService(contentDbContext);

        var result = await service.List("random-slug");

        result.AssertNotFound();
    }

    private static ReleaseService SetupReleaseService(
        ContentDbContext contentDbContext,
        IReleaseFileRepository? releaseFileRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IUserService? userService = null)
    {
        return new(
            contentDbContext,
            releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            userService ?? AlwaysTrueUserService().Object,
            MapperUtils.ContentMapper(contentDbContext)
        );
    }
}
