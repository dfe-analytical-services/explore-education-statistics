using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseFileService
    {
        Task<Either<ActionResult, Unit>> Delete(
            Guid releaseId,
            Guid id,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> Delete(
            Guid releaseId,
            IEnumerable<Guid> ids,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListAll(Guid releaseId, params ReleaseFileTypes[] types);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId,
            IEnumerable<Guid> referencedReleaseVersions);

        Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid id);

        Task<Either<ActionResult, FileInfo>> UploadAncillary(Guid releaseId,
            IFormFile file,
            string name);

        Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseId,
            IFormFile file,
            Guid? replacingId = null);
    }
}