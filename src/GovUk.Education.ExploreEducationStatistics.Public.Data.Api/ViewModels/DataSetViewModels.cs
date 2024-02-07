using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Describes and provides high-level information about a data set.
/// </summary>
public record DataSetViewModel
{
    /// <summary>
    /// The ID of the data set. 
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the data set.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// A summary of the data set’s contents.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// The status of the data set.Can be one of the following:
    ///
    /// - `Published` - the data set has been published and will receive updates
    /// - `Deprecated` - the data set is being discontinued and will no receive updates
    /// - `Unpublished` - the data set has been unpublished and can no longer be used
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required DataSetStatus Status { get; init; }

    /// <summary>
    /// The latest published data set version.
    /// </summary>
    public required DataSetLatestVersionViewModel LatestVersion { get; init; }

    /// <summary>
    /// The ID of the data set that supersedes this data set (if it has been deprecated).
    /// </summary>
    public Guid? SupersedingDataSetId { get; init; }
}

/// <summary>
/// Describes and provides high-level information about the latest version for a given data-set.
/// </summary>
public record DataSetLatestVersionViewModel
{
    /// <summary>
    /// The version number.
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    public required long TotalResults { get; init; }

    /// <summary>
    /// Describes a time period range in human-readable format.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumLabelJsonConverter<GeographicLevel>>))]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// The filters available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Filters { get; init; }

    /// <summary>
    /// The indicators available in the data set.
    /// </summary>
    public required IReadOnlyList<string> Indicators { get; init; }
}

/// <summary>
/// A paginated list of data-set summaries.
/// </summary>
public record DataSetPaginatedListViewModel : PaginatedListViewModel<DataSetViewModel>
{
    public DataSetPaginatedListViewModel(
        List<DataSetViewModel> results,
        int totalResults,
        int page,
        int pageSize)
        : base(
            results: results,
            totalResults: totalResults,
            page: page,
            pageSize: pageSize)
    {
    }

    [JsonConstructor]
    public DataSetPaginatedListViewModel(List<DataSetViewModel> results, PagingViewModel paging)
        : base(results, paging)
    {
    }
}
