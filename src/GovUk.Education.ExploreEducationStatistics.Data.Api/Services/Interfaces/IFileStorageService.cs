using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        bool FileExists(string publication, string release, string filename);

        IEnumerable<string> ListFiles(string publication, string release);
        
        Task<FileStreamResult> StreamFile(string publication, string release, string filename);
    }
}