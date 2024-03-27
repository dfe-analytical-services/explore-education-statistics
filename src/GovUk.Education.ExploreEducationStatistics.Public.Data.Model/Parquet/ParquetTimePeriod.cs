namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public record ParquetTimePeriod
{
    public required int Id { get; set; }

    public required string Period { get; set; }

    public required string Identifier { get; set; }
}
