namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;

public record CreateDataSetResponseViewModel
{
    public required Guid DataSetId { get; init; }
    public required Guid DataSetVersionId { get; init; }
    public required Guid InstanceId { get; init; }
}
