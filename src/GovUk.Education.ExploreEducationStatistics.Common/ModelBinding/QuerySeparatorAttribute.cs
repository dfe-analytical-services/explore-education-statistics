#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

/// <summary>
/// Configure <see cref="SeparatedQueryModelBinder"/> to bind from a query parameter
/// that contains some values separated by delimiters (e.g. a comma). This allows
/// binding to models that are not supported by default (e.g. enumerable strings).
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class QuerySeparatorAttribute : Attribute
{
    /// <summary>
    /// The separator used in the query parameter. Defaults to a comma.
    /// </summary>
    public string Separator { get; set; }

    public QuerySeparatorAttribute(string separator = ",")
    {
        Separator = separator;
    }
}
