#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IReleaseNoteService
{
    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteSaveRequest saveRequest
    );

    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteSaveRequest saveRequest
    );

    Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(Guid releaseVersionId, Guid releaseNoteId);
}
