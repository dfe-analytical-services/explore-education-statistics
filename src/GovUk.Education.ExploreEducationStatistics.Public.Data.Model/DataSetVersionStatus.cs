using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum DataSetVersionStatus
{
    Processing,
    Failed,
    Mapping,
    Draft,
    Finalising,
    Published,
    Deprecated,
    Withdrawn,
    Cancelled,
}
