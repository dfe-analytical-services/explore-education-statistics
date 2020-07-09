using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseFilesToPublicContainer(CopyReleaseFilesCommand copyReleaseFilesCommand);

        Task DeleteAllContentAsyncExcludingStaging();

        IEnumerable<FileInfo> ListPublicFiles(string publication, string release);

        Task MoveStagedContentAsync();

        Task UploadAsJson(string blobName, object value, JsonSerializerSettings settings = null);
    }
}