#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetUploadRepository
{
    Task<Either<ActionResult, List<DataSetUploadViewModel>>> ListAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);
}
