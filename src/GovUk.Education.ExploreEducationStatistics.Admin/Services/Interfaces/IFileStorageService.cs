using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{

    public interface IFileStorageService
    {
        Task<Common.Model.Either<ActionResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, string userName);

        Task<Either<ActionResult, IEnumerable<Common.Model.FileInfo>>> ListChartFilesAsync(Guid releaseId);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFilesAsync(Guid releaseId, params ReleaseFileTypes[] types);
        
        Task<Either<ActionResult, IEnumerable<Common.Model.FileInfo>>> ListPublicFilesPreview(Guid releaseId, IEnumerable<Guid> referencedReleaseVersions);

        Task<Either<ActionResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId, IFormFile file,
            string name, ReleaseFileTypes type, bool overwrite);

        Task<Either<ActionResult, Common.Model.FileInfo>> UploadChartFileAsync(Guid releaseId, IFormFile file);

        Task<Either<ActionResult, bool>> DeleteNonDataFileAsync(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, bool>> RemoveDataFileReleaseLinkAsync(Guid releaseId, string dataFileName);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, bool>> DeleteNonDataFilesAsync(Guid releaseId, ReleaseFileTypes type,
            IEnumerable<string> chartFileNames);
    }
}