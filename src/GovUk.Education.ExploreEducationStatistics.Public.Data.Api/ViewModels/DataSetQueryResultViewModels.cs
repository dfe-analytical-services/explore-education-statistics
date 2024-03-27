using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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
    /// The filters associated with this result.
    ///
    /// This is a dictionary where the key is the filter ID and
    /// the value is the specific filter option ID.
    /// </summary>
    public required Dictionary<string, string> Filters { get; init; }

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
    public required Dictionary<string, string?> Locations { get; init; }

    /// <summary>
    /// The data values for the result's indicators.
    ///
    /// This is a dictionary where the key is the indicator ID
    /// and the value is the data value.
    /// </summary>
    public required Dictionary<string, string> Values { get; init; }

    /// <summary>
    /// The time period relevant to this result's data.
    /// </summary>
    public required TimePeriodViewModel TimePeriod { get; init; }
}
