#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

[Collection(CacheServiceTests)]
public class ThemeControllerCachingTests : CacheServiceTestFixture
{
    [Fact]
    public void ThemeViewModelList_SerializeAndDeserialize()
    {
        var themes = ListOf(new ThemeViewModel(Guid.NewGuid(), "slug", "title", "summary"));
        var converted = DeserializeObject<List<ThemeViewModel>>(SerializeObject(themes));
        converted.AssertDeepEqualTo(themes);
    }
}
