using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsWriteStrategy
{
    bool CanHandle(BaseCaptureRequest request);

    Task Report(BaseCaptureRequest request);
}
