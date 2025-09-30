using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ProcessNextDataSetVersionMappingsFunctions(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionMappingService mappingService) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.CreateMappings)]
    public async Task CreateMappings(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.CreatingMappings, cancellationToken);
        await mappingService.CreateMappings(dataSetVersionImport.DataSetVersionId, dataSetVersionImport.DataSetVersionToReplaceId, cancellationToken);
    }

    [Function(ActivityNames.ApplyAutoMappings)]
    public async Task ApplyAutoMappings(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.AutoMapping, cancellationToken);
        await mappingService.ApplyAutoMappings(
            dataSetVersionImport.DataSetVersionId,
            dataSetVersionImport.DataSetVersionToReplaceId is not null,
            cancellationToken);
    }

    [Function(ActivityNames.CompleteNextDataSetVersionMappingProcessing)]
    public async Task CompleteNextDataSetVersionMappingProcessing(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ManualMapping, cancellationToken);

        var dataSetVersion = dataSetVersionImport.DataSetVersion;
        dataSetVersion.Status = DataSetVersionStatus.Mapping;

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
