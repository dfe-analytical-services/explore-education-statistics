using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CreateMappingsFunction(
    PublicDataDbContext publicDataDbContext,
    IDataSetMetaService dataSetMetaService) : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.CreateMappings)]
    public async Task CreateMappings(
        [ActivityTrigger] Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ImportingMetadata, cancellationToken);

        var (filters, locations, metaSummary) = 
            await dataSetMetaService.ReadDataSetVersionMetaForMappings(
                dataSetVersionId: dataSetVersionImport.DataSetVersionId, 
                cancellationToken);
        
        // TODO EES-4945 - implement the creation of mappings here.
    }
}
