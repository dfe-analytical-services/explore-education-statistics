using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<Either<ActionResult, T>> GetDeserialized<T>(string path);

        Task<bool> IsBlobReleased(string containerName, string path);

        Task<FileStreamResult> StreamFile(string containerName, string path);
    }
}