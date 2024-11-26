namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;

public record LongRunningOrchestrationContext
{
    public required int DurationSeconds { get; init; }
}
