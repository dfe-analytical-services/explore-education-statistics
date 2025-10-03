#nullable enable
using FluentValidation.Results;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class ValidationFailureExtensions
{
    private static readonly HashSet<string> IgnoredMessagePlaceholders =
    [
        "PropertyName",
        "PropertyPath",
        "PropertyValue",
        "CollectionIndex",
    ];

    public static Dictionary<string, object?> GetErrorDetail(this ValidationFailure failure)
    {
        var detail =
            (failure.FormattedMessagePlaceholderValues ?? [])
                .Where(kv => !IgnoredMessagePlaceholders.Contains(kv.Key))
                .ToDictionary(kv => kv.Key.ToLowerFirst(), kv => kv.Value) as Dictionary<string, object?>;

        if (!detail.ContainsKey("value"))
        {
            detail["value"] = failure.AttemptedValue;
        }

        if (failure.CustomState is null)
        {
            return detail;
        }

        var type = failure.CustomState.GetType();

        if (type is { IsClass: true, IsPrimitive: false, IsEnum: false, IsValueType: false })
        {
            var customKeyValues = failure
                .CustomState.ToDictionary()
                .ToDictionary(kv => kv.Key.CamelCase(), kv => kv.Value);

            foreach (var keyValue in customKeyValues)
            {
                detail[keyValue.Key] = keyValue.Value;
            }
        }
        else
        {
            detail["state"] = failure.CustomState;
        }

        return detail;
    }
}
