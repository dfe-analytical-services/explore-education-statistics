using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public interface IFileStorageService
    {
        Task<Either<ValidationResult, Either<ValidationResult, bool>>> UploadDataFilesAsync(ReleaseId releaseId,
            IFormFile dataFile, IFormFile metaFile, string name, bool overwrite);
    }
}