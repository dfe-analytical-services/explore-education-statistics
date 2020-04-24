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
        Task<Either<ActionResult, bool>> ValidateFileForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, bool overwrite);
        Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name, bool overwrite);
        Task<Either<ActionResult, bool>> ValidateUploadFileType(IFormFile file, ReleaseFileTypes type);
    }
}