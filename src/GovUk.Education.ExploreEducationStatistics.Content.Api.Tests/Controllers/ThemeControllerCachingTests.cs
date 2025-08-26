#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class ThemeControllerCachingTests : CacheServiceTestFixture
{
    private readonly IList<ThemeViewModel> _themes =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Slug = "slug1",
            Title = "title1",
            Summary = "summary1"
        },
        new()
        {
            Id = Guid.NewGuid(),
            Slug = "slug2",
            Title = "title2",
            Summary = "summary2"
        }
    ];

    [Fact]
    public async Task ListThemes_NoCachedEntryExists()
    {
        var themeService = new Mock<IThemeService>(Strict);

        MemoryCacheService
            .Setup(s => s.GetItem(
                new ListThemesCacheKey(),
                typeof(IList<ThemeViewModel>)))
            .Returns((object?)null);

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
