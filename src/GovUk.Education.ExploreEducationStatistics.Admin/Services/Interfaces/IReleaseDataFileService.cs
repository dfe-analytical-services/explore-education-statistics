#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseDataFileService
    {
        Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid id, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> Delete(Guid releaseId, IEnumerable<Guid> ids, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false);

        Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseId, Guid fileId);

        Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseId);

        Task<Either<ActionResult, List<DataFileInfo>>> ReorderDataFiles(
            Guid releaseId,
            List<Guid> fileIds);

        Task<Either<ActionResult, DataFileInfo>> Upload(Guid releaseId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            Guid? replacingFileId = null,
            string? subjectName = null);

        Task<Either<ActionResult, DataFileInfo>> UploadAsZip(Guid releaseId,
            IFormFile zipFormFile,
            Guid? replacingFileId = null,
            string? subjectName = null);
    }
}
