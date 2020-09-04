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
        Task<Either<ActionResult, DataFileInfo>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, string userName);

        Task<Either<ActionResult, bool>>  UploadDataFilesAsZipAsync(Guid releaseId, IFormFile zipFile, string name, string userEmail);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFilesAsync(Guid releaseId, params ReleaseFileTypes[] types);

        Task<Either<ActionResult, IEnumerable<DataFileInfo>>> ListDataFilesAsync(Guid releaseId);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId, IEnumerable<Guid> referencedReleaseVersions);

        Task<Either<ActionResult, FileInfo>> UploadFileAsync(Guid releaseId, IFormFile file,
            string name, ReleaseFileTypes type, bool overwrite);

        Task<Either<ActionResult, FileInfo>> UploadChartFileAsync(Guid releaseId, IFormFile file, Guid? id = null);

        Task<Either<ActionResult, bool>> DeleteNonDataFileAsync(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, bool>> DeleteChartFileAsync(Guid releaseId, Guid id);

        Task<Either<ActionResult, bool>> DeleteChartFilesAsync(Guid releaseId, IEnumerable<Guid> fileIds);

        Task<Either<ActionResult, bool>> DeleteDataFilesAsync(Guid releaseId, string dataFileName);

        Task<Either<ActionResult, Unit>> DeleteAllFiles(Guid releaseId);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            Guid id);
    }
}