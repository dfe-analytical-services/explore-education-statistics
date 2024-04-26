namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;

public record ProcessorTriggerResponseViewModel
{
    public required Guid DataSetVersionId { get; init; }
    public required Guid InstanceId { get; init; }
}
