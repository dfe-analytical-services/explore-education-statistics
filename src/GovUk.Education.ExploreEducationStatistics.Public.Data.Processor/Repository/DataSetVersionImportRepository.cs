using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class DataSetVersionImportRepository(PublicDataDbContext publicDataDbContext) : IDataSetVersionImportRepository
{
    public async Task UpdateStage(Guid dataSetVersionId,
        DataSetVersionImportStage stage,
        CancellationToken cancellationToken)
    {
        // This currently assumes a single import per DataSetVersion
        var dataSetVersionImport = await publicDataDbContext.DataSetVersionImports
            .SingleAsync(i => i.DataSetVersionId == dataSetVersionId, cancellationToken: cancellationToken);
        dataSetVersionImport.Stage = stage;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
