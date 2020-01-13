using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseToPublicContainer(CopyReleaseCommand copyReleaseCommand);

        Task DeleteAllContentAsyncExcludingStaging();

        IEnumerable<FileInfo> ListPublicFiles(string publication, string release);

        Task MoveStagedContentAsync(ReleaseStatus releaseStatus);

        Task UploadFromStreamAsync(string blobName, string contentType, string content);
    }
}