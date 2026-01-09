#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseVersionService
{
    Task<Either<ActionResult, DeleteReleasePlanViewModel>> GetDeleteReleaseVersionPlan(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> DeleteReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> DeleteTestReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, ReleaseVersionViewModel>> GetRelease(Guid releaseVersionId);

    Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(Guid releaseVersionId);

    Task<Either<ActionResult, ReleaseVersionViewModel>> UpdateReleaseVersion(
        Guid releaseVersionId,
        ReleaseVersionUpdateRequest request
    );

    Task<Either<ActionResult, Unit>> UpdatePublishedDisplayDate(
        Guid releaseVersionId,
        ReleaseVersionPublishedDisplayDateUpdateRequest request
    );

    Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId);

    Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListReleasesWithStatuses(
        params ReleaseApprovalStatus[] releaseApprovalStatues
    );

    Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListUsersReleasesForApproval();

    Task<Either<ActionResult, List<ReleaseVersionSummaryViewModel>>> ListScheduledReleases();

    Task<Either<ActionResult, DeleteDataFilePlanViewModel>> GetDeleteDataFilePlan(
        Guid releaseVersionId,
        Guid fileId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId);

    Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(Guid releaseVersionId, Guid fileId);
}
