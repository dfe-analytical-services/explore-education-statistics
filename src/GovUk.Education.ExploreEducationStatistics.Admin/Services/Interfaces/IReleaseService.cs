#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, ReleaseViewModel>> CreateRelease(ReleaseCreateRequest release);

        Task<Either<ActionResult, DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseVersionId);

        Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseVersionId);

        Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid releaseVersionId);

        Task<Either<ActionResult, ReleasePublicationStatusViewModel>>
            GetReleasePublicationStatus(Guid releaseVersionId);

        Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(Guid releaseVersionId, ReleaseUpdateRequest request);

        Task<Either<ActionResult, Unit>> UpdateReleasePublished(Guid releaseVersionId,
            ReleasePublishedUpdateRequest request);

        Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId);

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListReleasesWithStatuses(
            params ReleaseApprovalStatus[] releaseApprovalStatues);

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListUsersReleasesForApproval();

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> ListScheduledReleases();

        Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseVersionId, Guid fileId);

        Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseVersionId, Guid fileId);

        Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(Guid releaseVersionId,
            Guid fileId);
    }
}
