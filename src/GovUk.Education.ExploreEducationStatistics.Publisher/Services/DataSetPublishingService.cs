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
        await UpdateDataSetVersionFileIdsForAmendments(releaseVersionIds);
    }

    /// <summary>
    /// Publishes any relevant data set versions, marking them as the latest live version
    /// of their data set. If the data set has not been published before, this will
    /// also be marked as published.
    /// </summary>
    private async Task PromoteDraftDataSetVersions(IEnumerable<Guid> releaseVersionIds)
    {
        var releaseFileIds = (await GetReleaseFiles(releaseVersionIds))
            .Select(releaseFile => releaseFile.Id);

        var dataSets = await publicDataDbContext
            .DataSets
            .Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.LatestDraftVersion)
            .Where(dataSet => dataSet.LatestDraftVersion != null
                              && releaseFileIds.Contains(dataSet.LatestDraftVersion.ReleaseFileId))
            .ToListAsync();

        foreach (var dataSet in dataSets)
        {
            var currentDraftVersion = dataSet.LatestDraftVersion;
            currentDraftVersion!.Status = DataSetVersionStatus.Published;
            currentDraftVersion.Published = DateTimeOffset.UtcNow;

            dataSet.Status = DataSetStatus.Published;
            dataSet.Published ??= currentDraftVersion.Published;

            dataSet.LatestLiveVersion = currentDraftVersion;
            dataSet.LatestLiveVersionId = currentDraftVersion.Id;

            dataSet.LatestDraftVersion = null;
            dataSet.LatestDraftVersionId = null;
        }

        await publicDataDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// For any amendment release, update any relevant data set versions being published
    /// with IDs that reference the amendment's files.
    /// </summary>
    private async Task UpdateDataSetVersionFileIdsForAmendments(IEnumerable<Guid> releaseVersionIds)
    {
        var previousReleaseVersionIds = await contentDbContext
            .ReleaseVersions
            .Where(releaseVersion => releaseVersionIds.Contains(releaseVersion.Id))
            .Where(releaseVersion => releaseVersion.PreviousVersionId != null)
            .Select(releaseVersion => releaseVersion.PreviousVersionId)
            .Distinct()
            .Cast<Guid>()
            .ToListAsync();

        if (previousReleaseVersionIds.Count == 0)
        {
            return;
        }

        var previousReleaseFilesById = (await GetReleaseFiles(previousReleaseVersionIds))
            .ToDictionary(rf => rf.Id);

        var dataSetVersions = await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => previousReleaseFilesById.Keys.Contains(dataSetVersion.ReleaseFileId))
            .ToListAsync();

        if (dataSetVersions.Count == 0)
        {
            return;
        }

        foreach (var dataSetVersion in dataSetVersions)
        {
            var previousReleaseFile = previousReleaseFilesById[dataSetVersion.ReleaseFileId];

            var nextReleaseFile = await contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseVersion)
                .Where(rf => rf.FileId == previousReleaseFile.FileId)
                .Where(rf => rf.ReleaseVersion.PreviousVersionId == previousReleaseFile.ReleaseVersionId)
                .SingleAsync();

            dataSetVersion.ReleaseFileId = nextReleaseFile.Id;
        }

        await publicDataDbContext.SaveChangesAsync();
    }

    private async Task<List<ReleaseFile>> GetReleaseFiles(IEnumerable<Guid> releaseVersionIds)
    {
        return await contentDbContext
            .ReleaseFiles
            .Where(rf => releaseVersionIds.Contains(rf.ReleaseVersionId) && rf.File.Type == FileType.Data)
            .ToListAsync();
    }
}
