using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task ReportDataSetVersionQuery(
        Guid dataSetId,
        Guid dataSetVersionId,
        string semVersion,
        string dataSetTitle,
        DataSetQueryRequest query,
        int resultsCount,
        int totalRowsCount,
        DateTime startTime,
        DateTime endTime);
}
