using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<Either<ActionResult, bool>> ValidateFileForUpload(Guid releaseId, IFormFile file, ReleaseFileTypes type, bool overwrite);
        Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name);
        Task<Either<ActionResult, bool>> ValidateZippedDataFileForUpload(Guid releaseId, IFormFile zipFile, string name);
        Task<Either<ActionResult, bool>> ValidateUploadFileType(IFormFile file, ReleaseFileTypes type);
    }
}