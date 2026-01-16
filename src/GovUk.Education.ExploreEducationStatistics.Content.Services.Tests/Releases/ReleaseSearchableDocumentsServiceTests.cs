using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases;

public abstract class ReleaseSearchableDocumentsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetLatestReleaseAsSearchableDocumentTests : ReleaseSearchableDocumentsServiceTests
    {
        [Fact]
        public async Task WhenPublicationAndReleaseExist_ReturnsExpectedSearchableDocument()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.HeadlinesSection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.Headlines)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Headlines content</p>")]);

            releaseVersion.SummarySection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.ReleaseSummary)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Summary content</p>")]);

            releaseVersion.GenericContent =
            [
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 1")
                    .WithContentBlocks([
                        _dataFixture.DefaultHtmlBlock().WithBody("<p>Section 1 block 1 content</p>"),
                        //contentDataBlockParent.LatestPublishedVersion!.ContentBlock,
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
                _dataFixture
                    .DefaultContentSection()
                    .WithHeading("Section 4")
                    .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Section 4 block 1 content</p>")]),
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
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

                // Assert
                var actual = outcome.AssertRight();

                var expectedHtmlContent = $"""
                    <html>
                        <head>
                            <title>{publication.Title}</title>
                        </head>
                        <body>
                            <h1>{publication.Title}</h1>
                            <h2>{release.Title}</h2>
                            <h3>Summary</h3>
                            <p>Summary content</p>
                            <h3>Headlines</h3>
                            <p>Headlines content</p>
                            <h3>Section 1</h3>
                            <p>Section 1 block 1 content</p>
                            <p>Section 1 block 3 content</p>
                            <h3>Section 2</h3>
                            <p>Section 2 block 1 content</p>
                            <h3>Section 4</h3>
                            <p>Section 4 block 1 content</p>
                        </body>
                    </html>
                    """;

                Assert.Multiple([
                    () => Assert.Equal(release.Id, actual.ReleaseId),
                    () => Assert.Equal(release.Slug, actual.ReleaseSlug),
                    () => Assert.Equal(releaseVersion.Id, actual.ReleaseVersionId),
                    () => Assert.Equal(publication.Id, actual.PublicationId),
                    () => Assert.Equal(publication.Slug, actual.PublicationSlug),
                    () => Assert.Equal(publication.Summary, actual.Summary),
                    () => Assert.Equal(publication.Title, actual.PublicationTitle),
                    () => Assert.Equal(releaseVersion.PublishedDisplayDate!.Value, actual.Published),
                    () => Assert.Equal(publication.Theme.Id, actual.ThemeId),
                    () => Assert.Equal(publication.Theme.Title, actual.ThemeTitle),
                    () => Assert.Equal(releaseVersion.Type.ToString(), actual.Type),
                    () => Assert.Equal(releaseVersion.Type.ToSearchDocumentTypeBoost(), actual.TypeBoost),
                    .. GetAssertTrimmedLinesEqual(expectedHtmlContent, actual.HtmlContent),
                ]);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionHasNoContent_ReturnsSearchableDocumentWithMinimalHtml()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Initialise the release version with empty sections to match how a newly created release would be configured
            releaseVersion.HeadlinesSection = CreateContentSection(ContentSectionType.Headlines);
            releaseVersion.SummarySection = CreateContentSection(ContentSectionType.ReleaseSummary);

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
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

                // Assert
                var actual = outcome.AssertRight();

                // The expected HTML content is missing the empty headlines, summary, and generic sections
                var expectedHtmlContent = $"""
                    <html>
                        <head>
                            <title>{publication.Title}</title>
                        </head>
                        <body>
                            <h1>{publication.Title}</h1>
                            <h2>{release.Title}</h2>
                        </body>
                    </html>
                    """;

                Assert.Multiple(GetAssertTrimmedLinesEqual(expectedHtmlContent, actual.HtmlContent));
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetLatestReleaseAsSearchableDocument(publicationSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme());

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
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

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
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

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
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        private static Action[] GetAssertTrimmedLinesEqual(string expectedLines, string actualLines)
        {
            // Trim each line then assert they are the same
            var expectedList = expectedLines.ToLines().Select(line => line.Trim()).ToList();
            var actualList = actualLines.ToLines().Select(line => line.Trim()).ToList();

            return
            [
                () => Assert.Equal(expectedList.Count, actualList.Count),
                .. expectedList
                    .Zip(actualList, (expected, actual) => (Action)(() => Assert.Equal(expected, actual)))
                    .ToArray(),
            ];
        }
    }

    private ContentSection CreateContentSection(ContentSectionType type) =>
        _dataFixture.DefaultContentSection().WithType(type);

    private static ReleaseSearchableDocumentsService BuildService(ContentDbContext contentDbContext) =>
        new(contentDbContext);
}
