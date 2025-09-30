#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;

public static class StringExtensions
{
    public static string DefaultsTo(this string? value, string defaultValue) =>
        string.IsNullOrEmpty(value) ? defaultValue : value;
}
