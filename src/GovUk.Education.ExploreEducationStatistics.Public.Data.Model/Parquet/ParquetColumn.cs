namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public record ParquetColumn
{
    public required string ColumnName { get; init; }

    public required string ColumnType { get; init; }

    public required string Null { get; init; }

    public required string Key { get; init; }

    public required string Default { get; init; }

    public required string Extra { get; init; }
}
