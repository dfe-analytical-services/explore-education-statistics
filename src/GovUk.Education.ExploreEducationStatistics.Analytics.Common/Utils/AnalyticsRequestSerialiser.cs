using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;

public static class AnalyticsRequestSerialiser
{
    public static string SerialiseRequest<TAnalyticsRequest>(TAnalyticsRequest requestToSerialise) =>
        JsonSerializationUtils.Serialize(
            obj: requestToSerialise,
            formatting: Formatting.Indented,
            orderedProperties: true,
            camelCase: true
        );
}
