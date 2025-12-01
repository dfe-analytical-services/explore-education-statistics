using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NCrontab;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cache.CronSchedules;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class ThemeControllerCachingTests
{
    private readonly IList<ThemeViewModel> _themes =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Slug = "slug1",
            Title = "title1",
            Summary = "summary1",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Slug = "slug2",
            Title = "title2",
            Summary = "summary2",
        },
    ];

    [Fact]
    public async Task ListThemes_NoCachedEntryExists()
    {
        var themeService = new Mock<IThemeService>(Strict);

        var memoryCacheService = new Mock<IMemoryCacheService>(Strict);

        memoryCacheService
            .Setup(s => s.GetMemoryCacheOptions())
            .Returns(new MemoryCacheServiceOptions { Enabled = true });

        memoryCacheService
            .Setup(s => s.GetItem(new ListThemesCacheKey(), typeof(IList<ThemeViewModel>)))
            .Returns((object?)null);

        var expectedCacheConfiguration = new MemoryCacheConfiguration(
            10,
            CrontabSchedule.Parse(HalfHourlyExpirySchedule)
        );

        var cachingTime = DateTime.UtcNow;

        memoryCacheService.Setup(s =>
            s.SetItem<object>(
                new ListThemesCacheKey(),
                _themes,
                ItIs.DeepEqualTo(expectedCacheConfiguration),
                cachingTime
            )
        );

        themeService.Setup(s => s.ListThemes()).ReturnsAsync(_themes);

        var controller = BuildController(
            themeService.Object,
            memoryCacheService: memoryCacheService.Object,
            timeProvider: new FakeTimeProvider(cachingTime)
        );

        var result = await controller.ListThemes();

        VerifyAllMocks(memoryCacheService, themeService);

        Assert.Equal(_themes, result);
    }

    [Fact]
    public async Task ListThemes_CachedEntryExists()
    {
        var memoryCacheService = new Mock<IMemoryCacheService>(Strict);

        memoryCacheService
            .Setup(s => s.GetMemoryCacheOptions())
            .Returns(new MemoryCacheServiceOptions { Enabled = true });

        memoryCacheService
            .Setup(s => s.GetItem(new ListThemesCacheKey(), typeof(IList<ThemeViewModel>)))
            .Returns(_themes);

        var controller = BuildController(memoryCacheService: memoryCacheService.Object);

        var result = await controller.ListThemes();

        VerifyAllMocks(memoryCacheService);

        Assert.Equal(_themes, result);
    }

    [Fact]
    public void ThemeViewModelList_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<List<ThemeViewModel>>(SerializeObject(_themes));
        converted.AssertDeepEqualTo(_themes);
    }

    private static ThemeController BuildController(
        IThemeService? themeService = null,
        IMemoryCacheService? memoryCacheService = null,
        TimeProvider? timeProvider = null
    )
    {
        return new(
            Mock.Of<IMethodologyCacheService>(),
            themeService ?? Mock.Of<IThemeService>(Strict),
            memoryCacheService ?? Mock.Of<IMemoryCacheService>(Strict),
            logger: Mock.Of<ILogger<ThemeController>>(),
            timeProvider: timeProvider ?? new FakeTimeProvider(DateTime.UtcNow)
        );
    }
}
