using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionService(
    IUserService userService,
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext) 
    : IDataSetVersionService
{
    public async Task<Either<ActionResult, List<DataSetVersionStatusViewModel>>> ListStatusesForReleaseVersion(
        Guid releaseVersionId,
        bool includePreviousReleaseVersion = false)
    {
        return await contentDbContext
            .ReleaseVersions
            .SingleOrNotFound(releaseVersion => releaseVersion.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var currentReleaseFiles = await GetReleaseFilesForReleaseVersions(releaseVersionId);
                var previousReleaseFilesStillIncluded = includePreviousReleaseVersion 
                    ? await GetPreviousReleaseFilesStillOnCurrentReleaseVersion(releaseVersion, currentReleaseFiles)
                    : [];

                var releaseFileIds = currentReleaseFiles
                    .Concat(previousReleaseFilesStillIncluded)
                    .Select(releaseFile => releaseFile.Id);

                return await publicDataDbContext
                    .DataSetVersions
                    .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.ReleaseFileId))
                    .Include(dataSetVersion => dataSetVersion.DataSet)
                    .Select(dataSetVersion => new DataSetVersionStatusViewModel(
                        dataSetVersion.Id,
                        dataSetVersion.DataSet.Title,
                        dataSetVersion.Status)
                    )
                    .ToListAsync();
            });
    }

    private async Task<List<ReleaseFile>> GetPreviousReleaseFilesStillOnCurrentReleaseVersion(
        ReleaseVersion releaseVersion,
        List<ReleaseFile> currentReleaseFiles)
    {
        if (releaseVersion.PreviousVersionId == null)
        {
            return [];
        }
        
        var previousReleaseFiles = 
            await GetReleaseFilesForReleaseVersions(releaseVersion.PreviousVersionId.Value);

        var currentReleaseVersionCsvFileIds = currentReleaseFiles
            .Select(releaseFile => releaseFile.FileId)
            .ToList();
                
        return previousReleaseFiles
            .Where(previousReleaseFile => currentReleaseVersionCsvFileIds.Contains(previousReleaseFile.FileId))
            .ToList();
    }

    private async Task<List<ReleaseFile>> GetReleaseFilesForReleaseVersions(Guid releaseVersionId)
    {
        return await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .ToListAsync();
    }
}

public record DataSetVersionStatusViewModel(Guid Id, string Title, DataSetVersionStatus Status);
