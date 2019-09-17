using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<Common.Model.Either<ValidationResult,IEnumerable<FileInfo>>> UploadDataFilesAsync(ReleaseId releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, bool overwrite);
        Task<IEnumerable<FileInfo>> ListFilesAsync(ReleaseId releaseId, ReleaseFileTypes type);

        Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadFilesAsync(ReleaseId releaseId,
            IFormFile dataFile, string name, ReleaseFileTypes type, bool overwrite);
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteFileAsync(ReleaseId releaseId, ReleaseFileTypes type, string fileName);
        
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteDataFileAsync(ReleaseId releaseId, string fileName);

        Task<Either<ValidationResult, FileStreamResult>> StreamFile(ReleaseId releaseId, ReleaseFileTypes type, string fileName);
    }
}