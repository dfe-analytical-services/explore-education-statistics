using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json.Converters;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public record DataSetViewModel
{
    /// <summary>
    /// The ID of the data set. 
    /// <br/><br/>
    /// thing
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
    /// The status of the data set. Can be one of the following: 
    /// <br/><br/>
    /// Published - the data set has been published and will receive updates \
    /// Deprecated - the data set is being discontinued and will no receive updates \
    /// Unpublished - the data set has been unpublished and can no longer be used 
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

public record TimePeriodRangeViewModel
{
    /// <summary>
    /// The starting time period in human-readable format.
    /// </summary>
    public required string Start { get; set; }

    /// <summary>
    /// The ending time period in human-readable format.
    /// </summary>
    public required string End { get; set; }
}

public record PaginatedDataSetViewModel : PaginatedListViewModel<DataSetViewModel>
{
    public PaginatedDataSetViewModel(
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
    public PaginatedDataSetViewModel(List<DataSetViewModel> results, PagingViewModel paging)
        : base(results, paging)
    {
    }
}
