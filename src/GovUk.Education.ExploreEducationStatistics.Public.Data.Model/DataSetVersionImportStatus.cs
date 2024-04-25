using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataSetVersionImportStatus
{
    Queued,
    Validating,
    Importing,
    Complete,
    Cancelled,
    Failed,
}
