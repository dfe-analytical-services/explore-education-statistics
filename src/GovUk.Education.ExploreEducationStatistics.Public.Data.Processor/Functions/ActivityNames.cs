namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

internal static class ActivityNames
{
    public const string CopyCsvFiles = nameof(CopyCsvFilesFunction.CopyCsvFiles);
    public const string ImportMetadata = nameof(ImportMetadataFunction.ImportMetadata);
    public const string ImportData = nameof(ImportDataFunction.ImportData);
    public const string WriteDataFiles = nameof(WriteDataFilesFunction.WriteDataFiles);
    public const string HandleProcessingFailure = nameof(HandleProcessingFailureFunction.HandleProcessingFailure);

    public const string CompleteInitialDataSetVersionProcessing = nameof(
        CompleteInitialDataSetVersionProcessingFunction.CompleteInitialDataSetVersionProcessing
    );

    public const string CreateMappings = nameof(ProcessNextDataSetVersionMappingsFunctions.CreateMappings);
    public const string ApplyAutoMappings = nameof(ProcessNextDataSetVersionMappingsFunctions.ApplyAutoMappings);
    public const string CompleteNextDataSetVersionMappingProcessing = nameof(
        ProcessNextDataSetVersionMappingsFunctions.CompleteNextDataSetVersionMappingProcessing
    );

    public const string CreateChanges = nameof(ProcessCompletionOfNextDataSetVersionFunctions.CreateChanges);
    public const string CompleteNextDataSetVersionImportProcessing = nameof(
        ProcessCompletionOfNextDataSetVersionFunctions.CompleteNextDataSetVersionImportProcessing
    );
}
