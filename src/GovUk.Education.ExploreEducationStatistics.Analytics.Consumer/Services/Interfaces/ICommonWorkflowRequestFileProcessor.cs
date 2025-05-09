using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;

public interface ICommonWorkflowRequestFileProcessor
{
    string SourceDirectory();

    string ReportsDirectory();

    string ReportsFilenameSuffix();
    
    void InitialiseDuckDb(DuckDbConnection duckDbConnection);

    void ProcessSourceFile(string filepath, DuckDbConnection duckDbConnection);

    void CreateParquetReport(string reportFilepath, DuckDbConnection duckDbConnection);
}
