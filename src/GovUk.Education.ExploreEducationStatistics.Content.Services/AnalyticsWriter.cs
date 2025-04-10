#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriter(
    IEnumerable<IAnalyticsWriteStrategy> writerStrategies) : IAnalyticsWriter

{
    public async Task Report(BaseCaptureRequest request)
    {
        var strategy = writerStrategies
            .Single(s => s.CanHandle(request));

        await strategy.Report(request);
    }
}
