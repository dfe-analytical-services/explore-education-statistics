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

public static class DataSetVersionImportStatusConstants
{
    public static readonly List<DataSetVersionImportStatus> TerminalStates =
    [
        DataSetVersionImportStatus.Cancelled,
        DataSetVersionImportStatus.Complete,
        DataSetVersionImportStatus.Failed
    ];
} 
