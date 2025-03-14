namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IQueryAnalyticsWriter
{
    Task ReportDataSetVersionQuery(CaptureDataSetVersionQueryRequest request);
}
