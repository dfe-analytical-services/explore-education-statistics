using System.Collections.Immutable;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

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
    public static readonly IList<DataSetVersionImportStatus> TerminalStates = ImmutableList.Create(
        DataSetVersionImportStatus.Cancelled,
        DataSetVersionImportStatus.Complete,
        DataSetVersionImportStatus.Failed);
    
    public static readonly IList<DataSetVersionImportStatus> InProgressStates =
        EnumUtil
            .GetEnums<DataSetVersionImportStatus>()
            .ToList()
            .Except(TerminalStates)
            .ToImmutableList();
}
