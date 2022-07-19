#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class ReleaseControllerCachingTests : CacheServiceTestFixture
{
    [Fact]
    public async Task GetLatestRelease_NoCachedEntryExists()
    {
        var publicationSlug = "publication-a";
        
        var releaseService = new Mock<IReleaseService>(Strict);
        
        InMemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(publicationSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var release = BuildReleaseViewModel();

        releaseService
            .Setup(mock => mock.GetCachedViewModel(publicationSlug, null))
            .ReturnsAsync(release);

        var expectedCacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.HalfHourly, 30);
        
        InMemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetLatestReleaseCacheKey(publicationSlug), 
                release, 
                expectedCacheConfiguration, 
                null))
            .Returns(Task.CompletedTask);
        
        var controller = BuildReleaseController(releaseService.Object);

        var result = await controller.GetLatestRelease(publicationSlug);
        VerifyAllMocks(releaseService, InMemoryCacheService);

        result.AssertOkResult(release);
    }

    [Fact]
    public async Task GetLatestRelease_CachedEntryExists()
    {
        var publicationSlug = "publication-a";
        
        var release = BuildReleaseViewModel();
        
        InMemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(publicationSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(release);
        
        var controller = BuildReleaseController();

        var result = await controller.GetLatestRelease(publicationSlug);
        VerifyAllMocks(InMemoryCacheService);

        result.AssertOkResult(release);
    }
    
    [Fact]
    public async Task GetRelease_NoCachedEntryExists()
    {
        var publicationSlug = "publication-a";
        var releaseSlug = "release-a";

        var releaseService = new Mock<IReleaseService>(Strict);
        
        InMemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var release = BuildReleaseViewModel();

        releaseService
            .Setup(mock => mock.GetCachedViewModel(publicationSlug, releaseSlug))
            .ReturnsAsync(release);

        var expectedCacheConfiguration = new InMemoryCacheConfiguration(ExpirySchedule.HalfHourly, 30);
        
        InMemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                release, 
                expectedCacheConfiguration, 
                null))
            .Returns(Task.CompletedTask);
        
        var controller = BuildReleaseController(releaseService.Object);

        var result = await controller.GetRelease(publicationSlug, releaseSlug);
        VerifyAllMocks(releaseService, InMemoryCacheService);

        result.AssertOkResult(release);
    }
    
    [Fact]
    public async Task GetRelease_CachedEntryExists()
    {
        var publicationSlug = "publication-a";
        var releaseSlug = "release-a";

        var release = BuildReleaseViewModel();
        
        InMemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(release);

        var controller = BuildReleaseController();

        var result = await controller.GetRelease(publicationSlug, releaseSlug);
        VerifyAllMocks(InMemoryCacheService);

        result.AssertOkResult(release);
    }

    private static ReleaseViewModel BuildReleaseViewModel()
    {
        var releaseId = Guid.NewGuid();

        return new ReleaseViewModel(
            new CachedReleaseViewModel(releaseId)
            {
                Type = new()
                {
                    Title = "National Statistics"
                }
            },
            new PublicationViewModel
            {
                Releases = AsList(new ReleaseTitleViewModel
                {
                    Id = releaseId
                })
            });
    }

    private static ReleaseController BuildReleaseController(
        IReleaseService? releaseService = null
    )
    {
        return new(
            releaseService ?? Mock.Of<IReleaseService>(Strict)
        );
    }
}