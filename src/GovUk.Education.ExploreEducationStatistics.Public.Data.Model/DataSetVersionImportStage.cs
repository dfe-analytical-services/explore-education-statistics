using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataSetVersionImportStage
{
    Created,
    Completing
}
