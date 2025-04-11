using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriter(
    IEnumerable<IAnalyticsWriteStrategy> writeStrategies) : IAnalyticsWriter
{
    public async Task Report(BaseCaptureRequest request, CancellationToken cancellationToken)
    {
        var strategy = writeStrategies
            .Single(s => s.CanHandle(request));

        await strategy.Report(request, cancellationToken);
    }
}
