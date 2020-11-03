using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseIds);

        Task DeletePreviousVersionsContent(params Guid[] releaseIds);

        Task UpdateAllContentAsync();

        Task UpdateContent(PublishContext context, params Guid[] releaseIds);

        Task UpdateMethodology(PublishContext context, Guid methodologyId);

        Task UpdatePublication(PublishContext context, Guid publicationId);

        Task UpdateTaxonomy(PublishContext context);
    }
}