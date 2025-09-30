using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides high-level information about a data set version.
/// </summary>
public class DataSetVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor), 2.1.1 (patch).
    /// </summary>
    /// <example>1.0</example>
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
    /// <example>Major</example>
    public required DataSetVersionType Type { get; init; }

    /// <summary>
    /// The version’s status. Can be one of the following:
    ///
    /// - `Published` - the version is published and can be used
    /// - `Deprecated` - the version is being deprecated and will not be usable in the future
    /// - `Withdrawn` - the version has been withdrawn and can no longer be used
    /// </summary>
    /// <example>Published</example>
    public required DataSetVersionStatus Status { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    /// <example>2024-03-01T09:30:00+00:00</example>
    public DateTimeOffset? Published { get; init; }

    /// <summary>
    /// When the version was withdrawn.
    /// </summary>
    /// <example>2024-06-01T12:00:00+00:00</example>
    public DateTimeOffset? Withdrawn { get; init; }

    /// <summary>
    /// Any notes about this version and its changes.
    /// </summary>
    /// <example>Some notes about the version.</example>
    public required string Notes { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>]
    /// <example>1000000</example>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The file that this data set version is based on.
    /// </summary>
    public required DataSetVersionFileViewModel File { get; init; }

    /// <summary>
    /// The statistical release that this version was published with.
    /// </summary>
    public required DataSetVersionReleaseViewModel Release { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    /// <example>["National", "Regional", "Local authority"]</example>
    [JsonConverter(
        typeof(ReadOnlyListJsonConverter<
            GeographicLevel,
            EnumToEnumLabelJsonConverter<GeographicLevel>
        >)
    )]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// The filters available in the data set.
    /// </summary>
    /// <example>["Characteristic", "School type"]</example>
    public required IReadOnlyList<string> Filters { get; init; }

    /// <summary>
    /// The indicators available in the data set.
    /// </summary>
    /// <example>["Authorised absence rate", "Overall absence rate"]</example>
    public required IReadOnlyList<string> Indicators { get; init; }
}

/// <summary>
/// A paginated list of data set versions.
/// </summary>
public record DataSetVersionPaginatedListViewModel
    : PaginatedListViewModel<DataSetVersionViewModel>;

/// <summary>
/// Provides high-level information about the latest version of a data set.
/// </summary>
public record DataSetLatestVersionViewModel
{
    /// <summary>
    /// The version number. Follows semantic versioning e.g. 2.0 (major), 1.1 (minor).
    /// </summary>
    /// <example>2.0</example>
    public required string Version { get; init; }

    /// <summary>
    /// When the version was published.
    /// </summary>
    /// <example>2024-03-01T09:30:00+00:00</example>
    public required DateTimeOffset Published { get; init; }

    /// <summary>
    /// The total number of results available to query in the data set.
    /// </summary>
    /// <example>1000000</example>
    public required long TotalResults { get; init; }

    /// <summary>
    /// The file that this data set version is based on.
    /// </summary>
    public required DataSetVersionFileViewModel File { get; init; }

    /// <summary>
    /// The time period range covered by the data set.
    /// </summary>
    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    /// <summary>
    /// The geographic levels available in the data set.
    /// </summary>
    /// <example>["National", "Regional", "Local authority"]</example>
    [JsonConverter(
        typeof(ReadOnlyListJsonConverter<
            GeographicLevel,
            EnumToEnumLabelJsonConverter<GeographicLevel>
        >)
    )]
    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    /// <summary>
    /// The filters available in the data set.
    /// </summary>
    /// <example>["Characteristic", "School type"]</example>
    public required IReadOnlyList<string> Filters { get; init; }

    /// <summary>
    /// The indicators available in the data set.
    /// </summary>
    /// <example>["Authorised absence rate" "Overall absence rate"]</example>
    public required IReadOnlyList<string> Indicators { get; init; }
}

/// <summary>
/// Details about the file a data set version is based on.
/// </summary>
public record DataSetVersionFileViewModel
{
    /// <summary>
    /// The ID of the file.
    /// </summary>
    /// <example>e0754872-3206-4918-aad4-029eaaae191f</example>
    public required Guid Id { get; init; }
}

/// <summary>
/// Details about the statistical release a data set version was published with.
/// </summary>
public record DataSetVersionReleaseViewModel
{
    /// <summary>
    /// The title of the release.
    /// </summary>
    /// <example>Spring term 2023/24</example>
    public required string Title { get; init; }

    /// <summary>
    /// The slug of the release.
    /// </summary>
    /// <example>2023-24-spring-term</example>
    public required string Slug { get; init; }
}
