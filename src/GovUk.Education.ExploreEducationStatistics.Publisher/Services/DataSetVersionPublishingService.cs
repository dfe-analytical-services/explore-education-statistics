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

        var releaseVersionDataSets = await publicDataDbContext
            .DataSets
            .AsNoTracking()
            .Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.LatestDraftVersion)
            .Where(dataSet => dataSet.LatestDraftVersion != null
                              && dataFileIds.Contains(dataSet.LatestDraftVersion.CsvFileId))
            .ToListAsync();

        releaseVersionDataSets
            .ForEach(dataSet => 
            {
                var currentDraftVersion = dataSet.LatestDraftVersion;
                currentDraftVersion!.Status = DataSetVersionStatus.Published;
                currentDraftVersion.Published = DateTimeOffset.UtcNow;

                var currentLiveVersion = dataSet.LatestLiveVersion;
                if (currentLiveVersion != null)
                {
                    currentLiveVersion.Status = DataSetVersionStatus.Deprecated;
                }
                
                dataSet.Status = DataSetStatus.Published;
                dataSet.Published ??= currentDraftVersion.Published;
                
                // Set the newly published DataSetVersion as the overarching DataSet's LatestLiveVersion,
                // and unset it from being the latest Draft version.
                dataSet.LatestLiveVersion = currentDraftVersion;
                dataSet.LatestLiveVersionId = currentDraftVersion.Id;
                dataSet.LatestDraftVersion = null;
                dataSet.LatestDraftVersionId = null;
                
                publicDataDbContext.DataSets.Update(dataSet);
                publicDataDbContext.DataSetVersions.Update(currentDraftVersion);

                if (currentLiveVersion != null)
                {
                    publicDataDbContext.DataSetVersions.Update(currentLiveVersion);
                }
            });
        
        publicDataDbContext.DataSets.UpdateRange(releaseVersionDataSets);

        await publicDataDbContext.SaveChangesAsync();
    }
}
