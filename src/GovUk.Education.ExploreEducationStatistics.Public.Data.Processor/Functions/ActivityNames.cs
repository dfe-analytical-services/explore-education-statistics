namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

internal static class ActivityNames
{
    public const string CopyCsvFiles = nameof(CopyCsvFilesFunction.CopyCsvFiles);
    public const string ImportMetadata = nameof(ImportMetadataFunction.ImportMetadata);
    public const string HandleProcessingFailure = nameof(HandleProcessingFailureFunction.HandleProcessingFailure);

    public const string ImportData = nameof(ProcessInitialDataSetVersionFunction.ImportData);
    public const string WriteDataFiles = nameof(ProcessInitialDataSetVersionFunction.WriteDataFiles);

    public const string CompleteProcessing = nameof(CompleteProcessingFunction.CompleteProcessing);
}
