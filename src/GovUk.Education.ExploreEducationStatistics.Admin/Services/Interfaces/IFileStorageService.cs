using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, bool overwrite, string userName);
        
        Task<IEnumerable<FileInfo>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type);

        Task<IEnumerable<FileInfo>> ListPublicFilesPreviewAsync(Guid releaseId);
        
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile dataFile, string name, ReleaseFileTypes type, bool overwrite);
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteFileAsync(Guid releaseId, ReleaseFileTypes type, string fileName);
        
        Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteDataFileAsync(Guid releaseId, string fileName);

        Task<Either<ValidationResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type, string fileName);
    }
}