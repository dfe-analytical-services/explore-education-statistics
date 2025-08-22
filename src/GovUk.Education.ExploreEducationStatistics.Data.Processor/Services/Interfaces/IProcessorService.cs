namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IProcessorService
{
    Task ProcessStage1(Guid importId);

    Task ProcessStage2(Guid importId);

    Task ProcessStage3(Guid importId);
}
