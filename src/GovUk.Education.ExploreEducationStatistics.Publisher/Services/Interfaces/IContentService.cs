namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IContentService
{
    Task DeletePreviousVersionsDownloadFiles(IReadOnlyList<Guid> releaseVersionIds);

    Task DeletePreviousVersionsContent(IReadOnlyList<Guid> releaseVersionIds);

    Task UpdateCachedTaxonomyBlobs();
}
