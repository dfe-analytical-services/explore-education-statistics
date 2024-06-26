using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionChangeState : IFilterOptionDetails
{
    public required string PublicId { get; set; }

    public required string Label { get; set; }

    public required string FilterId { get; set; }

    public bool? IsAggregate { get; set; } = null!;
}
