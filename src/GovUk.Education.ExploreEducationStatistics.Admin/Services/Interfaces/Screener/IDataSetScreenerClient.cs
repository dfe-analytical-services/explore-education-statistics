#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;

public interface IDataSetScreenerClient
{
    Task<List<DataSetScreenerProgressResponse>> GetScreenerProgress(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    );

    Task<List<DataSetScreenerCompletionReportResponse>> GetScreenerCompletionReports(
        IList<Guid> dataSetIds,
        CancellationToken cancellationToken
    );

    Task DeleteScreenerProgressAndCompletionFiles(IList<Guid> dataSetIds, CancellationToken cancellationToken);
}
