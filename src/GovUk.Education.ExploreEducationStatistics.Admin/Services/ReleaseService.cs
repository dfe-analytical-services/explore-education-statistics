#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public partial class ReleaseService(
    ContentDbContext context,
    IUserService userService,
    IReleaseVersionService releaseVersionService,
    IReleaseCacheService releaseCacheService,
    IPublicationCacheService publicationCacheService,
    IGuidGenerator guidGenerator) : IReleaseService
{
    public async Task<Either<ActionResult, ReleaseVersionViewModel>> CreateRelease(ReleaseCreateRequest releaseCreate)
    {
        return await ReleaseCreateRequestValidator.Validate(releaseCreate)
            .OnSuccess(async () => await context.Publications
                .SingleOrNotFoundAsync(p => p.Id == releaseCreate.PublicationId))
            .OnSuccess(userService.CheckCanCreateReleaseForPublication)
            .OnSuccessDo(async _ =>
                await ValidateReleaseSlugUniqueToPublication(releaseCreate.Slug, releaseCreate.PublicationId))
            .OnSuccess(async publication =>
            {
                var release = new Release
                {
                    PublicationId = releaseCreate.PublicationId,
                    Year = releaseCreate.Year,
                    TimePeriodCoverage = releaseCreate.TimePeriodCoverage,
                    Slug = releaseCreate.Slug,
                    Label = FormatReleaseLabel(releaseCreate.Label),
                };

                var newReleaseVersion = new ReleaseVersion
                {
                    Id = guidGenerator.NewGuid(),
                    Release = release,
                    Type = releaseCreate.Type!.Value,
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    PublicationId = release.PublicationId,
                };

                if (releaseCreate.TemplateReleaseId.HasValue)
                {
                    await CreateGenericContentFromTemplate(releaseCreate.TemplateReleaseId.Value,
                        newReleaseVersion);
                }
                else
                {
                    newReleaseVersion.GenericContent = new List<ContentSection>();
                }

                newReleaseVersion.SummarySection = new ContentSection
                {
                    Type = ContentSectionType.ReleaseSummary,
                };
                newReleaseVersion.KeyStatisticsSecondarySection = new ContentSection
                {
                    Type = ContentSectionType.KeyStatisticsSecondary,
                };
                newReleaseVersion.HeadlinesSection = new ContentSection
                {
                    Type = ContentSectionType.Headlines,
                };
                newReleaseVersion.RelatedDashboardsSection = new ContentSection
                {
                    Type = ContentSectionType.RelatedDashboards,
                };
                newReleaseVersion.Created = DateTime.UtcNow;
                newReleaseVersion.CreatedById = userService.GetUserId();

                await context.ReleaseVersions.AddAsync(newReleaseVersion);

                publication.ReleaseSeries.Insert(0, new ReleaseSeriesItem
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = release.Id
                });
                context.Publications.Update(publication);

                await context.SaveChangesAsync();
                return await releaseVersionService.GetRelease(newReleaseVersion.Id);
            });
    }

    public async Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
        Guid releaseId, 
        ReleaseUpdateRequest releaseUpdate,
        CancellationToken cancellationToken = default)
    {
        return await GetRelease(releaseId)
            .OnSuccess(userService.CheckCanUpdateRelease)
            .OnSuccess(async release => await ValidateUpdateRequest(
                release: release,
                releaseUpdate: releaseUpdate, 
                cancellationToken: cancellationToken))
            .OnSuccessDo(async releaseAndSlugs => await UpdateRelease(
                release: releaseAndSlugs.release,
                newReleaseSlug: releaseAndSlugs.newReleaseSlug,
                releaseUpdate: releaseUpdate,
                cancellationToken: cancellationToken))
            .OnSuccess(async releaseAndSlugs =>
            {
                var latestPublishedReleaseVersion = await GetLatestPublishedReleaseVersion(
                    publicationId: releaseAndSlugs.release.PublicationId,
                    releaseSlug: releaseAndSlugs.newReleaseSlug,
                    cancellationToken: cancellationToken);

                var releaseIsLive = latestPublishedReleaseVersion is not null;

                if (releaseIsLive)
                {
                    await UpdateReleaseCache(
                        oldReleaseSlug: releaseAndSlugs.oldReleaseSlug,
                        newReleaseSlug: releaseAndSlugs.newReleaseSlug,
                        publicationSlug: releaseAndSlugs.release.Publication.Slug,
                        latestReleaseVersionId: latestPublishedReleaseVersion!.Id);
                }

                return (releaseAndSlugs.oldReleaseSlug, releaseIsLive);
            })
            .OnSuccessDo(async releaseData =>
            {
                if (releaseData.releaseIsLive)
                {
                    await CreateReleaseRedirect(
                        releaseId: releaseId,
                        oldReleaseSlug: releaseData.oldReleaseSlug,
                        cancellationToken: cancellationToken);
                }
            })
            .OnSuccess(async () => await GetRelease(releaseId))
            .OnSuccess(MapRelease);
    }

    private async Task CreateGenericContentFromTemplate(
        Guid templateReleaseVersionId,
        ReleaseVersion newReleaseVersion)
    {
        var templateReleaseVersion = await context
            .ReleaseVersions
            .AsNoTracking()
            .Include(releaseVersion => releaseVersion.Content)
            .FirstAsync(rv => rv.Id == templateReleaseVersionId);

        newReleaseVersion.Content = templateReleaseVersion
            .Content
            .Where(section => section.Type == ContentSectionType.Generic)
            .Select(section => CloneContentSectionFromReleaseTemplate(section, newReleaseVersion))
            .ToList();
    }

    private static ContentSection CloneContentSectionFromReleaseTemplate(
        ContentSection originalSection,
        ReleaseVersion newReleaseVersion)
    {
        // Create a new ContentSection based upon the original template.
        return new ContentSection
        {
            // Assign a new Id.
            Id = Guid.NewGuid(),

            // Assign it to the new release version.
            ReleaseVersionId = newReleaseVersion.Id,

            // Copy certain fields from the original.
            Caption = originalSection.Caption,
            Heading = originalSection.Heading,
            Order = originalSection.Order,
            Type = originalSection.Type
        };
    }

    private async Task<Either<ActionResult, (Release release, string oldReleaseSlug, string newReleaseSlug)>> ValidateUpdateRequest(
        Release release, 
        ReleaseUpdateRequest releaseUpdate,
        CancellationToken cancellationToken)
    {
        var newReleaseSlug = CalculateNewReleaseSlug(release, releaseUpdate);
        var oldReleaseSlug = release.Slug;

        return await ValidateReleaseSlugUniqueToPublication(
            slug: newReleaseSlug,
            publicationId: release.PublicationId,
            releaseId: release.Id,
            cancellationToken: cancellationToken)
            .OnSuccess(async _ => await ValidateReleaseIsNotUndergoingPublishing(release.Id, cancellationToken ))
            .OnSuccess(async _ => await ValidateReleaseRedirectDoesNotExistForNewSlug(
                releaseId: release.Id,
                newReleaseSlug: newReleaseSlug, 
                cancellationToken: cancellationToken))
            .OnSuccess(_ => (release, oldReleaseSlug, newReleaseSlug));
    }

    private static string CalculateNewReleaseSlug(Release release, ReleaseUpdateRequest releaseUpdate)
    {
        return NamingUtils.CreateReleaseSlug(
            year: release.Year,
            timePeriodCoverage: release.TimePeriodCoverage,
            label: releaseUpdate.Label);
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseSlugUniqueToPublication(
        string slug,
        Guid publicationId,
        Guid? releaseId = null,
        CancellationToken cancellationToken = default)
    {
        var slugAlreadyExists = await context.Releases
            .Where(r => r.PublicationId == publicationId)
            .AnyAsync(r => r.Slug == slug && r.Id != releaseId, cancellationToken: cancellationToken);

        return slugAlreadyExists
            ? ValidationActionResult(SlugNotUnique)
            : Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseIsNotUndergoingPublishing(Guid releaseId, CancellationToken cancellationToken)
    {
        var releaseVersions = await context.ReleaseVersions
            .Where(rv => rv.ReleaseId == releaseId)
            .Select(rv => new
            {
                rv.ApprovalStatus,
                rv.Published
            })
            .ToListAsync(cancellationToken: cancellationToken);

        var existsUnpublishedApprovedReleaseVersions = releaseVersions
            .Any(rv => rv.ApprovalStatus == ReleaseApprovalStatus.Approved && rv.Published is null);

        return existsUnpublishedApprovedReleaseVersions
            ? ValidationActionResult(ReleaseUndergoingPublishing)
            : Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseRedirectDoesNotExistForNewSlug(Guid releaseId, string newReleaseSlug, CancellationToken cancellationToken)
    {
        return await context.ReleaseRedirects
            .Where(rr => rr.ReleaseId == releaseId)
            .AnyAsync(rr => rr.Slug == newReleaseSlug, cancellationToken: cancellationToken)
            ? ValidationActionResult(ReleaseSlugUsedByRedirect)
            : Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> UpdateRelease(
        Release release, 
        string newReleaseSlug, 
        ReleaseUpdateRequest releaseUpdate,
        CancellationToken cancellationToken)
    {
        var newLabel = FormatReleaseLabel(releaseUpdate.Label);

        release.Label = newLabel;
        release.Slug = newReleaseSlug;

        await context.SaveChangesAsync(cancellationToken: cancellationToken);

        return Unit.Instance;
    }

    private static string? FormatReleaseLabel(string? releaseLabel)
    {
        var trimmedNewLabel = string.IsNullOrWhiteSpace(releaseLabel) 
            ? null 
            : releaseLabel.Trim();

        return trimmedNewLabel.HasValue()
            ? MatchWhitespaceRegex().Replace(trimmedNewLabel, " ")
            : null;
    }

    private async Task<ReleaseVersion?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default)
    {
        return await context.ReleaseVersions
            .LatestReleaseVersions(
                publicationId: publicationId,
                releaseSlug: releaseSlug, 
                publishedOnly: true)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> UpdateReleaseCache(
        string oldReleaseSlug,
        string newReleaseSlug,
        string publicationSlug,
        Guid latestReleaseVersionId)
    {
        var slugHasChanged = oldReleaseSlug != newReleaseSlug;

        if (slugHasChanged)
        {
            // Remove release-specific path cache as the release slug has changed - hence,
            // the path should also change
            await releaseCacheService.RemoveRelease(
                publicationSlug: publicationSlug,
                releaseSlug: oldReleaseSlug);
        }

        // Update release-specific path cache
        await releaseCacheService.UpdateRelease(
            releaseVersionId: latestReleaseVersionId,
            publicationSlug: publicationSlug,
            releaseSlug: newReleaseSlug);

        // Update latest release path cache
        await releaseCacheService.UpdateRelease(
            releaseVersionId: latestReleaseVersionId,
            publicationSlug: publicationSlug);

        // Update publication cache (view-model contains release related data that has now been updated)
        await publicationCacheService.UpdatePublication(publicationSlug);

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CreateReleaseRedirect(
        Guid releaseId, 
        string oldReleaseSlug,
        CancellationToken cancellationToken)
    {
        var newReleaseRedirect = new ReleaseRedirect
        {
            ReleaseId = releaseId,
            Slug = oldReleaseSlug
        };

        await context.ReleaseRedirects.AddAsync(newReleaseRedirect, cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken: cancellationToken);

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Release>> GetRelease(Guid releaseId)
    {
        return await context.Releases
            .Include(r => r.Publication)
            .SingleOrNotFoundAsync(p => p.Id == releaseId);
    }

    private static ReleaseViewModel MapRelease(Release release)
    {
        return new ReleaseViewModel
        {
            Id = release.Id,
            PublicationId = release.PublicationId,
            Slug = release.Slug,
            TimePeriodCoverage = release.TimePeriodCoverage,
            Year = release.Year,
            Label = release.Label,
            Title = release.Title,
        };
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex MatchWhitespaceRegex();
}
