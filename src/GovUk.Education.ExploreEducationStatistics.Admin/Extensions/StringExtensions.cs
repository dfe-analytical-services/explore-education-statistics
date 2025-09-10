#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class StringExtensions
{
    public static string ThrowIfBlank(this string? value, string description) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentNullException($"Value is missing:{description}")
            : value;
}
