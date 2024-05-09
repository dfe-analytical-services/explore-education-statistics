using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
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
        Guid releaseVersionId)
    {
        return await contentDbContext
            .ReleaseVersions
            .SingleOrNotFound(releaseVersion => releaseVersion.Id == releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
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
                    .Select(dataSetVersion => new DataSetVersionStatusViewModel(
                        dataSetVersion.Id,
                        dataSetVersion.DataSet.Title,
                        dataSetVersion.Status)
                    )
                    .ToListAsync();
            });
    }
}

public record DataSetVersionStatusViewModel(Guid Id, string Title, DataSetVersionStatus Status);
