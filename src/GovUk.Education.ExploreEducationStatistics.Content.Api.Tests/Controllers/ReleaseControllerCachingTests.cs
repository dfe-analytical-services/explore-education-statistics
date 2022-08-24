#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class ReleaseControllerCachingTests : CacheServiceTestFixture
{
    private const string HalfHourlyExpirySchedule = "*/30 * * * *";
    
    [Fact]
    public async Task GetLatestRelease_NoCachedEntryExists()
    {
        var publicationSlug = "publication-a";
        
        var releaseService = new Mock<IReleaseService>(Strict);
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(publicationSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var release = BuildReleaseViewModel();

        releaseService
            .Setup(mock => mock.GetCachedViewModel(publicationSlug, null))
            .ReturnsAsync(release);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));
        
        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetLatestReleaseCacheKey(publicationSlug), 
                release, 
                ItIs.DeepEqualTo(expectedCacheConfiguration), 
                null))
            .Returns(Task.CompletedTask);
        
        var controller = BuildReleaseController(releaseService.Object);

        var result = await controller.GetLatestRelease(publicationSlug);
        VerifyAllMocks(releaseService, MemoryCacheService);

        result.AssertOkResult(release);
    }

    [Fact]
    public async Task GetLatestRelease_CachedEntryExists()
    {
        var publicationSlug = "publication-a";
        
        var release = BuildReleaseViewModel();
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetLatestReleaseCacheKey(publicationSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(release);
        
        var controller = BuildReleaseController();

        var result = await controller.GetLatestRelease(publicationSlug);
        VerifyAllMocks(MemoryCacheService);

        result.AssertOkResult(release);
    }
    
    [Fact]
    public async Task GetRelease_NoCachedEntryExists()
    {
        var publicationSlug = "publication-a";
        var releaseSlug = "release-a";

        var releaseService = new Mock<IReleaseService>(Strict);
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(null);

        var release = BuildReleaseViewModel();

        releaseService
            .Setup(mock => mock.GetCachedViewModel(publicationSlug, releaseSlug))
            .ReturnsAsync(release);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            15, CrontabSchedule.Parse(HalfHourlyExpirySchedule));
        
        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                release, 
                ItIs.DeepEqualTo(expectedCacheConfiguration), 
                null))
            .Returns(Task.CompletedTask);
        
        var controller = BuildReleaseController(releaseService.Object);

        var result = await controller.GetRelease(publicationSlug, releaseSlug);
        VerifyAllMocks(releaseService, MemoryCacheService);

        result.AssertOkResult(release);
    }
    
    [Fact]
    public async Task GetRelease_CachedEntryExists()
    {
        var publicationSlug = "publication-a";
        var releaseSlug = "release-a";

        var release = BuildReleaseViewModel();
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new GetReleaseCacheKey(publicationSlug, releaseSlug), 
                typeof(ReleaseViewModel)))
            .ReturnsAsync(release);

        var controller = BuildReleaseController();

        var result = await controller.GetRelease(publicationSlug, releaseSlug);
        VerifyAllMocks(MemoryCacheService);

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