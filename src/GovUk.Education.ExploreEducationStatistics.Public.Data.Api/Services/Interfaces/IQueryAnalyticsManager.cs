namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IQueryAnalyticsManager
{
    Task AddQuery(CaptureDataSetVersionQueryRequest request, CancellationToken cancellationToken);
    
    ValueTask<CaptureDataSetVersionQueryRequest> ReadQuery(CancellationToken cancellationToken);
}
