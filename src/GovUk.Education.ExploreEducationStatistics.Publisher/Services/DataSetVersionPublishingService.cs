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
            .ToListAsync();

        if (dataSetVersionsToPublish.Count == 0)
        {
            return;
        }
        
        dataSetVersionsToPublish.ForEach(dataSetVersion =>
        {
            var dataSet = dataSetVersion.DataSet;
            dataSetVersion.Status = DataSetVersionStatus.Published;
            dataSetVersion.Published = DateTime.UtcNow;
            dataSet.Status = DataSetStatus.Published;
            
            // Set the newly published DataSetVersion as the overarching DataSet's LatestLiveVersion,
            // and unset it from being the latest Draft version.
            dataSet.LatestLiveVersionId = dataSetVersion.Id;
            dataSet.LatestDraftVersionId = null;
        });

        await publicDataDbContext.SaveChangesAsync();
    }
}
