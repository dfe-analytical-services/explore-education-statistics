using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ImportMetadataFunction(PublicDataDbContext publicDataDbContext, IDataSetMetaService dataSetMetaService)
    : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.ImportMetadata)]
    public async Task ImportMetadata([ActivityTrigger] Guid instanceId, CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ImportingMetadata, cancellationToken);
        await dataSetMetaService.CreateDataSetVersionMeta(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }
}
