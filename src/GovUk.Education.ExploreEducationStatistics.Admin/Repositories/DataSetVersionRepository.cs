using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Semver;

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
    
    public async Task<DataSetVersion> GetDataSetVersion(
        Guid dataSetId,
        SemVersion version,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .Where(dsv => dsv.VersionPatch == version.Patch)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

}
