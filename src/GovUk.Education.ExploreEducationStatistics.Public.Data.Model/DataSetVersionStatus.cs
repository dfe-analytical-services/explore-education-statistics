using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataSetVersionStatus
{
    Processing,
    Failed,
    Mapping,
    Draft,
    Published,
    Deprecated,
    Withdrawn
}
