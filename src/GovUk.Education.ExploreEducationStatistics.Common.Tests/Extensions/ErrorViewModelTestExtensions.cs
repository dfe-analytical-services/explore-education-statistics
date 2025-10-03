using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ErrorViewModelTestExtensions
{
    public static T GetDetail<T>(this ErrorViewModel error)
        where T : notnull
    {
        var detailJson = Assert.IsType<JsonElement>(error.Detail);
        var detail = detailJson.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(detail);

        return detail;
    }
}
