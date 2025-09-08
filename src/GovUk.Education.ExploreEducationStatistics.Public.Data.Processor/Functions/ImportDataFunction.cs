using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class ImportDataFunction(PublicDataDbContext publicDataDbContext, IDataDuckDbRepository dataDuckDbRepository)
    : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    [Function(ActivityNames.ImportData)]
    public async Task ImportData([ActivityTrigger] Guid instanceId, CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        await UpdateImportStage(dataSetVersionImport, DataSetVersionImportStage.ImportingData, cancellationToken);
        await dataDuckDbRepository.CreateDataTable(dataSetVersionImport.DataSetVersionId, cancellationToken);
    }
}
