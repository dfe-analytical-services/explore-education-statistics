#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext) 
    : IDataSetVersionService
{
    public async Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(Guid releaseVersionId)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync();

        return await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.ReleaseFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .Select(dataSetVersion => new DataSetVersionStatusSummary(
                dataSetVersion.Id,
                dataSetVersion.DataSet.Title,
                dataSetVersion.Status)
            )
            .ToListAsync();
    }

    public async Task<bool> FileHasVersion(
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .AnyAsync(dsv => dsv.ReleaseFileId == releaseFileId, cancellationToken);
    }
}

public record DataSetVersionStatusSummary(Guid Id, string Title, DataSetVersionStatus Status);
