namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterMeta
{
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    public string Hint { get; set; } = string.Empty;

    public required List<FilterOptionMeta> Options { get; set; }
}
