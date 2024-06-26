using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterChangeState : IFilterDetails
{
    public required int Id { get; set; }

    public required string PublicId { get; set; }

    public required string Label { get; set; }

    public string Hint { get; set; } = string.Empty;
}
