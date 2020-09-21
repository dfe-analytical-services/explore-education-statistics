using System;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<Either<ActionResult, Unit>> ValidateFileForUpload(Guid releaseId, IFormFile file, ReleaseFileTypes type,
            bool overwrite);

        Task<Either<ActionResult, Unit>> ValidateFileUploadName(string name);

        Task<Either<ActionResult, Unit>> ValidateDataFilesForUpload(Guid releaseId, IFormFile dataFile,
            IFormFile metaFile);

        Task<Either<ActionResult, Unit>> ValidateDataArchiveEntriesForUpload(Guid releaseId, ZipArchiveEntry dataFile,
            ZipArchiveEntry metaFile);

        Task<Either<ActionResult, Unit>> ValidateSubjectName(Guid releaseId, string name);

        Task<Either<ActionResult, Unit>> ValidateUploadFileType(IFormFile file, ReleaseFileTypes type);
    }
}