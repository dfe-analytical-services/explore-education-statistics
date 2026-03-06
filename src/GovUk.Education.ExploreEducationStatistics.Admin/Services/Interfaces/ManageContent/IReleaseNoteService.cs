#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IReleaseNoteService
{
    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> CreateReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteCreateRequest createRequest,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteUpdateRequest updateRequest,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    );
}
