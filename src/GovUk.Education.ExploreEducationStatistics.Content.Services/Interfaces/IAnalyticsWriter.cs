using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsWriter
{
    Task ReportZipDownload(AnalyticsWriter.CaptureZipDownloadRequest request);

    Task ReportCsvDownload(AnalyticsWriter.CaptureCsvDownloadRequest request);
}
