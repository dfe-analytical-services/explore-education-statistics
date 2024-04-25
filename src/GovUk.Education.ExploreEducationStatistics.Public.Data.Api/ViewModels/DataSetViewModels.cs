using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides high-level information about a data set.
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
    /// The status of the data set. Can be one of the following:
    ///
    /// - `Published` - the data set has been published and will receive updates
    /// - `Deprecated` - the data set is being discontinued and will no receive updates
    /// - `Withdrawn` - the data set has been withdrawn and can no longer be used
    /// </summary>
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
/// Provides high-level information about the latest version of a data set.
/// </summary>
public record DataSetLatestVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor).
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumValueJsonConverter<GeographicLevel>>))]
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
/// A paginated list of data sets.
/// </summary>
public record DataSetPaginatedListViewModel : PaginatedListViewModel<DataSetViewModel>;

public class DataSetVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor).
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// The version type. Can be one of the following:
    ///
    /// - `Major` - backwards incompatible changes are being introduced
    /// - `Minor` - backwards compatible changes are being introduced
    /// 
    /// Major versions typically indicate that some action may be required 
    /// to ensure code that consumes the data set continues to work.
    /// 
    /// Minor versions should not cause issues in the functionality of existing code.
    /// </summary>
    public required DataSetVersionType Type { get; init; }

    /// <summary>
    /// The version’s status. Can be one of the following:
    ///
    /// - `Published` - the version is published and can be used
    /// - `Deprecated` - the version is being deprecated and will not be usable in the future
    /// - `Withdrawn` - the version has been withdrawn and can no longer be used
    /// </summary>
    public required DataSetVersionStatus Status { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// When the version was withdrawn.
    /// </summary>
    public DateTimeOffset? Withdrawn { get; init; }

    /// <summary>
    /// Any notes about this version and its changes.
    /// </summary>
    public required string Notes { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    [JsonConverter(typeof(ListJsonConverter<GeographicLevel, EnumToEnumValueJsonConverter<GeographicLevel>>))]
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
/// A paginated list of data set versions.
/// </summary>
public record DataSetVersionPaginatedListViewModel : PaginatedListViewModel<DataSetVersionViewModel>;
