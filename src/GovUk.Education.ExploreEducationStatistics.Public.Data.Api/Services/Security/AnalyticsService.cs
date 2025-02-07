using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;

public class AnalyticsService : IAnalyticsService
{
    public void ReportDataSetVersionQuery(
        DataSetVersion dataSetVersion,
        DataSetQueryRequest query,
        int resultsCount,
        int totalRowsCount,
        DateTime startTime,
        DateTime endTime)
    {
        var request = new CaptureDataSetVersionQueryRequest(
            dataSetId: dataSetVersion.DataSetId,
            dataSetVersionId: dataSetVersion.Id,
            dataSetVersion: dataSetVersion.SemVersion().ToString(),
            resultsCount: resultsCount,
            totalRowsCount: totalRowsCount,
            startTime: startTime,
            endTime: endTime,
            queryJson: JsonConvert.SerializeObject(query));

        Console.Out.WriteLine(JsonConvert.SerializeObject(request));
        // TODO EES-5830 - implement the process of putting the message
        // onto the queue
    }
}
