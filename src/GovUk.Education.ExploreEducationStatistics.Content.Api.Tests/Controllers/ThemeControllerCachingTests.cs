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
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class ThemeControllerCachingTests : CacheServiceTestFixture
{
    private readonly IList<ThemeViewModel> _themes = ListOf(
        new ThemeViewModel(Guid.NewGuid(), "slug1", "title1", "summary1"),
        new ThemeViewModel(Guid.NewGuid(), "slug2", "title2", "summary2"));
    
    [Fact]
    public async Task ListThemes_NoCachedEntryExists()
    {
        var themeService = new Mock<IThemeService>(Strict);
        
        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListThemesCacheKey(),
                typeof(IList<ThemeViewModel>)))
            .Returns(null);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10, CrontabSchedule.Parse(HalfHourlyExpirySchedule));
        
        MemoryCacheService
            .Setup(s => s.SetItem<object>(
                new ListThemesCacheKey(),
                _themes,
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                null));
        
        themeService
            .Setup(s => s.ListThemes())
            .ReturnsAsync(_themes);
        
        var controller = BuildController(themeService.Object);

        var result = await controller.ListThemes();
        
        VerifyAllMocks(MemoryCacheService, themeService);

        Assert.Equal(_themes, result);
    }
    
    [Fact]
    public async Task ListThemes_CachedEntryExists()
    {
        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListThemesCacheKey(),
                typeof(IList<ThemeViewModel>)))
            .Returns(_themes);
        
        var controller = BuildController();

        var result = await controller.ListThemes();
        
        VerifyAllMocks(MemoryCacheService);

        Assert.Equal(_themes, result);
    }
    
    [Fact]
    public void ThemeViewModelList_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<List<ThemeViewModel>>(SerializeObject(_themes));
        converted.AssertDeepEqualTo(_themes);
    }

    private static ThemeController BuildController(IThemeService? themeService = null)
    {
        return new(
            Mock.Of<IMethodologyCacheService>(),
            themeService ?? Mock.Of<IThemeService>(Strict)
        );
    }
}
