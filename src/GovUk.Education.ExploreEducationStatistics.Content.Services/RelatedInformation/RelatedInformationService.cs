using CSharpFunctionalExtensions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Errors;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;

public class RelatedInformationService(ContentDbContext contentDbContext) : IRelatedInformationService
{
    public Task<Result<RelatedInformationDto[], ResourceNotFoundError>> GetRelatedInformationForRelease(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        GetPublicationBySlug(publicationSlug, cancellationToken)
            .Bind(publication =>
                GetLatestPublishedReleaseVersionByReleaseSlug(publication, releaseSlug, cancellationToken)
            )
            .Map(releaseVersion => releaseVersion.RelatedInformation.Select(RelatedInformationDto.FromLink).ToArray());

    private Task<Result<Publication, ResourceNotFoundError>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .TryFirstAsync(cancellationToken)
            .ToResult<Publication, ResourceNotFoundError>(new PublicationNotFoundError(publicationSlug));

    private Task<Result<ReleaseVersion, ResourceNotFoundError>> GetLatestPublishedReleaseVersionByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .TryFirstAsync(cancellationToken)
            .ToResult<ReleaseVersion, ResourceNotFoundError>(new ReleaseNotFoundError(publication.Slug, releaseSlug));
}
