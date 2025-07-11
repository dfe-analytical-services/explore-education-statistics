namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface IRequestFileProcessor
{
    Task Process();
    
    string SourceDirectory { get; }
    string ReportsDirectory { get; }
}
