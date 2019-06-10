using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        bool FileExistsAndIsReleased(string publication, string release, string filename);

        Task<FileStreamResult> StreamFile(string publication, string release, string filename);
    }
}