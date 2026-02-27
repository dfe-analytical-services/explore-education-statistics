using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases;

public abstract class ReleaseContentServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetReleaseContentTests : ReleaseContentServiceTests
    {
        [Fact]
        public async Task WhenPublicationAndReleaseExist_ReturnsExpectedContent()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            var (keyStatsDataBlockParent, keyStatsSecondaryDataBlockParent, contentDataBlockParent) = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(
                    _dataFixture
                        .DefaultDataBlockVersion()
                        .WithReleaseVersion(releaseVersion)
                        .WithCharts([
                            new InfographicChart
                            {
                                Title = "Chart title",
                                FileId = "file-id",
                                Height = 400,
                                Width = 500,
                            },
                        ])
                )
                .GenerateTuple3();

            releaseVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithOrder(1),
                _dataFixture
                    .DefaultKeyStatisticDataBlock()
                    .WithOrder(2)
                    .WithDataBlockParent(keyStatsDataBlockParent)
                    .WithDataBlock(keyStatsSecondaryDataBlockParent.LatestPublishedVersion!.ContentBlock),
                _dataFixture.DefaultKeyStatisticText().WithOrder(3),
            ];

            releaseVersion.HeadlinesSection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.Headlines)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Headlines content</p>")]);

            releaseVersion.KeyStatisticsSecondarySection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.KeyStatisticsSecondary)
                .WithContentBlocks([keyStatsSecondaryDataBlockParent.LatestPublishedVersion!.ContentBlock]);

            releaseVersion.SummarySection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.ReleaseSummary)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Summary content</p>")]);

            releaseVersion.WarningSection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.Warning)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Warning content</p>")]);

            releaseVersion.GenericContent =
            [
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 1")
                    .WithContentBlocks([
                        _dataFixture.DefaultHtmlBlock().WithBody("<p>Section 1 block 1 content</p>"),
                        contentDataBlockParent.LatestPublishedVersion!.ContentBlock,
                        _dataFixture.DefaultHtmlBlock().WithBody("<p>Section 1 block 3 content</p>"),
                    ]),
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 2")
                    .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Section 2 block 1 content</p>")]),
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 3")
                    .WithContentBlocks([
                        _dataFixture.DefaultEmbedBlockLink().WithEmbedBlock(_dataFixture.DefaultEmbedBlock()),
                    ]),
            ];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);

                Assert.NotNull(result.HeadlinesSection);
                AssertContentSectionEqual(releaseVersion.HeadlinesSection, result.HeadlinesSection);

                Assert.NotNull(result.KeyStatisticsSecondarySection);
                AssertContentSectionEqual(
                    releaseVersion.KeyStatisticsSecondarySection,
                    result.KeyStatisticsSecondarySection
                );

                Assert.NotNull(result.SummarySection);
                AssertContentSectionEqual(releaseVersion.SummarySection, result.SummarySection);

                Assert.NotNull(result.WarningSection);
                AssertContentSectionEqual(releaseVersion.WarningSection, result.WarningSection);

                var expectedContentSections = releaseVersion.GenericContent.OrderBy(cs => cs.Order).ToList();
                Assert.Equal(expectedContentSections.Count, result.Content.Length);
                Assert.All(
                    expectedContentSections,
                    (expectedContentSection, index) =>
                    {
                        var actualContentSection = result.Content[index];
                        AssertContentSectionEqual(expectedContentSection, actualContentSection);
                    }
                );

                var expectedKeyStatistics = releaseVersion.KeyStatistics.OrderBy(ks => ks.Order).ToList();
                Assert.Equal(expectedKeyStatistics.Count, result.KeyStatistics.Length);
                Assert.All(
                    expectedKeyStatistics,
                    (expectedKeyStatistic, index) =>
                    {
                        var actualKeyStatistic = result.KeyStatistics[index];
                        AssertKeyStatisticEqual(expectedKeyStatistic, actualKeyStatistic);
                    }
                );
            }
        }

        [Fact]
        public async Task WhenContentContainsComments_RemovesComments()
        {
            // Arrange
            const string section1Block1Body = """
                <p><comment-start name="comment-1"></comment-start>Section 1 block 1 content<comment-end name="comment-1"></comment-end></p>
                """;
            const string section1Block2Body = """
                <p><commentplaceholder-start name="comment-2"></commentplaceholder-start>Section 1 block 2 content<commentplaceholder-end name="comment-2"></commentplaceholder-end></p>
                """;
            const string section1Block3Body = """
                <p><resolvedcomment-start name="comment-3"></resolvedcomment-start>Section 1 block 3 content<resolvedcomment-end name="comment-3"></resolvedcomment-end></p>
                """;
            const string headlinesBlockBody = """
                <p><comment-start name="comment-4"></comment-start>Headlines content<comment-end name="comment-4"></comment-end></p>
                """;
            const string summaryBlockBody = """
                <p><commentplaceholder-start name="comment-5"></commentplaceholder-start>Summary content<commentplaceholder-end name="comment-5"></commentplaceholder-end></p>
                """;
            const string warningBlockBody = """
                <p><resolvedcomment-start name="comment-6"></resolvedcomment-start>Warning content<resolvedcomment-end name="comment-6"></resolvedcomment-end></p>
                """;

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithPublished(DateTimeOffset.UtcNow)
                                        .WithContent([
                                            _dataFixture
                                                .DefaultContentSection()
                                                .WithHeading("Section 1")
                                                .WithContentBlocks(
                                                    _dataFixture
                                                        .DefaultHtmlBlock()
                                                        .ForIndex(0, s => s.SetBody(section1Block1Body))
                                                        .ForIndex(1, s => s.SetBody(section1Block2Body))
                                                        .ForIndex(2, s => s.SetBody(section1Block3Body))
                                                        .GenerateList()
                                                ),
                                        ])
                                        .WithKeyStatisticsSecondaryContent([])
                                        .WithHeadlinesContent([
                                            _dataFixture.DefaultHtmlBlock().WithBody(headlinesBlockBody),
                                        ])
                                        .WithReleaseSummaryContent([
                                            _dataFixture.DefaultHtmlBlock().WithBody(summaryBlockBody),
                                        ])
                                        .WithWarningContent([
                                            _dataFixture.DefaultHtmlBlock().WithBody(warningBlockBody),
                                        ]),
                                ]
                            ),
                    ]
                );

            var release = publication.Releases[0];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var actual = outcome.AssertRight();

                var contentSection1 = Assert.Single(actual.Content);

                Assert.Equal(3, contentSection1.Content.Length);
                var htmlBlock1 = Assert.IsType<HtmlBlockDto>(contentSection1.Content[0]);
                Assert.Equal("<p>Section 1 block 1 content</p>", htmlBlock1.Body);
                var htmlBlock2 = Assert.IsType<HtmlBlockDto>(contentSection1.Content[1]);
                Assert.Equal("<p>Section 1 block 2 content</p>", htmlBlock2.Body);
                var htmlBlock3 = Assert.IsType<HtmlBlockDto>(contentSection1.Content[2]);
                Assert.Equal("<p>Section 1 block 3 content</p>", htmlBlock3.Body);

                Assert.Single(actual.HeadlinesSection.Content);
                var headlinesHtmlBlock = Assert.IsType<HtmlBlockDto>(actual.HeadlinesSection.Content[0]);
                Assert.Equal("<p>Headlines content</p>", headlinesHtmlBlock.Body);

                Assert.Single(actual.SummarySection.Content);
                var summaryHtmlBlock = Assert.IsType<HtmlBlockDto>(actual.SummarySection.Content[0]);
                Assert.Equal("<p>Summary content</p>", summaryHtmlBlock.Body);

                Assert.Single(actual.WarningSection.Content);
                var warningHtmlBlock = Assert.IsType<HtmlBlockDto>(actual.WarningSection.Content[0]);
                Assert.Equal("<p>Warning content</p>", warningHtmlBlock.Body);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionHasEmptyContent_ReturnsEmptyContent()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Initialise the release version with empty sections to match how a newly created release would be configured.
            InitialiseNonGenericSectionsForReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(release.Versions[0].Id, result.ReleaseVersionId);
                Assert.Empty(result.Content);
                Assert.Empty(result.HeadlinesSection.Content);
                Assert.Empty(result.KeyStatistics);
                Assert.Empty(result.KeyStatisticsSecondarySection.Content);
                Assert.Empty(result.SummarySection.Content);
                Assert.Empty(result.WarningSection.Content);
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersions_ReturnsContentForLatestPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)]);
            var release = publication.Releases[0];

            var olderReleaseVersion = release.Versions[0];
            olderReleaseVersion.GenericContent =
            [
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 1")
                    .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Old content</p>")]),
            ];
            InitialiseNonGenericSectionsForReleaseVersion(olderReleaseVersion);

            var latestPublishedReleaseVersion = release.Versions[1];
            latestPublishedReleaseVersion.GenericContent =
            [
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 1")
                    .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Latest published content</p>")]),
            ];
            InitialiseNonGenericSectionsForReleaseVersion(latestPublishedReleaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(latestPublishedReleaseVersion.Id, result.ReleaseVersionId);
                var actualHtmlBlock = Assert.IsType<HtmlBlockDto>(result.Content[0].Content[0]);
                Assert.Equal("<p>Latest published content</p>", actualHtmlBlock.Body);
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            const string releaseSlug = "test-release";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseContent(publicationSlug: publicationSlug, releaseSlug: releaseSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();
            const string releaseSlug = "release-that-does-not-exist";

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: releaseSlug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenReleaseHasNoPublishedVersion_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);
            var release = publication.Releases[0];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseContent(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        private void InitialiseNonGenericSectionsForReleaseVersion(ReleaseVersion releaseVersion)
        {
            releaseVersion.HeadlinesSection = CreateContentSection(ContentSectionType.Headlines);
            releaseVersion.KeyStatisticsSecondarySection = CreateContentSection(
                ContentSectionType.KeyStatisticsSecondary
            );
            releaseVersion.SummarySection = CreateContentSection(ContentSectionType.ReleaseSummary);
            releaseVersion.WarningSection = CreateContentSection(ContentSectionType.Warning);
        }

        private ContentSection CreateContentSection(ContentSectionType type) =>
            _dataFixture.DefaultContentSection().WithType(type);

        private static void AssertContentSectionEqual(ContentSection expected, ContentSectionDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Heading, actual.Heading);

            var expectedContentBlocks = expected.Content.OrderBy(cb => cb.Order).ToList();

            Assert.Equal(expectedContentBlocks.Count, actual.Content.Length);
            Assert.All(
                expectedContentBlocks,
                (expectedBlock, index) =>
                {
                    var actualBlock = actual.Content[index];
                    AssertContentBlockTypeEqual(expectedBlock, actualBlock);
                    switch (expectedBlock)
                    {
                        case DataBlock expectedDataBlock when actualBlock is DataBlockDto actualDataBlock:
                            AssertDataBlockEqual(expectedDataBlock, actualDataBlock);
                            break;
                        case EmbedBlockLink expectedEmbedBlockLink
                            when actualBlock is EmbedBlockLinkDto actualEmbedBlockLink:
                            AssertEmbedBlockLinkEqual(expectedEmbedBlockLink, actualEmbedBlockLink);
                            break;
                        case HtmlBlock expectedHtmlBlock when actualBlock is HtmlBlockDto actualHtmlBlock:
                            AssertHtmlBlockEqual(expectedHtmlBlock, actualHtmlBlock);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(expectedBlock), expectedBlock, null);
                    }
                }
            );
        }

        private static void AssertContentBlockTypeEqual(ContentBlock expected, ContentBlockBaseDto actual)
        {
            var expectedType = expected switch
            {
                DataBlock => typeof(DataBlockDto),
                EmbedBlockLink => typeof(EmbedBlockLinkDto),
                HtmlBlock => typeof(HtmlBlockDto),
                _ => throw new ArgumentOutOfRangeException(nameof(expected), expected, null),
            };
            if (expectedType != actual.GetType())
            {
                Assert.Fail(
                    $"Expected {expectedType.Name} with Id: {expected.Id} but got {actual.GetType().Name} with Id: {actual.Id}"
                );
            }
        }

        private static void AssertDataBlockEqual(DataBlock expected, DataBlockDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            AssertDataBlockVersionEqual(expected.DataBlockVersion, actual.DataBlockVersion);
        }

        private static void AssertDataBlockVersionEqual(DataBlockVersion expected, DataBlockVersionDto actual)
        {
            Assert.Equal(expected.Id, actual.DataBlockVersionId);
            Assert.Equal(expected.DataBlockParentId, actual.DataBlockParentId);
            expected.Charts.AssertDeepEqualTo(actual.Charts);
            Assert.Equal(expected.Heading, actual.Heading);
            Assert.Equal(expected.Name, actual.Name);
            expected.Query.AssertDeepEqualTo(actual.Query);
            Assert.Equal(expected.Source, actual.Source);
            expected.Table.AssertDeepEqualTo(actual.Table);
        }

        private static void AssertEmbedBlockLinkEqual(EmbedBlockLink expected, EmbedBlockLinkDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            AssertEmbedBlockEqual(expected.EmbedBlock, actual.EmbedBlock);
        }

        private static void AssertEmbedBlockEqual(EmbedBlock expected, EmbedBlockDto actual)
        {
            Assert.Equal(expected.Id, actual.EmbedBlockId);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Url, actual.Url);
        }

        private static void AssertHtmlBlockEqual(HtmlBlock expected, HtmlBlockDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Body, actual.Body);
        }

        private static void AssertKeyStatisticEqual(KeyStatistic expected, KeyStatisticBaseDto actual)
        {
            AssertKeyStatisticTypeEqual(expected, actual);
            switch (expected)
            {
                case KeyStatisticDataBlock expectedDataBlock when actual is KeyStatisticDataBlockDto actualDataBlock:
                    AssertKeyStatisticDataBlockEqual(expectedDataBlock, actualDataBlock);
                    break;
                case KeyStatisticText expectedText when actual is KeyStatisticTextDto actualText:
                    AssertKeyStatisticTextEqual(expectedText, actualText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expected), expected, null);
            }
        }

        private static void AssertKeyStatisticTypeEqual(KeyStatistic expected, KeyStatisticBaseDto actual)
        {
            var expectedType = expected switch
            {
                KeyStatisticDataBlock => typeof(KeyStatisticDataBlockDto),
                KeyStatisticText => typeof(KeyStatisticTextDto),
                _ => throw new ArgumentOutOfRangeException(nameof(expected), expected, null),
            };
            if (expectedType != actual.GetType())
            {
                Assert.Fail(
                    $"Expected {expectedType.Name} with Id: {expected.Id} but got {actual.GetType().Name} with Id: {actual.Id}"
                );
            }
        }

        private static void AssertKeyStatisticDataBlockEqual(
            KeyStatisticDataBlock expected,
            KeyStatisticDataBlockDto actual
        )
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.DataBlockId, actual.DataBlockVersionId);
            Assert.Equal(expected.DataBlockParentId, actual.DataBlockParentId);
            Assert.Equal(expected.GuidanceText, actual.GuidanceText);
            Assert.Equal(expected.GuidanceTitle, actual.GuidanceTitle);
            Assert.Equal(expected.Trend, actual.Trend);
        }

        private static void AssertKeyStatisticTextEqual(KeyStatisticText expected, KeyStatisticTextDto actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.GuidanceText, actual.GuidanceText);
            Assert.Equal(expected.GuidanceTitle, actual.GuidanceTitle);
            Assert.Equal(expected.Statistic, actual.Statistic);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.Trend, actual.Trend);
        }
    }

    private static ReleaseContentService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
