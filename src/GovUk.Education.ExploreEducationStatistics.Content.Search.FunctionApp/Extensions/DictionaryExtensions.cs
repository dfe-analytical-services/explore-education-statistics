namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class DictionaryExtensions
{
    public static string ToDetailedString<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary) =>
        dictionary is null ? "<null>" :
        !dictionary.Any() ? "<empty>" :
        string.Join("|", dictionary.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
}
