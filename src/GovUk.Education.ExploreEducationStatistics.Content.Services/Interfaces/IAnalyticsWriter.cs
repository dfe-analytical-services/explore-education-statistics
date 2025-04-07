using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsWriter
{
    public Task ReportZipDownload(AnalyticsWriter.CaptureZipDownloadRequest request);
}
