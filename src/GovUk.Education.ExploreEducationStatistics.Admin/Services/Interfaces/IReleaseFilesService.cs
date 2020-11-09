using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseFilesService
    {
        Task<Either<ActionResult, DataFileInfo>> UploadDataFiles(Guid releaseId, IFormFile dataFile,
            IFormFile metaFile, string userName, Guid? replacingFileId = null, string subjectName = null);

        Task<Either<ActionResult, DataFileInfo>> UploadDataFilesAsZip(Guid releaseId, IFormFile zipFile,
            string userName, Guid? replacingFileId = null, string subjectName = null);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFiles(Guid releaseId,
            params ReleaseFileTypes[] types);

        Task<Either<ActionResult, IEnumerable<DataFileInfo>>> ListDataFiles(Guid releaseId);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId,
            IEnumerable<Guid> referencedReleaseVersions);

        Task<Either<ActionResult, DataFileInfo>> GetDataFile(Guid releaseId, Guid fileReferenceId);

        Task<Either<ActionResult, FileInfo>> UploadFile(Guid releaseId, IFormFile file, string name,
            ReleaseFileTypes type, bool overwrite);

        Task<Either<ActionResult, FileInfo>> UploadChartFile(Guid releaseId, IFormFile file, Guid? id = null);

        Task<Either<ActionResult, Unit>> DeleteNonDataFile(
            Guid releaseId,
            ReleaseFileTypes type,
            string fileName,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteChartFile(Guid releaseId, Guid id, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteChartFiles(
            Guid releaseId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteDataFiles(Guid releaseId, Guid fileId, bool forceDelete = false);

        Task<Either<ActionResult, Unit>> DeleteAllFiles(Guid releaseId, bool forceDelete = false);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, Guid id);
    }
}