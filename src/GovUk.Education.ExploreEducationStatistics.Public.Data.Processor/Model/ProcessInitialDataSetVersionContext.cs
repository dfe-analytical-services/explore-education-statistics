namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;

public record ProcessInitialDataSetVersionContext
{
    public required Guid DataSetVersionId { get; init; }
}
