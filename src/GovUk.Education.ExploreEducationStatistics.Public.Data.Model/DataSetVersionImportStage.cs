using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataSetVersionImportStage
{
    Pending,
    CopyingCsvFiles,
    ImportingMetadata,
    CreatingMappings,
    AutoMapping,
    ManualMapping,
    CreatingChanges,
    ImportingData,
    WritingDataFiles,
    Completing,
}

public static class DataSetVersionImportStageExtensions
{
    public static DataSetVersionImportStage PreviousStage(this DataSetVersionImportStage stage)
    {
        var stages = EnumUtil.GetEnums<DataSetVersionImportStage>();
        var prevIndex = stages.IndexOf(stage) - 1;

        return prevIndex == -1
            ? throw new ArgumentOutOfRangeException(
                nameof(stage),
                stage,
                $"No previous stage has been defined for '{stage}'"
            )
            : stages[prevIndex];
    }
}
