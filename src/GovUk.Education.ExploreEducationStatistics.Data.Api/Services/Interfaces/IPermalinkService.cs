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
    // TODO EES-3755 Remove after Permalink snapshot migration work is complete
    Task<Either<ActionResult, LegacyPermalinkViewModel>> GetLegacy(Guid permalinkId,
        CancellationToken cancellationToken = default);

    // TODO EES-3755 Remove after Permalink snapshot migration work is complete
    Task<Either<ActionResult, Unit>> LegacyDownloadCsvToStream(
        Guid permalinkId,
        Stream stream,
        CancellationToken cancellationToken = default);

    // TODO EES-3755 Remove after Permalink snapshot migration work is complete
    Task<Either<ActionResult, LegacyPermalinkViewModel>> CreateLegacy(PermalinkCreateRequest request);

    // TODO EES-3755 Remove after Permalink snapshot migration work is complete
    Task<Either<ActionResult, LegacyPermalinkViewModel>> CreateLegacy(Guid releaseId,
        PermalinkCreateRequest request);

    // TODO EES-3755 Remove after Permalink snapshot migration work is complete
    Task<Either<ActionResult, Unit>> MigratePermalink(Guid permalinkId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PermalinkViewModel>> CreatePermalink(PermalinkCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PermalinkViewModel>> CreatePermalink(Guid releaseId,
        PermalinkCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, PermalinkViewModel>> GetPermalink(Guid permalinkId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DownloadCsvToStream(Guid permalinkId,
        Stream stream,
        CancellationToken cancellationToken = default);
}
