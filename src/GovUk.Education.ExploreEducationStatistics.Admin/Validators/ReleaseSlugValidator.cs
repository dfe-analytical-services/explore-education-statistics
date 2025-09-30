#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public class ReleaseSlugValidator(ContentDbContext context) : IReleaseSlugValidator
{
    public async Task<Either<ActionResult, Unit>> ValidateNewSlug(
        string newReleaseSlug,
        Guid publicationId,
        Guid? releaseId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ValidateReleaseSlugUniqueToPublication(
                slug: newReleaseSlug,
                publicationId: publicationId,
                releaseId: releaseId,
                cancellationToken: cancellationToken
            )
            .OnSuccess(async _ =>
                await ValidateReleaseRedirectDoesNotExistForNewSlug(
                    slug: newReleaseSlug,
                    publicationId: publicationId,
                    cancellationToken: cancellationToken
                )
            );
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseSlugUniqueToPublication(
        string slug,
        Guid publicationId,
        CancellationToken cancellationToken,
        Guid? releaseId = null
    )
    {
        var slugAlreadyExists = await context
            .Releases.Where(r => r.PublicationId == publicationId)
            .AnyAsync(
                r => r.Slug == slug && r.Id != releaseId,
                cancellationToken: cancellationToken
            );

        return slugAlreadyExists ? ValidationActionResult(SlugNotUnique) : Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseRedirectDoesNotExistForNewSlug(
        string slug,
        Guid publicationId,
        CancellationToken cancellationToken
    )
    {
        return await context
            .ReleaseRedirects.Where(rr => rr.Release.PublicationId == publicationId)
            .AnyAsync(rr => rr.Slug == slug, cancellationToken: cancellationToken)
            ? ValidationActionResult(ReleaseSlugUsedByRedirect)
            : Unit.Instance;
    }
}
