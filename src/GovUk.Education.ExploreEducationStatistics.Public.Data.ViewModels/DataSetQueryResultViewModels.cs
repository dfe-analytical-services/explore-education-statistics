using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A paginated list of results from a data set query.
/// </summary>
public record DataSetQueryPaginatedResultsViewModel : PaginatedListViewModel<DataSetQueryResultViewModel>
{
    /// <summary>
    /// A list of warnings, highlighting any potential issues with the request.
    /// </summary>
    public required List<WarningViewModel> Warnings { get; init; }
}

/// <summary>
/// A result from a data set query containing its values and any facets relevant to it.
/// </summary>
public record DataSetQueryResultViewModel
{
    /// <summary>
    /// The time period relevant to this result's data.
    /// </summary>
    public required TimePeriodViewModel TimePeriod { get; init; }

    /// <summary>
    /// The geographic level relevant to this result's data.
    /// </summary>
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel GeographicLevel { get; init; }

    /// <summary>
    /// The locations relevant to this result's data.
    ///
    /// This is a dictionary where the key is the location's geographic
    /// level and the value is the location's ID.
    /// </summary>
    /// <example>
    /// {
    ///     "NAT": "04bTr",
    ///     "REG": "4veOu",
    ///     "LA": "owqlK"
    /// }
    /// </example>
    public required Dictionary<string, string> Locations { get; init; }

    /// <summary>
    /// The filters associated with this result.
    ///
    /// This is a dictionary where the key is the filter ID and
    /// the value is the specific filter option ID.
    /// </summary>
    /// <example>
    /// {
    ///     "ups2K": "n0WqP",
    ///     "j51wV": "AnZsi",
    ///     "hAkBQ": "dvB4z"
    /// }
    /// </example>
    public required Dictionary<string, string> Filters { get; init; }

    /// <summary>
    /// The data values for the result's indicators.
    ///
    /// This is a dictionary where the key is the indicator ID
    /// and the value is the data value.
    /// </summary>
    /// <example>
    /// {
    ///     "wLcft": "23593018",
    ///     "4S8Ou": "50.342",
    ///     "9kVFg": "25369172"
    /// }
    /// </example>
    public required Dictionary<string, string> Values { get; init; }
}
