using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;

public static class WarningViewModelTestExtensions
{
    public static T GetDetail<T>(this WarningViewModel error)
        where T : notnull
    {
        var detailJson = Assert.IsType<JsonElement>(error.Detail);
        var detail = detailJson.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(detail);

        return detail;
    }
}
