using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task UploadFileAsync(string publication, string release, string filename, string path, string name);
        IEnumerable<FileInfo> ListFiles(string publication, string release);
    }
}