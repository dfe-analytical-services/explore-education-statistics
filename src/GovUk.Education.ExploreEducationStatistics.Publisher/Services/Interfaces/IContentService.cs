namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IContentService
{
    Task DeletePreviousVersionsDownloadFiles(IReadOnlyList<Guid> releaseVersionIds);

    Task DeletePreviousVersionsContent(IReadOnlyList<Guid> releaseVersionIds);

    Task UpdateContent(Guid releaseVersionId, DateTimeOffset expectedPublishDate);

    Task UpdateContentStaged(DateTimeOffset expectedPublishDate, params Guid[] releaseVersionIds);

    Task UpdateCachedTaxonomyBlobs();
}
