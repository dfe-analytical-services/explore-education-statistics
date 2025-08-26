#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class ReleaseControllerTests : CacheServiceTestFixture
{
    private const string PublicationSlug = "publication-a";
    private const string ReleaseSlug = "200";

    public ReleaseControllerTests()
    {
        MemoryCacheService
            .Setup(s => s.GetItem(
                It.IsAny<IMemoryCacheKey>(), typeof(ReleaseViewModel)))
            .Returns((object?)null);

        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<IMemoryCacheKey>(),
                It.IsAny<ReleaseViewModel>(),
                It.IsAny<MemoryCacheConfiguration>(),
                null));
    }

    [Fact]
    public async Task GetLatestRelease()
    {
        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
            .ReturnsAsync(releaseCacheViewModel);

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetLatestRelease(PublicationSlug);

        MockUtils.VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService);

        result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
            new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
    }

    [Fact]
    public async Task GetLatestReleaseAsSearchable()
    {
        // ARRANGE
        var themeId = Guid.NewGuid();
        const string themeTitle = "the theme";
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();
        var publishedTimestamp = new DateTime(2025, 02, 19, 11, 41, 00, DateTimeKind.Utc);
        const ReleaseType releaseType = ReleaseType.OfficialStatistics;

        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = publicationId,
            Title = "Publication Title",
            Summary = "This is the publication summary",
            Theme = new ThemeViewModelBuilder()
                .WithId(themeId)
                .WithTitle(themeTitle),
            Slug = "publication-slug",
            
        };
        var releaseCacheViewModel = new ReleaseCacheViewModel(releaseVersionId)
        {
            ReleaseId = releaseId,
            Published = publishedTimestamp,
            Title = "Release Title",
            SummarySection = new ContentSectionViewModelBuilder().AddHtmlContent("<p>This is the release summary</p>"),
            Type = releaseType,
            Slug = "release-slug",
            Content = 
            [
                new ContentSectionViewModelBuilder().WithHeading("section one").AddHtmlContent("<p>content section body one</p>"),
                new ContentSectionViewModelBuilder().WithHeading("section two").AddHtmlContent("<p>content section body two</p>"),
                new ContentSectionViewModelBuilder().WithHeading("section three").AddHtmlContent("<p>content section body three</p>"),
            ],
            HeadlinesSection = new ContentSectionViewModelBuilder().AddHtmlContent("<p>here is the headline content</p>")
        };

        var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
            .ReturnsAsync(releaseCacheViewModel);

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        // ACT
        var result = await controller.GetLatestReleaseAsSearchableDocument(PublicationSlug);

        // ASSERT
        MockUtils.VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService);

        var expectedHtmlContent = """
                                  <html>
                                     <head>
                                         <title>Publication Title</title>
                                     </head>
                                      <body>
                                          <h1>Publication Title</h1>
                                          <h2>Release Title</h2>
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

        var actual = result.Value;
        Assert.NotNull(actual);
        
        AssertAll(
            [
                () => Assert.Equal(releaseId, actual.ReleaseId),
                () => Assert.Equal(releaseVersionId, actual.ReleaseVersionId), 
                () => Assert.Equal(publishedTimestamp, actual.Published), 
                () => Assert.Equal(publicationId, actual.PublicationId),
                () => Assert.Equal("Publication Title", actual.PublicationTitle), 
                () => Assert.Equal("This is the publication summary", actual.Summary), 
                () => Assert.Equal(themeId, actual.ThemeId),
                () => Assert.Equal(themeTitle, actual.ThemeTitle),
                () => Assert.Equal("OfficialStatistics", actual.Type), 
                () => Assert.Equal(releaseType.ToSearchDocumentTypeBoost(), actual.TypeBoost), 
                () => Assert.Equal("publication-slug", actual.PublicationSlug), 
                () => Assert.Equal("release-slug", actual.ReleaseSlug), 
            ],
            GetAssertTrimmedLinesEqual(expectedHtmlContent, actual.HtmlContent));
    }

    private IEnumerable<Action> GetAssertTrimmedLinesEqual(string expectedLines, string actualLines)
    {
        // Trim each line then assert they are the same
        var expectedList = expectedLines.ToLines().Select(line => line.Trim()).ToList();
        var actualList = actualLines.ToLines().Select(line => line.Trim()).ToList();
        Assert.Equal(expectedList.Count, actualList.Count);
        return expectedList
            .Zip(actualList, (e, a) => (Expected:e, Actual:a))
            .Select(x => (Action)(() => Assert.Equal(x.Expected, x.Actual)));
    }

    [Fact]
    public async Task GetLatestRelease_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);

        var result = await controller.GetLatestRelease(PublicationSlug);

        MockUtils.VerifyAllMocks(publicationCacheService);

        result.AssertNotFoundResult();
    }
    
    [Fact]
    public async Task GetLatestReleaseAsSearchable_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);

        var result = await controller.GetLatestReleaseAsSearchableDocument(PublicationSlug);

        MockUtils.VerifyAllMocks(publicationCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetLatestRelease_ReleaseNotFound()
    {
        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };

        var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetLatestRelease(PublicationSlug);

        MockUtils.VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetRelease()
    {
        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, ReleaseSlug))
            .ReturnsAsync(releaseCacheViewModel);

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService);

        result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
            new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
    }

    [Fact]
    public async Task GetRelease_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);

        var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(publicationCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetRelease_ReleaseNotFound()
    {
        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };

        var methodologyCacheService = new Mock<IMethodologyCacheService>(MockBehavior.Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, ReleaseSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetLatestReleaseSummary()
    {
        var publicationCacheViewModel = new PublicationCacheViewModel();
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(
                PublicationSlug, null))
            .ReturnsAsync(releaseCacheViewModel);

        var controller = BuildReleaseController(
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);
        var result = await controller.GetLatestReleaseSummary(PublicationSlug);

        MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

        result.AssertOkResult(new ReleaseSummaryViewModel(
            releaseCacheViewModel, publicationCacheViewModel));
    }

    [Fact]
    public async Task GetLatestReleaseSummary_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);
        var result = await controller.GetLatestReleaseSummary(PublicationSlug);

        MockUtils.VerifyAllMocks(publicationCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetLatestReleaseSummary_ReleaseNotFound()
    {
        var publicationCacheViewModel = new PublicationCacheViewModel();

        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(
                PublicationSlug, null))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);
        var result = await controller.GetLatestReleaseSummary(PublicationSlug);

        MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetReleaseSummary()
    {
        var publicationCacheViewModel = new PublicationCacheViewModel();
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(
                PublicationSlug, ReleaseSlug))
            .ReturnsAsync(releaseCacheViewModel);

        var controller = BuildReleaseController(
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);
        var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

        result.AssertOkResult(new ReleaseSummaryViewModel(
            releaseCacheViewModel, publicationCacheViewModel));
    }

    [Fact]
    public async Task GetReleaseSummary_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(publicationCacheService: publicationCacheService.Object);
        var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(publicationCacheService);

        result.AssertNotFoundResult();
    }

    [Fact]
    public async Task GetReleaseSummary_ReleaseNotFound()
    {
        var publicationCacheViewModel = new PublicationCacheViewModel();

        var publicationCacheService = new Mock<IPublicationCacheService>(MockBehavior.Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(MockBehavior.Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(
                PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(
                PublicationSlug, ReleaseSlug))
            .ReturnsAsync(new NotFoundResult());

        var controller = BuildReleaseController(
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);
        var result = await controller.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        MockUtils.VerifyAllMocks(publicationCacheService, releaseCacheService);

        result.AssertNotFoundResult();
    }

    private static ReleaseCacheViewModel BuildReleaseCacheViewModel()
    {
        return new ReleaseCacheViewModel(Guid.NewGuid());
    }

    private static ReleaseController BuildReleaseController(
        IMethodologyCacheService? methodologyCacheService = null,
        IPublicationCacheService? publicationCacheService = null,
        IReleaseCacheService? releaseCacheService = null,
        IReleaseService? releaseService = null
    )
    {
        return new(
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(MockBehavior.Strict),
            publicationCacheService ?? Mock.Of<IPublicationCacheService>(MockBehavior.Strict),
            releaseCacheService ?? Mock.Of<IReleaseCacheService>(MockBehavior.Strict),
            releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict)
        );
    }
    
    private void AssertAll(params IEnumerable<Action>[] assertions) => Assert.All(assertions.SelectMany(a => a), assertion => assertion());
}
