using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release);

        Task<Either<ActionResult, bool>> DeleteReleaseAsync(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> CreateReleaseAmendmentAsync(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid id);

        Task<Either<ActionResult, ReleasePublicationStatusViewModel>> GetReleasePublicationStatusAsync(Guid releaseId);

        Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(Guid releaseId, UpdateReleaseViewModel request);

        Task<Either<ActionResult, TitleAndIdViewModel>> GetLatestReleaseAsync(Guid publicationId);

        Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyReleasesForReleaseStatusesAsync(params ReleaseStatus[] releaseStatuses);

        Task<Either<ActionResult, List<MyReleaseViewModel>>> GetMyScheduledReleasesAsync();

        Task<Either<ActionResult, DeleteDataFilePlan>> GetDeleteDataFilePlan(Guid releaseId, string dataFileName, string subjectTitle);

        Task<Either<ActionResult, bool>>  RemoveDataFilesAsync(Guid releaseId, string fileName, string subjectTitle);

        Task<Either<ActionResult, ImportStatus>> GetDataFileImportStatus(Guid releaseId, string dataFileName);

        IEnumerable<Guid> GetReferencedReleaseFileVersions(Guid releaseId, params ReleaseFileTypes[] types);
    }
}
