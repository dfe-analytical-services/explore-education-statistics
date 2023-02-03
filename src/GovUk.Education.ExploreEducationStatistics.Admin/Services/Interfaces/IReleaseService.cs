#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, ReleaseViewModel>> CreateRelease(ReleaseCreateRequest release);

        Task<Either<ActionResult, DeleteReleasePlan>> GetDeleteReleasePlan(Guid releaseId);

        Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendment(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid id);

        Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(Guid releaseId, ReleaseUpdateRequest request);

        Task<Either<ActionResult, Unit>> UpdateReleasePublished(Guid releaseId,
            ReleasePublishedUpdateRequest request);

        Task<Either<ActionResult, IdTitleViewModel>> GetLatestPublishedRelease(Guid publicationId);

        Task<Either<ActionResult, List<ReleaseViewModel>>> ListReleasesWithStatuses(
            params ReleaseApprovalStatus[] releaseApprovalStatues);

        Task<Either<ActionResult, List<ReleaseViewModel>>> ListScheduledReleases();

        Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, DataImportStatusViewModel>> GetDataFileImportStatus(Guid releaseId, Guid fileId);
    }
}
