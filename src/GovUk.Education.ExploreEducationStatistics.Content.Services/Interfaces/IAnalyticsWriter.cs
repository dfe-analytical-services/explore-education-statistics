using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsWriter
{
    Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
