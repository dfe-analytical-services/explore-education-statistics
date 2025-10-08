#nullable enable
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class OptionsExtensions
{
    public static OptionsWrapper<T> ToOptionsWrapper<T>(this T options)
        where T : class => new(options);
}
