using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// Provides high-level information about a data set.
/// </summary>
public record DataSetViewModel
{
    /// <summary>
    /// The ID of the data set. 
    /// </summary>
    /// <example>7588c2d6-9e8a-4d84-8f19-6b8d52a01fbd</example>
    public required Guid Id { get; init; }

    /// <summary>
    /// The title of the data set.
    /// </summary>
    /// <example>Absence rates by geographic level</example>
    public required string Title { get; init; }

    /// <summary>
    /// A summary of the data set’s contents.
    /// </summary>
    /// <example>Absence information for all enrolments in schools.</example>
    public required string Summary { get; init; }

    /// <summary>
    /// The status of the data set. Can be one of the following:
    ///
    /// - `Published` - the data set has been published and will receive updates
    /// - `Deprecated` - the data set is being discontinued and will no longer receive updates
    /// - `Withdrawn` - the data set has been withdrawn and can no longer be used
    /// </summary>
    /// <example>Published</example>
    public required DataSetStatus Status { get; init; }

    /// <summary>
    /// The ID of the data set that supersedes this data set (if it has been deprecated).
    /// </summary>
    /// <example>2118a6df-4934-4a1f-ad2e-4589d2b9ccaf</example>
    public Guid? SupersedingDataSetId { get; init; }

    /// <summary>
    /// The latest published data set version.
    /// </summary>
    public DataSetLatestVersionViewModel? LatestVersion { get; init; }
}

/// <summary>
/// A paginated list of data sets.
/// </summary>
public record DataSetPaginatedListViewModel : PaginatedListViewModel<DataSetViewModel>;

