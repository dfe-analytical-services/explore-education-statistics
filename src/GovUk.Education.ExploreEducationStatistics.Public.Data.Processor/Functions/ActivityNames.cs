namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

internal static class ActivityNames
{
    public const string CopyCsvFiles = nameof(CopyCsvFilesFunction.CopyCsvFiles);
    public const string ImportMetadata = nameof(ImportMetadataFunction.ImportMetadata);
    public const string HandleProcessingFailure = nameof(HandleProcessingFailureFunction.HandleProcessingFailure);

    public const string ImportData = nameof(ProcessInitialDataSetVersionFunction.ImportData);
    public const string WriteDataFiles = nameof(ProcessInitialDataSetVersionFunction.WriteDataFiles);
    public const string CompleteInitialDataSetVersionProcessing = 
        nameof(ProcessInitialDataSetVersionFunction.CompleteInitialDataSetVersionProcessing);

    public const string CreateMappings = nameof(ProcessNextDataSetVersionFunction.CreateMappings);
    public const string ApplyAutoMappings = nameof(ProcessNextDataSetVersionFunction.ApplyAutoMappings);
    public const string CompleteNextDataSetVersionMappingProcessing = 
        nameof(ProcessNextDataSetVersionFunction.CompleteNextDataSetVersionMappingProcessing);
}
