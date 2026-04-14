#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using RichardSzalay.MockHttp;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.LinkChecker;

public class LinksCheckerTests
{
    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly DataFixture _dataFixture = new();

    public class ExtractReleaseLinksAsyncTests : LinksCheckerTests
    {
        [Fact]
        public async Task ExtractLinksFromLatestPublishedVersions()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication().Generate();
            var release = _dataFixture.DefaultRelease().WithPublication(publication).Generate();

            var releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(DateTime.UtcNow.AddDays(-1))
                .Generate();

            var htmlContent =
                @"<div>
                <a href=""https://find-statistics/pub/test/release/2024"">Test Link 1</a>
                <a href=""https://find-statistics/pub/other"">Test Link 2</a>
            </div>";

            var contentSection = new ContentSection { Heading = "Summary" };

            var htmlBlock = new HtmlBlock
            {
                Body = htmlContent,
                ReleaseVersion = releaseVersion,
                ContentSection = contentSection,
            };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            context.HtmlBlocks.Add(htmlBlock);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);

            var firstLink = result[0];
            Assert.Equal("https://find-statistics/pub/test/release/2024", firstLink.Url);
            Assert.Equal("Test Link 1", firstLink.LinkText);
            Assert.Equal(publication.Title, firstLink.PublicationTitle);
            Assert.Equal("Summary", firstLink.Heading);

            var secondLink = result[1];
            Assert.Equal("https://find-statistics/pub/other", secondLink.Url);
            Assert.Equal("Test Link 2", secondLink.LinkText);
        }

        [Fact]
        public async Task IgnoresUnpublishedVersions()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication();
            var release = _dataFixture.DefaultRelease().WithPublication(publication);

            var unpublishedVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(null); // Not published

            var htmlContent =
                @"<div>
                <a href=""https://find-statistics/pub/test"">Should Not Be Found</a>
            </div>";

            var htmlBlock = new HtmlBlock { Body = htmlContent, ReleaseVersion = unpublishedVersion };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(unpublishedVersion);
            context.HtmlBlocks.Add(htmlBlock);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task IgnoresHtmlBlocksWithoutFindStatisticsLinks()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication();
            var release = _dataFixture.DefaultRelease().WithPublication(publication);

            var releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(DateTime.UtcNow.AddDays(-1));

            var htmlContent =
                @"<div>
                <a href=""https://www.gov.uk"">Gov.uk Link</a>
                <a href=""https://www.bbc.co.uk"">BBC Link</a>
            </div>";

            var htmlBlock = new HtmlBlock { Body = htmlContent, ReleaseVersion = releaseVersion };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            context.HtmlBlocks.Add(htmlBlock);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task IgnoresEmptyAndNullBodies()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication();
            var release = _dataFixture.DefaultRelease().WithPublication(publication);

            var releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(DateTime.UtcNow.AddDays(-1));

            var htmlBlockWithNullBody = new HtmlBlock { Body = null, ReleaseVersion = releaseVersion };

            var htmlBlockWithEmptyBody = new HtmlBlock { Body = "", ReleaseVersion = releaseVersion };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            context.HtmlBlocks.Add(htmlBlockWithNullBody);
            context.HtmlBlocks.Add(htmlBlockWithEmptyBody);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task OnlyUsesLatestPublishedVersionPerRelease()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication();
            var release = _dataFixture.DefaultRelease().WithPublication(publication).Generate();

            var version1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(DateTime.UtcNow.AddDays(-5))
                .Generate();

            var version2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(2)
                .WithPublished(DateTime.UtcNow.AddDays(-1))
                .Generate();

            var htmlBlock1 = new HtmlBlock
            {
                Body = @"<a href=""https://find-statistics/old"">Old Version Link</a>",
                ReleaseVersion = version1,
            };

            var htmlBlock2 = new HtmlBlock
            {
                Body = @"<a href=""https://find-statistics/new"">New Version Link</a>",
                ReleaseVersion = version2,
            };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.AddRange(version1, version2);
            context.HtmlBlocks.AddRange(htmlBlock1, htmlBlock2);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.Single(result);
            Assert.Equal("https://find-statistics/new", result[0].Url);
        }

        [Fact]
        public async Task DecodesHtmlEntitiesInLinkText()
        {
            // Arrange
            var publication = _dataFixture.DefaultPublication();
            var release = _dataFixture.DefaultRelease().WithPublication(publication);

            var releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(release)
                .WithVersion(1)
                .WithPublished(DateTime.UtcNow.AddDays(-1));

            var htmlContent =
                @"<div>
                <a href=""https://find-statistics/test"">Link &amp; Text</a>
            </div>";

            var htmlBlock = new HtmlBlock { Body = htmlContent, ReleaseVersion = releaseVersion };

            using var context = InMemoryApplicationDbContext();
            context.Publications.Add(publication);
            context.Releases.Add(release);
            context.ReleaseVersions.Add(releaseVersion);
            context.HtmlBlocks.Add(htmlBlock);
            await context.SaveChangesAsync();

            var service = BuildService();

            // Act
            var result = await service.ExtractReleaseLinksAsync(context);

            // Assert
            Assert.Single(result);
            Assert.Equal("Link & Text", result[0].LinkText);
        }

        [Fact]
        public async Task RespectsCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            using var context = InMemoryApplicationDbContext();
            var service = BuildService();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                service.ExtractReleaseLinksAsync(context, cts.Token)
            );
        }
    }

    public class TestReleaseLinksAsyncTests : LinksCheckerTests
    {
        [Fact]
        public async Task ReturnsSuccessfulStatusCodesForValidLinks()
        {
            // Arrange
            _mockHttp
                .When(HttpMethod.Head, "https://find-statistics/publication/test")
                .Respond(System.Net.HttpStatusCode.OK);

            _mockHttp
                .When(HttpMethod.Head, "https://find-statistics/publication/test2")
                .Respond(System.Net.HttpStatusCode.OK);

            var linkDetails = new List<LinkDetails>
            {
                new(
                    PublicationTitle: "Publication 1",
                    PublicationSlug: "pub-1",
                    ReleaseSlug: "release-1",
                    Heading: "Section 1",
                    Url: "https://find-statistics/publication/test",
                    LinkText: "Link 1"
                ),
                new(
                    PublicationTitle: "Publication 2",
                    PublicationSlug: "pub-2",
                    ReleaseSlug: "release-2",
                    Heading: "Section 2",
                    Url: "https://find-statistics/publication/test2",
                    LinkText: "Link 2"
                ),
            };

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.Equal(200, item.StatusCode));
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ReturnsDifferentStatusCodesForDifferentResponses()
        {
            // Arrange
            _mockHttp.When(HttpMethod.Head, "https://find-statistics/found").Respond(System.Net.HttpStatusCode.OK);

            _mockHttp
                .When(HttpMethod.Head, "https://find-statistics/not-found")
                .Respond(System.Net.HttpStatusCode.NotFound);

            _mockHttp
                .When(HttpMethod.Head, "https://find-statistics/forbidden")
                .Respond(System.Net.HttpStatusCode.Forbidden);

            var linkDetails = new List<LinkDetails>
            {
                CreateLinkDetails("https://find-statistics/found", "Link 1"),
                CreateLinkDetails("https://find-statistics/not-found", "Link 2"),
                CreateLinkDetails("https://find-statistics/forbidden", "Link 3"),
            };

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, a => a.StatusCode == 200);
            Assert.Contains(result, a => a.StatusCode == 404);
            Assert.Contains(result, a => a.StatusCode == 403);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task ReturnsZeroStatusCodeForNetworkException()
        {
            // Arrange
            _mockHttp
                .Expect(HttpMethod.Head, "https://find-statistics/network-error")
                .Throw(new HttpRequestException("Network error"));

            var linkDetails = new List<LinkDetails>
            {
                CreateLinkDetails("https://find-statistics/network-error", "Link with network error"),
            };

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Single(result);
            Assert.Equal(0, result[0].StatusCode);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task Returns408ForTaskCanceledTimeout()
        {
            // Arrange
            _mockHttp
                .Expect(HttpMethod.Head, "https://find-statistics/timeout")
                .Throw(new TaskCanceledException("Request timeout"));

            var linkDetails = new List<LinkDetails>
            {
                CreateLinkDetails("https://find-statistics/timeout", "Link with timeout"),
            };

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Single(result);
            Assert.Equal(408, result[0].StatusCode);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestsAllLinksInParallel()
        {
            // Arrange
            var linkCount = 10;
            for (int i = 0; i < linkCount; i++)
            {
                _mockHttp
                    .When(HttpMethod.Head, $"https://find-statistics/test-{i}")
                    .Respond(System.Net.HttpStatusCode.OK);
            }

            var linkDetails = Enumerable
                .Range(0, linkCount)
                .Select(i => CreateLinkDetails($"https://find-statistics/test-{i}", $"Link {i}"))
                .ToList();

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Equal(linkCount, result.Count);
            Assert.All(result, item => Assert.Equal(200, item.StatusCode));
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task PreservesLinkDetailsInResponse()
        {
            // Arrange
            _mockHttp.Expect(HttpMethod.Head, "https://find-statistics/test").Respond(System.Net.HttpStatusCode.OK);

            var originalLink = new LinkDetails(
                PublicationTitle: "Test Publication",
                PublicationSlug: "test-publication",
                ReleaseSlug: "test-release",
                Heading: "Test Section",
                Url: "https://find-statistics/test",
                LinkText: "Test Link Text"
            );

            var linkDetails = new List<LinkDetails> { originalLink };

            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(linkDetails);

            // Assert
            Assert.Single(result);
            Assert.Equal(originalLink.PublicationTitle, result[0].PublicationTitle);
            Assert.Equal(originalLink.PublicationSlug, result[0].PublicationSlug);
            Assert.Equal(originalLink.ReleaseSlug, result[0].ReleaseSlug);
            Assert.Equal(originalLink.Heading, result[0].SectionHeading);
            Assert.Equal(originalLink.Url, result[0].Url);
            Assert.Equal(originalLink.LinkText, result[0].LinkText);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task RespectsCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var linkDetails = new List<LinkDetails> { CreateLinkDetails("https://find-statistics/test", "Link") };

            var service = BuildService();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                service.TestReleaseLinksAsync(linkDetails, cts.Token)
            );
        }

        [Fact]
        public async Task ReturnsEmptyListForEmptyInput()
        {
            // Arrange
            var service = BuildService();

            // Act
            var result = await service.TestReleaseLinksAsync(new List<LinkDetails>());

            // Assert
            Assert.Empty(result);
        }
    }

    private LinksChecker BuildService()
    {
        var httpClient = _mockHttp.ToHttpClient();
        return new LinksChecker(httpClient);
    }

    private static LinkDetails CreateLinkDetails(
        string url,
        string linkText,
        string publicationTitle = "Test Publication",
        string publicationSlug = "test-publication",
        string releaseSlug = "test-release",
        string? sectionHeading = null
    )
    {
        return new LinkDetails(
            PublicationTitle: publicationTitle,
            PublicationSlug: publicationSlug,
            ReleaseSlug: releaseSlug,
            Heading: sectionHeading,
            Url: url,
            LinkText: linkText
        );
    }
}
