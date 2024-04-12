namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public record ParquetLocationOption
{
    public required int Id { get; set; }

    public required string Label { get; set; }

    public required string Level { get; set; }

    public required string PublicId { get; set; }

    public string? Code { get; set; }

    public string? OldCode { get; set; }

    public string? Ukprn { get; set; }

    public string? Urn { get; set; }

    public string? LaEstab { get; set; }
}
