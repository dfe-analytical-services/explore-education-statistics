using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class DataSetVersionPublishingService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext
    ) : IDataSetVersionPublishingService
{
    public async Task PublishDataSetVersions(IEnumerable<Guid> releaseVersionIds)
    {
        var dataFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => releaseVersionIds.Contains(rf.ReleaseVersionId) && rf.File.Type == FileType.Data)
            .Select(rf => rf.FileId)
            .ToListAsync();

        var dataSetVersionsToPublish = await publicDataDbContext
            .DataSetVersions
            .AsNoTracking()
            .Where(dataSetVersion => dataFileIds.Contains(dataSetVersion.CsvFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .ThenInclude(dataSet => dataSet.LatestLiveVersion)
            .ToListAsync();

        dataSetVersionsToPublish.ForEach(dataSetVersion =>
        {
            var dataSet = dataSetVersion.DataSet;
            var currentLiveVersion = dataSet.LatestLiveVersion;

            dataSet.Status = DataSetStatus.Published;
            dataSetVersion.Status = DataSetVersionStatus.Published;
            if (currentLiveVersion != null)
            {
                currentLiveVersion.Status = DataSetVersionStatus.Deprecated;
            }
            
            dataSetVersion.Published = DateTimeOffset.UtcNow;
            if (currentLiveVersion == null)
            {
                dataSet.Published = dataSetVersion.Published;
            }
            
            // Set the newly published DataSetVersion as the overarching DataSet's LatestLiveVersion,
            // and unset it from being the latest Draft version.
            dataSet.LatestLiveVersionId = dataSetVersion.Id;
            dataSet.LatestDraftVersionId = null;
        });
        
        publicDataDbContext.DataSetVersions.UpdateRange(dataSetVersionsToPublish);
        publicDataDbContext.DataSets.UpdateRange(dataSetVersionsToPublish.Select(dsv => dsv.DataSet));

        await publicDataDbContext.SaveChangesAsync();
    }
}
