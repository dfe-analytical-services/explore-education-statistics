#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

internal class DataSetVersionRepository(PublicDataDbContext publicDataDbContext) : IDataSetVersionRepository
{
    public async Task<DataSetVersion?> GetByReleaseFileId(
        Guid releaseFileId, 
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Where(dsv => dsv.ReleaseFileId == releaseFileId)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
