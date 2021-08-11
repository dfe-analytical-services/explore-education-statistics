using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, ReleaseViewModel>> CreateRelease(ReleaseCreateViewModel release);

        Task<Either<ActionResult, Unit>> DeleteRelease(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendment(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid id);

        Task<Either<ActionResult, List<ReleaseStatusViewModel>>> GetReleaseStatuses(Guid releaseId);

        Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatus(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(Guid releaseId, ReleaseUpdateViewModel request);

        Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseStatus(Guid releaseId, ReleaseStatusCreateViewModel request);

        Task<Either<ActionResult, TitleAndIdViewModel>> GetLatestPublishedRelease(Guid publicationId);

        Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyReleasesForReleaseStatusesAsync(
            params ReleaseApprovalStatus[] releaseApprovalStatues);

        Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyScheduledReleasesAsync();

        Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, Unit>> RemoveDataFiles(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, DataImportViewModel>> GetDataFileImportStatus(Guid releaseId, Guid fileId);
    }
}
