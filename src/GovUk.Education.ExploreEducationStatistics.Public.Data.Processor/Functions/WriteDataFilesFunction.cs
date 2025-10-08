using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class WriteDataFilesFunction(PublicDataDbContext publicDataDbContext, IParquetService parquetService)
    : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.WriteDataFiles)]
    public async Task WriteDataFiles([ActivityTrigger] Guid instanceId, CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.WritingDataFiles, cancellationToken);
        await parquetService.WriteDataFiles(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }
}
