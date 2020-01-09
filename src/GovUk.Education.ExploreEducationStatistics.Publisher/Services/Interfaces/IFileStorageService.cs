using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task CopyReleaseToPublicContainer(PublishReleaseFilesMessage message);

        Task DeleteAllContentAsync();

        IEnumerable<FileInfo> ListPublicFiles(string publication, string release);

        Task MoveStagedContentAsync(ReleaseStatus releaseStatus);

        Task UploadFromStreamAsync(string blobName, string contentType, string content);
    }
}