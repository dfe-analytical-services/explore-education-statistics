#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetUploadRepository(ContentDbContext contentDbContext) : IDataSetUploadRepository
{
    public async Task<Either<ActionResult, List<DataSetUpload>>> ListAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.DataSetUploads
            .Where(uploads => uploads.ReleaseVersionId == releaseVersionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, DataSetUploadViewModel?>> GetForDataFile(
        Guid dataFileId,
        CancellationToken cancellationToken = default)
    {
        var upload = await contentDbContext.DataSetUploads.FirstOrDefaultAsync(
            uploads => uploads.DataFileId == dataFileId,
            cancellationToken);

        return upload is null
            ? new NotFoundResult()
            : new DataSetUploadViewModel
            {
                Id = upload.Id,
                DataSetTitle = upload.DataSetTitle,
                DataFileName = upload.DataFileName,
                MetaFileName = upload.MetaFileName,
                Status = upload.Status,
                ScreenerResult = upload.ScreenerResult,
            };
    }
}
