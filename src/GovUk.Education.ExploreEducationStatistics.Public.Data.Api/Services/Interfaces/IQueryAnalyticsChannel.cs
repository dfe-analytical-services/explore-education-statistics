namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IQueryAnalyticsChannel
{
    Task WriteQuery(CaptureDataSetVersionQueryRequest request, CancellationToken cancellationToken);
    
    ValueTask<CaptureDataSetVersionQueryRequest> ReadQuery(CancellationToken cancellationToken);
}
