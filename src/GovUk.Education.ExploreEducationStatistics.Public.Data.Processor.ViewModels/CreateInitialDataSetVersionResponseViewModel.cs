namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;

public record CreateInitialDataSetVersionResponseViewModel
{
    public required Guid DataSetVersionId { get; init; }
    public required Guid InstanceId { get; init; }
}
