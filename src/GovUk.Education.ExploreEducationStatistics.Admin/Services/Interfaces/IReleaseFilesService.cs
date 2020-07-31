using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{

    public interface IReleaseFilesService
    {
        Task<Common.Model.Either<ActionResult, bool>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, string userName);
        
        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFilesAsync(Guid releaseId, params ReleaseFileTypes[] types);
        
        Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId, IEnumerable<Guid> referencedReleaseVersions);

        Task<Either<ActionResult, FileInfo>> UploadFileAsync(Guid releaseId, IFormFile file,
            string name, ReleaseFileTypes type, bool overwrite);

        Task<Either<ActionResult, FileInfo>> AddChartFileAsync(Guid releaseId, IFormFile file);
        
        Task<Either<ActionResult, FileInfo>> UpdateChartFileAsync(Guid releaseId, IFormFile file, Guid id);

        Task<Either<ActionResult, bool>> DeleteNonDataFileAsync(Guid releaseId, ReleaseFileTypes type,
            string fileName);

        Task<Either<ActionResult, bool>> DeleteDataFilesAsync(Guid releaseId, string dataFileName);

        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            string fileName);
        
        Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, ReleaseFileTypes type,
            Guid id);
        
        Task<Either<ActionResult, bool>> DeleteNonDataFilesAsync(Guid releaseId, ReleaseFileTypes type,
            IEnumerable<string> chartFileNames);
    }
}