using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

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
}

public record DataSetViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required DataSetStatus Status { get; init; }

    public required DataSetLatestVersionViewModel LatestVersion { get; init; }

    public Guid? SupersedingDataSetId { get; init; }
}

public record DataSetLatestVersionViewModel
{
    public required string Number { get; init; }

    public required DateTimeOffset Published { get; init; }

    public required long TotalResults { get; init; }

    public required TimePeriodRangeViewModel TimePeriods { get; init; }

    public required IReadOnlyList<GeographicLevel> GeographicLevels { get; init; }

    public required IReadOnlyList<string> Filters { get; init; }
    
    public required IReadOnlyList<string> Indicators { get; init; }
}

public record TimePeriodRangeViewModel
{
    public required string Start { get; set; }

    public required string End { get; set; }
}
