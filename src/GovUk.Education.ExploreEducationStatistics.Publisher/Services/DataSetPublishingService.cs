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

public class DataSetPublishingService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext
    ) : IDataSetPublishingService
{
    public async Task PublishDataSets(IEnumerable<Guid> releaseVersionIds)
    {
        // Find all Data Files associated with the given ReleaseVersions.
        var dataFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => releaseVersionIds.Contains(rf.ReleaseVersionId) && rf.File.Type == FileType.Data)
            .Select(rf => rf.FileId)
            .ToListAsync();

        // Find all DataSets whose current Draft DataSetVersions reference any of the Release Versions' Data Files. 
        var releaseVersionDataSets = await publicDataDbContext
            .DataSets
            .AsNoTracking()
            .Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.LatestDraftVersion)
            .Where(dataSet => dataSet.LatestDraftVersion != null
                              && dataFileIds.Contains(dataSet.LatestDraftVersion.CsvFileId))
            .ToListAsync();

        // For each DataSet due to have a Draft DataSetVersion go live with the given Release Versions, publish the
        // Draft version, and ensure that the overarching DataSet is also published.
        releaseVersionDataSets
            .ForEach(dataSet => 
            {
                var currentDraftVersion = dataSet.LatestDraftVersion;
                currentDraftVersion!.Status = DataSetVersionStatus.Published;
                currentDraftVersion.Published = DateTimeOffset.UtcNow;

                // Mark the overarching DataSet as Published, and set a Published date if one has not previously been
                // set.
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
            });
        
        publicDataDbContext.DataSets.UpdateRange(releaseVersionDataSets);

        await publicDataDbContext.SaveChangesAsync();
    }
}
