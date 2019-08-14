using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        bool FileExistsAndIsReleased(string publication, string release, ReleaseFileTypes type, string filename);

        Task<FileStreamResult> StreamFile(string publication, string release, ReleaseFileTypes type, string filename);
    }
}