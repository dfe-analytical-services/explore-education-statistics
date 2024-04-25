using System;
using System.Collections.Generic;
using System.Linq;
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
            .AsNoTracking()
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync();

        return (await publicDataDbContext
            .DataSetVersions
            .AsNoTracking()
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.ReleaseFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .ToListAsync())
            .Select(dataSetVersion => new DataSetVersionStatusSummary(
                Id: dataSetVersion.Id,
                Title: dataSetVersion.DataSet.Title,
                Status: dataSetVersion.Status)
            )
            .ToList();
    }
}

public record DataSetVersionStatusSummary(Guid Id, string Title, DataSetVersionStatus Status);
