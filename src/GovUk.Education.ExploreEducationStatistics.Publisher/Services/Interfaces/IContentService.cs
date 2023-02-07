#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseIds);

        Task DeletePreviousVersionsContent(params Guid[] releaseIds);

        Task UpdateContent(params Guid[] releaseIds);

        Task UpdateContentStaged(DateTime expectedPublishDate, params Guid[] releaseIds);

        Task UpdateCachedTaxonomyBlobs();
    }
}
