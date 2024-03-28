namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public class ParquetIndicator
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public string Unit { get; set; } = string.Empty;

    public byte DecimalPlaces { get; set; }
}
