namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public string Hint { get; set; } = string.Empty;
}
