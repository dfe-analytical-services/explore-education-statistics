using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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

        Task<Either<ActionResult, IEnumerable<AdminFileInfo>>> ListAll(Guid releaseId, params FileType[] types);

        Task<Either<ActionResult, FileInfo>> GetFile(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, FileStreamResult>> Stream(Guid releaseId, Guid id);

        Task<Either<ActionResult, Unit>> UpdateName(Guid releaseId, Guid fileId, string name);

        Task<Either<ActionResult, AdminFileInfo>> UploadAncillary(Guid releaseId,
            IFormFile formFile,
            string name);

        Task<Either<ActionResult, FileInfo>> UploadChart(Guid releaseId,
            IFormFile formFile,
            Guid? replacingId = null);
    }
}
