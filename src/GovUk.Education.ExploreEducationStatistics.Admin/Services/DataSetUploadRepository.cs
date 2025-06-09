#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataSetUploadRepository(
    ContentDbContext contentDbContext,
    IMapper mapper) : IDataSetUploadRepository
{
    public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> ListAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.DataSetUploads
            .Where(uploads => uploads.ReleaseVersionId == releaseVersionId)
            .Select(entity => mapper.Map<DataSetUploadViewModel>(entity))
            .ToListAsync(cancellationToken);
    }
}
