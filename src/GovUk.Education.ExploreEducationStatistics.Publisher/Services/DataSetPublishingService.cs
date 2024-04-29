using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
    public async Task PublishDataSets(Guid[] releaseVersionIds)
    {
        await PromoteDraftDataSetVersions(releaseVersionIds);
        await UpdatePreviousReleaseFileIds(releaseVersionIds);
    }

    private async Task PromoteDraftDataSetVersions(IEnumerable<Guid> releaseVersionIds)
    {
        // Find all Data Files associated with the given ReleaseVersions.
        var releaseFileIds = (await GetReleaseFilesForReleaseVersions(releaseVersionIds))
            .Select(releaseFile => releaseFile.Id);

        // Find all DataSets whose current Draft DataSetVersions reference any of the Release Versions' Data Files. 
        var releaseVersionDataSets = await publicDataDbContext
            .DataSets
            .AsNoTracking()
            .Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.LatestDraftVersion)
            .Where(dataSet => dataSet.LatestDraftVersion != null
                              && releaseFileIds.Contains(dataSet.LatestDraftVersion.ReleaseFileId))
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

    private async Task UpdatePreviousReleaseFileIds(IEnumerable<Guid> releaseVersionIds)
    {
        var previousReleaseVersionIds = await contentDbContext
            .ReleaseVersions
            .Where(releaseVersion =>
                releaseVersionIds.Contains(releaseVersion.Id)
                && releaseVersion.PreviousVersionId != null)
            .Select(releaseVersion => releaseVersion.PreviousVersionId)
            .Cast<Guid>()
            .ToListAsync();

        var currentReleaseFiles = await GetReleaseFilesForReleaseVersions(releaseVersionIds);
        var previousReleaseFiles = await GetReleaseFilesForReleaseVersions(previousReleaseVersionIds);
        var previousToCurrentReleaseFileIds = previousReleaseFiles.ToDictionary(
            previousReleaseFile => previousReleaseFile.Id,
            previousReleaseFile => currentReleaseFiles.SingleOrDefault(currentReleaseFile => currentReleaseFile.FileId == previousReleaseFile.FileId)?.Id);
        
        var previousReleaseFileIdsToUpdate = previousToCurrentReleaseFileIds
            .Where(mapping => mapping.Value != null)
            .Select(mapping => mapping.Key);

        var previousDataSetVersions = await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => previousReleaseFileIdsToUpdate.Contains(dataSetVersion.ReleaseFileId))
            .ToListAsync();
        
        previousDataSetVersions.ForEach(previousDataSetVersion => 
            previousDataSetVersion.ReleaseFileId =
                previousToCurrentReleaseFileIds[previousDataSetVersion.ReleaseFileId]!.Value);

        await publicDataDbContext.SaveChangesAsync();
    }

    private async Task<List<ReleaseFile>> GetReleaseFilesForReleaseVersions(IEnumerable<Guid> releaseVersionIds) =>
        await contentDbContext
            .ReleaseFiles
            .Where(rf => releaseVersionIds.Contains(rf.ReleaseVersionId) && rf.File.Type == FileType.Data)
            .ToListAsync();
}
