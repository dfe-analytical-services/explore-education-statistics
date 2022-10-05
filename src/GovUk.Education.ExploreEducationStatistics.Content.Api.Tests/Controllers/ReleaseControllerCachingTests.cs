#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class ReleaseControllerCachingTests : CacheServiceTestFixture
{
    private const string HalfHourlyExpirySchedule = "*/30 * * * *";

    private const string PublicationSlug = "publication-a";
    private const string ReleaseSlug = "200";

    [Fact]
    public async Task GetLatestRelease_NoCachedEntryExists()
    {
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(PublicationSlug),
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);
        
        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, null))
            .ReturnsAsync(releaseCacheViewModel);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetLatestReleaseCacheKey(PublicationSlug),
                It.IsAny<ReleaseViewModel>(),
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null))
            .Returns(Task.CompletedTask);

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetLatestRelease(PublicationSlug);

        VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService,
            MemoryCacheService);

        result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
            new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
    }

    [Fact]
    public async Task GetLatestRelease_CachedEntryExists()
    {
        var releaseViewModel = new ReleaseViewModel(BuildReleaseCacheViewModel(), new PublicationViewModel());

        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(PublicationSlug),
                typeof(ReleaseViewModel)))
            .ReturnsAsync(releaseViewModel);

        var controller = BuildReleaseController();

        var result = await controller.GetLatestRelease(PublicationSlug);
        VerifyAllMocks(MemoryCacheService);

        result.AssertOkResult(releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_NoCachedEntryExists()
    {
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(PublicationSlug, ReleaseSlug),
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var methodologySummaries = new List<MethodologyVersionSummaryViewModel>();
        var publicationCacheViewModel = new PublicationCacheViewModel
        {
            Id = Guid.NewGuid()
        };
        var releaseCacheViewModel = BuildReleaseCacheViewModel();

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(publicationCacheViewModel.Id))
            .ReturnsAsync(methodologySummaries);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(publicationCacheViewModel);

        releaseCacheService.Setup(mock => mock.GetRelease(PublicationSlug, ReleaseSlug))
            .ReturnsAsync(releaseCacheViewModel);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            15, CrontabSchedule.Parse(HalfHourlyExpirySchedule));

        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetReleaseCacheKey(PublicationSlug, ReleaseSlug),
                It.IsAny<ReleaseViewModel>(),
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null))
            .Returns(Task.CompletedTask);

        var controller = BuildReleaseController(
            methodologyCacheService: methodologyCacheService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object);

        var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(methodologyCacheService,
            publicationCacheService,
            releaseCacheService,
            MemoryCacheService);

        result.AssertOkResult(new ReleaseViewModel(releaseCacheViewModel,
            new PublicationViewModel(publicationCacheViewModel, methodologySummaries)));
    }

    [Fact]
    public async Task GetRelease_CachedEntryExists()
    {
        var releaseViewModel = new ReleaseViewModel(BuildReleaseCacheViewModel(), new PublicationViewModel());

        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(PublicationSlug, ReleaseSlug),
                typeof(ReleaseViewModel)))
            .ReturnsAsync(releaseViewModel);

        var controller = BuildReleaseController();

        var result = await controller.GetRelease(PublicationSlug, ReleaseSlug);
        VerifyAllMocks(MemoryCacheService);

        result.AssertOkResult(releaseViewModel);
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
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
            releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
            releaseService ?? Mock.Of<IReleaseService>(Strict)
        );
    }
}
