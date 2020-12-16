using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileUploadsValidatorService
    {
        Task<Either<ActionResult, Unit>> ValidateFileForUpload(IFormFile file, ReleaseFileTypes type);

        Task<Either<ActionResult, Unit>> ValidateDataFilesForUpload(Guid releaseId, IFormFile dataFile,
            IFormFile metaFile);

        Task<Either<ActionResult, Unit>> ValidateDataArchiveEntriesForUpload(Guid releaseId, IDataArchiveFile archiveFile);

        Task<Either<ActionResult, Unit>> ValidateSubjectName(Guid releaseId, string name);
    }
}