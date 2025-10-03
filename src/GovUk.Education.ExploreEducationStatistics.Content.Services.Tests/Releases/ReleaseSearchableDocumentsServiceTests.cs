using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            var releaseViewModel = new ReleaseCacheViewModel(releaseVersion.Id)
            {
                ReleaseId = release.Id,
                Slug = release.Slug,
                Published = releaseVersion.Published,
                Title = release.Title,
                Type = releaseVersion.Type,
                HeadlinesSection = new ContentSectionViewModelBuilder().AddHtmlContent(
                    "<p>here is the headline content</p>"
                ),
                SummarySection = new ContentSectionViewModelBuilder().AddHtmlContent(
                    "<p>This is the release summary</p>"
                ),
                Content =
                [
                    new ContentSectionViewModelBuilder()
                        .WithHeading("section one")
                        .AddHtmlContent("<p>content section body one</p>"),
                    new ContentSectionViewModelBuilder()
                        .WithHeading("section two")
                        .AddHtmlContent("<p>content section body two</p>"),
                    new ContentSectionViewModelBuilder()
                        .WithHeading("section three")
                        .AddHtmlContent("<p>content section body three</p>"),
                ],
            };

            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService
                .Setup(mock => mock.GetRelease(publication.LatestPublishedReleaseVersionId!.Value, null))
                .ReturnsAsync(releaseViewModel);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context, releaseService.Object);

                // Act
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

                // Assert
                releaseService.VerifyAll();
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
                             <p>This is the release summary</p>
                             <h3>Headlines</h3>
                             <p>here is the headline content</p>
                             <h3>section one</h3>
                             <p>content section body one</p>
                             <h3>section two</h3>
                             <p>content section body two</p>
                             <h3>section three</h3>
                             <p>content section body three</p>
                         </body>
                     </html>
                    """;

                Assert.Multiple(
                    [
                        () => Assert.Equal(release.Id, actual.ReleaseId),
                        () => Assert.Equal(release.Slug, actual.ReleaseSlug),
                        () => Assert.Equal(releaseVersion.Id, actual.ReleaseVersionId),
                        () => Assert.Equal(publication.Id, actual.PublicationId),
                        () => Assert.Equal(publication.Slug, actual.PublicationSlug),
                        () => Assert.Equal(publication.Summary, actual.Summary),
                        () => Assert.Equal(publication.Title, actual.PublicationTitle),
                        () => Assert.Equal(releaseVersion.Published!.Value, actual.Published),
                        () => Assert.Equal(publication.Theme.Id, actual.ThemeId),
                        () => Assert.Equal(publication.Theme.Title, actual.ThemeTitle),
                        () => Assert.Equal(releaseVersion.Type.ToString(), actual.Type),
                        () => Assert.Equal(releaseVersion.Type.ToSearchDocumentTypeBoost(), actual.TypeBoost),
                        .. GetAssertTrimmedLinesEqual(expectedHtmlContent, actual.HtmlContent),
                    ]
                );
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

        [Fact]
        public async Task WhenReleaseServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            releaseService
                .Setup(mock => mock.GetRelease(publication.LatestPublishedReleaseVersionId!.Value, null))
                .ReturnsAsync(new NotFoundResult());

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context, releaseService.Object);

                // Act
                var outcome = await sut.GetLatestReleaseAsSearchableDocument(publication.Slug);

                // Assert
                releaseService.VerifyAll();
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

    private static ReleaseSearchableDocumentsService BuildService(
        ContentDbContext contentDbContext,
        IReleaseService? releaseService = null
    ) => new(contentDbContext, releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict));
}
