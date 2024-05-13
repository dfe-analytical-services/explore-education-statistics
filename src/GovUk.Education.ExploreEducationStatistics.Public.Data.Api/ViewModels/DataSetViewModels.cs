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
    /// - `Deprecated` - the data set is being discontinued and will no longer receive updates
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
/// A paginated list of data sets.
/// </summary>
public record DataSetPaginatedListViewModel : PaginatedListViewModel<DataSetViewModel>;

