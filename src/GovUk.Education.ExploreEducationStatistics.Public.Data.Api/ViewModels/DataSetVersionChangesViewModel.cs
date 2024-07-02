namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A set of changes made by a data set version.
/// </summary>
public record DataSetVersionChangesViewModel
{
    /// <summary>
    /// Any major changes that were made to the data set.
    /// </summary>
    public ChangeSetViewModel? MajorChanges { get; init; }

    /// <summary>
    /// Any minor changes that were made to the data set.
    /// </summary>
    public ChangeSetViewModel? MinorChanges { get; init; }
}

public record ChangeSetViewModel
{
    /// <summary>
    /// A list of any filter changes made to the data set.
    /// </summary>
    public IReadOnlyList<FilterChangeViewModel>? Filters { get; init; }

    /// <summary>
    /// A list of any filter option changes made to the data set.
    /// </summary>
    public IReadOnlyList<FilterOptionChangeViewModel>? FilterOptions { get; init; }

    /// <summary>
    /// A list of any geographic level changes made to the data set.
    /// </summary>
    public IReadOnlyList<GeographicLevelOptionChangeViewModel>? GeographicLevels { get; init; }

    /// <summary>
    /// A list of any indicator changes made to the data set.
    /// </summary>
    public IReadOnlyList<IndicatorChangeViewModel>? Indicators { get; init; }

    /// <summary>
    /// A list of any location level changes made to the data set.
    /// </summary>
    public IReadOnlyList<LocationGroupChangeViewModel>? LocationGroups { get; init; }

    /// <summary>
    /// A list of any location option changes made to the data set.
    /// </summary>
    public IReadOnlyList<LocationOptionChangeViewModel>? LocationOptions { get; init; }

    /// <summary>
    /// A list of any time period changes made to the data set.
    /// </summary>
    public IReadOnlyList<TimePeriodOptionChangeViewModel>? TimePeriods { get; init; }
}
