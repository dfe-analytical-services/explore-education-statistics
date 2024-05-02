using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class DataSetVersionRepository(PublicDataDbContext publicDataDbContext) : IDataSetVersionRepository
{
    public async Task SetStatus(Guid dataSetVersionId,
        DataSetVersionStatus status,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await publicDataDbContext.DataSetVersions
            .FirstAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);
        dataSetVersion.Status = status;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
