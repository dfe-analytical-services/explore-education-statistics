using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public abstract class BaseProcessDataSetVersionFunction(PublicDataDbContext publicDataDbContext)
{
    protected async Task<DataSetVersionImport> GetDataSetVersionImport(
        Guid instanceId,
        CancellationToken cancellationToken
    )
    {
        return await publicDataDbContext
            .DataSetVersionImports.Include(i => i.DataSetVersion)
            .SingleAsync(i => i.InstanceId == instanceId, cancellationToken: cancellationToken);
    }

    protected async Task UpdateImportStage(
        DataSetVersionImport dataSetVersionImport,
        DataSetVersionImportStage stage,
        CancellationToken cancellationToken
    )
    {
        dataSetVersionImport.Stage = stage;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
