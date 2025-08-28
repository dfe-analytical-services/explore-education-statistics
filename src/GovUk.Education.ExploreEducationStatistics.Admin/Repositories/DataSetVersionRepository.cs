using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories;

public class DataSetVersionRepository(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext) : IDataSetVersionRepository
{
    public async Task<List<DataSetVersion>> GetDataSetVersions(Guid releaseVersionId)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Select(rf => rf.Id)
            .ToListAsync();

        return await publicDataDbContext
            .DataSetVersions
            .Where(dsv => releaseFileIds.Contains(dsv.Release.ReleaseFileId))
            .ToListAsync();
    }
}
