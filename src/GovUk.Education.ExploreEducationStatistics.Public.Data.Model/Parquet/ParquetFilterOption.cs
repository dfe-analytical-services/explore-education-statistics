namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public record ParquetFilterOption
{
    public required int Id { get; set; }

    public required string Label { get; set; }

    public required string PublicId { get; set; }

    public required string ColumnName { get; set; }
}
