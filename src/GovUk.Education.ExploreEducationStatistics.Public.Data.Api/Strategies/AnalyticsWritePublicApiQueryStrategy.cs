using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;
using Newtonsoft.Json;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicApiQueryStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    DateTimeProvider dateTimeProvider,
    ILogger<AnalyticsWritePublicApiQueryStrategy> logger
    ) : IAnalyticsWriteStrategy<CaptureDataSetVersionQueryRequest>
{
    public Type RequestType => typeof(CaptureDataSetVersionQueryRequest);

    public string GetDirectory() => analyticsPathResolver.PublicApiQueriesDirectoryPath();

    public string GetFilename(CaptureDataSetVersionQueryRequest request)
    {
            return $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_{request.DataSetVersionId}_{RandomUtils.RandomString()}.json";
    }

    public string SerialiseRequest(CaptureDataSetVersionQueryRequest request)
    {
        var requestToSerialise = request with { Query = DataSetQueryNormalisationUtil.NormaliseQuery(request.Query) };

        return JsonSerializationUtils.Serialize(
                obj: requestToSerialise,
                formatting: Formatting.Indented,
                orderedProperties: true,
                camelCase: true);
    }
}
