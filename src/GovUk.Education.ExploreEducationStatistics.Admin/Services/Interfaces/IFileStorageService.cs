using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task UploadFilesAsync(string publication, string release, IFormFile file, IFormFile metaFile, string name);
        IEnumerable<FileInfo> ListFiles(string publication, string release);
    }
}