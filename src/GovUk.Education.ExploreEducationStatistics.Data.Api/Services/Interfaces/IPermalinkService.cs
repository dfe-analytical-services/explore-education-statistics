#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IPermalinkService
{
    Task<Either<ActionResult, PermalinkViewModel>> CreatePermalink(PermalinkCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PermalinkViewModel>> GetPermalink(Guid permalinkId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DownloadCsvToStream(Guid permalinkId,
        Stream stream,
        CancellationToken cancellationToken = default);
}
