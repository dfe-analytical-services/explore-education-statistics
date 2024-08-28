namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

internal static class ActivityNames
{
    public const string CopyCsvFiles = nameof(CopyCsvFilesFunction.CopyCsvFiles);
    public const string ImportMetadata = nameof(ImportMetadataFunction.ImportMetadata);
    public const string ImportData = nameof(ImportDataFunction.ImportData);
    public const string WriteDataFiles = nameof(WriteDataFilesFunction.WriteDataFiles);
    public const string HandleProcessingFailure = nameof(HandleProcessingFailureFunction.HandleProcessingFailure);

    public const string CompleteInitialDataSetVersionProcessing =
        nameof(ProcessInitialDataSetVersionFunction.CompleteInitialDataSetVersionProcessing);

    public const string CreateMappings = nameof(ProcessNextDataSetVersionMappingsFunction.CreateMappings);
    public const string ApplyAutoMappings = nameof(ProcessNextDataSetVersionMappingsFunction.ApplyAutoMappings);
    public const string CompleteNextDataSetVersionMappingProcessing =
        nameof(ProcessNextDataSetVersionMappingsFunction.CompleteNextDataSetVersionMappingProcessing);

    public const string UpdateFileStoragePath =
        nameof(ProcessCompletionOfNextDataSetVersionFunction.UpdateFileStoragePath);
    public const string CompleteNextDataSetVersionImportProcessing =
        nameof(ProcessCompletionOfNextDataSetVersionFunction.CompleteNextDataSetVersionImportProcessing);
}
