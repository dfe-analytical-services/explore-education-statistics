using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A set of changes made to a data set version.
/// </summary>
public record DataSetVersionChangesViewModel
{
    public string? Notes { get; set; }
    
    public required DataSetVersionNumber VersionNumber { get; set; }
    /// <summary>
    /// Any major changes that were made to the data set.
    /// </summary>
    public required ChangeSetViewModel MajorChanges { get; init; }

    /// <summary>
    /// Any minor changes that were made to the data set.
    /// </summary>
    public required ChangeSetViewModel MinorChanges { get; init; }

    /// <summary>
    /// Change logs for any patch versions associated with this change log
    /// </summary>
    public List<DataSetVersionChangesViewModel>? PatchHistory { get; set; }
}

/// <summary>
/// A set of changes grouped by their type (major or minor).
/// </summary>
public record ChangeSetViewModel
{
    /// <summary>
    /// A list of any filter changes made to the data set.
    /// </summary>
    public IReadOnlyList<FilterChangeViewModel>? Filters { get; init; }

    /// <summary>
    /// A list of any filter option changes made to the data set.
    /// </summary>
    public IReadOnlyList<FilterOptionChangesViewModel>? FilterOptions { get; init; }

    /// <summary>
    /// A list of any geographic level changes made to the data set.
    /// </summary>
    public IReadOnlyList<GeographicLevelChangeViewModel>? GeographicLevels { get; init; }

    /// <summary>
    /// A list of any indicator changes made to the data set.
    /// </summary>
    public IReadOnlyList<IndicatorChangeViewModel>? Indicators { get; init; }

    /// <summary>
    /// A list of any location group changes made to the data set.
    /// </summary>
    public IReadOnlyList<LocationGroupChangeViewModel>? LocationGroups { get; init; }

    /// <summary>
    /// A list of any location option changes made to the data set.
    /// </summary>
    public IReadOnlyList<LocationOptionChangesViewModel>? LocationOptions { get; init; }

    /// <summary>
    /// A list of any time period changes made to the data set.
    /// </summary>
    public IReadOnlyList<TimePeriodOptionChangeViewModel>? TimePeriods { get; init; }
}

/// <summary>
/// A set of filter option changes and details of the filter they belong to.
/// </summary>
public record FilterOptionChangesViewModel
{
    /// <summary>
    /// The filter the option changes belong to.
    /// </summary>
    public required FilterViewModel Filter { get; init; }

    /// <summary>
    /// The list of filter option changes.
    /// </summary>
    public required IReadOnlyList<FilterOptionChangeViewModel> Options { get; init; }
}

/// <summary>
/// A set of location option changes and details of the geographic level they belong to.
/// </summary>
public record LocationOptionChangesViewModel
{
    /// <summary>
    /// The geographic level the changes belong to.
    /// </summary>
    public required GeographicLevelViewModel Level { get; init; }
    
    /// <summary>
    /// The list of location option changes.
    /// </summary>
    public required IReadOnlyList<LocationOptionChangeViewModel> Options { get; init; }
}

