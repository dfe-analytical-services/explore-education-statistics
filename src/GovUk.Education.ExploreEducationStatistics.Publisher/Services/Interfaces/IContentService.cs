using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IContentService
{
    Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseVersionIds);

    Task DeletePreviousVersionsContent(params Guid[] releaseVersionIds);

    Task UpdateContent(params Guid[] releaseVersionIds);

    Task UpdateContentStaged(DateTime expectedPublishDate,
        params Guid[] releaseVersionIds);

    Task UpdateCachedTaxonomyBlobs();
}
