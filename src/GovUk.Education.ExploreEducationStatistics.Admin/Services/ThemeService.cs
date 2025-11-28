#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ThemeService(
    IOptions<AppOptions> appOptions,
    ContentDbContext contentDbContext,
    IDataSetVersionRepository dataSetVersionRepository,
    IMapper mapper,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService,
    IMethodologyService methodologyService,
    IPublishingService publishingService,
    IReleaseVersionService releaseVersionService,
    IAdminEventRaiser eventRaiser,
    IPublicationCacheService publicationCacheService,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    ILogger<ThemeService> logger
) : IThemeService
{
    private readonly bool _themeDeletionAllowed = appOptions.Value.EnableThemeDeletion;

    public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(ThemeSaveViewModel created)
    {
        return await userService
            .CheckCanManageAllTaxonomy()
            .OnSuccess(async _ =>
            {
                if (await contentDbContext.Themes.AnyAsync(theme => theme.Slug == created.Slug))
                {
                    return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                }

                var saved = await contentDbContext.Themes.AddAsync(
                    new Theme
                    {
                        Slug = created.Slug,
                        Summary = created.Summary,
                        Title = created.Title,
                    }
                );

                await contentDbContext.SaveChangesAsync();

                await publishingService.TaxonomyChanged();

                return await GetTheme(saved.Entity.Id);
            });
    }

    public async Task<Either<ActionResult, ThemeViewModel>> UpdateTheme(Guid id, ThemeSaveViewModel updated)
    {
        return await persistenceHelper
            .CheckEntityExists<Theme>(id)
            .OnSuccessDo(userService.CheckCanManageAllTaxonomy)
            .OnSuccess(async theme =>
            {
                if (await contentDbContext.Themes.AnyAsync(t => t.Slug == updated.Slug && t.Id != id))
                {
                    return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                }

                theme.Title = updated.Title;
                theme.Slug = updated.Slug;
                theme.Summary = updated.Summary;

                await contentDbContext.SaveChangesAsync();

                await publishingService.TaxonomyChanged();

                await InvalidatePublicationsCacheByTheme(theme.Id);

                await eventRaiser.OnThemeUpdated(theme);

                return await GetTheme(theme.Id);
            });
    }

    public async Task InvalidatePublicationsCacheByTheme(Guid themeId)
    {
        var themePublicationsSlugs = await contentDbContext
            .Publications.Where(p => p.ThemeId == themeId)
            .Select(p => p.Slug)
            .ToListAsync();

        await themePublicationsSlugs.ToAsyncEnumerable().ForEachAwaitAsync(InvalidatePublicationCacheSafe);
    }

    private async Task InvalidatePublicationCacheSafe(string publicationSlug)
    {
        try
        {
            await publicationCacheService
                .UpdatePublication(publicationSlug)
                .OnFailureDo(result =>
                    logger.LogWarning(
                        "Failed to invalidate cache for Publication {PublicationSlug}. Reason: {Result}",
                        publicationSlug,
                        result
                    )
                );
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to invalidate cache for Publication {PublicationSlug}.", publicationSlug);
        }
    }

    public async Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id)
    {
        return await userService
            .CheckCanManageAllTaxonomy()
            .OnSuccess(() => persistenceHelper.CheckEntityExists<Theme>(id))
            .OnSuccess(mapper.Map<ThemeViewModel>);
    }

    public async Task<Either<ActionResult, List<ThemeViewModel>>> GetThemes()
    {
        return await userService
            .CheckCanAccessSystem()
            .OnSuccess(async _ =>
                await userService
                    .CheckCanManageAllTaxonomy()
                    .OnSuccess(async () => await contentDbContext.Themes.ToListAsync())
                    .OrElse(GetUserThemes)
            )
            .OnSuccess(list => list.Select(mapper.Map<ThemeViewModel>).OrderBy(theme => theme.Title).ToList());
    }

    public async Task<Either<ActionResult, Unit>> DeleteTheme(
        Guid themeId,
        CancellationToken cancellationToken = default
    )
    {
        return await userService
            .CheckCanManageAllTaxonomy()
            .OnSuccess(() => contentDbContext.Themes.FirstOrNotFoundAsync(t => t.Id == themeId, cancellationToken))
            .OnSuccessDo(CheckCanDeleteTheme)
            .OnSuccessDo(() => DeletePublicationsForTheme(themeId, cancellationToken))
            .OnSuccessVoid(async theme =>
            {
                contentDbContext.Themes.Remove(theme);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                await publishingService.TaxonomyChanged(cancellationToken);
            });
    }

    private async Task<Either<List<ActionResult>, Unit>> DeletePublicationsForTheme(
        Guid themeId,
        CancellationToken cancellationToken
    )
    {
        var publicationIds = await contentDbContext
            .Publications.Where(p => p.ThemeId == themeId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var deletePublicationResults = await publicationIds
            .ToAsyncEnumerable()
            .SelectAwait(async publicationId =>
                await DeleteMethodologiesForPublication(publicationId, cancellationToken)
                    .OnSuccess(() => DeletePublication(publicationId, cancellationToken))
            )
            .ToListAsync(cancellationToken);

        return deletePublicationResults.AggregateSuccessesAndFailures().OnSuccessVoid();
    }

    private async Task<Either<ActionResult, Unit>> DeleteMethodologiesForPublication(
        Guid publicationId,
        CancellationToken cancellationToken
    )
    {
        var methodologyIdsToDelete = await contentDbContext
            .PublicationMethodologies.Where(pm => pm.Owner && pm.PublicationId == publicationId)
            .Select(pm => pm.MethodologyId)
            .ToListAsync(cancellationToken);

        return await methodologyIdsToDelete
            .Select(methodologyId => methodologyService.DeleteMethodology(methodologyId, true))
            .OnSuccessAllReturnVoid();
    }

    private async Task<Either<ActionResult, Unit>> DeletePublication(
        Guid publicationId,
        CancellationToken cancellationToken
    )
    {
        var publication = await contentDbContext
            .Publications.Include(p => p.LatestPublishedReleaseVersion)
            .Include(p => p.Contact)
            .FirstAsync(p => p.Id == publicationId, cancellationToken);

        // Capture details of the latest published release before it is deleted
        // so that they can be used to raise an event after the publication is deleted.
        var latestPublicationRelease =
            publication.LatestPublishedReleaseVersion != null
                ? new LatestPublishedReleaseInfo
                {
                    LatestPublishedReleaseId = publication.LatestPublishedReleaseVersion!.ReleaseId,
                    LatestPublishedReleaseVersionId = publication.LatestPublishedReleaseVersion.Id,
                }
                : null;

        // Some Content Db Releases may be soft-deleted and therefore not visible.
        // Ignore the query filter to make sure they are found
        var releaseVersionsToDelete = await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .IgnoreQueryFilters()
            .Include(rv => rv.Release)
            .Where(rv => rv.Release.PublicationId == publicationId)
            .ToListAsync(cancellationToken);

        var releaseVersionsAndDataSetVersions = await releaseVersionsToDelete
            .ToAsyncEnumerable()
            .SelectAwait(async rv =>
            {
                var dataSetVersions = await dataSetVersionRepository.GetDataSetVersions(rv.Id);

                return new ReleaseVersionAndDataSetVersions(ReleaseVersion: rv, DataSetVersions: dataSetVersions);
            })
            .ToListAsync(cancellationToken);

        var releaseVersionIdsInDeleteOrder = releaseVersionsAndDataSetVersions
            .Order(new DependentReleaseVersionDeleteOrderComparator())
            .Select(rv => rv.ReleaseVersion.Id)
            .ToList();

        return await releaseVersionIdsInDeleteOrder
            .Select(releaseVersionId =>
                releaseVersionService.DeleteTestReleaseVersion(releaseVersionId, cancellationToken)
            )
            .OnSuccessAll()
            .OnSuccessVoid(async () =>
            {
                contentDbContext.Publications.Remove(publication);
                contentDbContext.Contacts.Remove(publication.Contact);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                await eventRaiser.OnPublicationDeleted(publication.Id, publication.Slug, latestPublicationRelease);
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteUITestThemes(CancellationToken cancellationToken = default)
    {
        return !_themeDeletionAllowed
            ? new ForbidResult()
            : await userService
                .CheckCanManageAllTaxonomy()
                .OnSuccess(async _ =>
                    (await contentDbContext.Themes.ToListAsync(cancellationToken)).Where(theme =>
                        theme.IsTestOrSeedTheme()
                    )
                )
                .OnSuccessVoid(async themes =>
                {
                    foreach (var theme in themes)
                    {
                        await DeleteTheme(theme.Id, cancellationToken);
                    }

                    await contentDbContext.SaveChangesAsync(cancellationToken);
                    await publishingService.TaxonomyChanged(cancellationToken);
                });
    }

    private async Task<Either<ActionResult, Unit>> CheckCanDeleteTheme(Theme theme)
    {
        if (!_themeDeletionAllowed)
        {
            return new ForbidResult();
        }

        // For now we only want to delete test themes as we
        // don't really have a mechanism to clean things up
        // properly across the entire application.
        // TODO: EES-1295 ability to completely delete releases
        if (!theme.IsTestOrSeedTheme())
        {
            return new ForbidResult();
        }

        return await Task.FromResult(Unit.Instance);
    }

    private async Task<List<Theme>> GetUserThemes()
    {
        var userId = userService.GetUserId();

        return await userReleaseRoleRepository
            .Query()
            .WhereForUser(userId)
            .WhereRolesNotIn(ReleaseRole.PrereleaseViewer)
            .Select(userReleaseRole => userReleaseRole.ReleaseVersion.Release.Publication.Theme)
            .Concat(
                userPublicationRoleRepository
                    .Query()
                    .WhereForUser(userId)
                    .WhereRolesIn([PublicationRole.Owner, PublicationRole.Allower])
                    .Select(userPublicationRole => userPublicationRole.Publication.Theme)
            )
            .Distinct()
            .ToListAsync();
    }
}

public record ReleaseVersionAndDataSetVersions(ReleaseVersion ReleaseVersion, List<DataSetVersion> DataSetVersions);

public class DependentReleaseVersionDeleteOrderComparator : IComparer<ReleaseVersionAndDataSetVersions>
{
    public int Compare(ReleaseVersionAndDataSetVersions? version1, ReleaseVersionAndDataSetVersions? version2)
    {
        if (version1 == null || version2 == null)
        {
            return Comparer<ReleaseVersionAndDataSetVersions>.Default.Compare(version1, version2);
        }

        var releaseVersion1 = version1.ReleaseVersion;
        var releaseVersion2 = version2.ReleaseVersion;

        // Compare ReleaseVersions if they both belong to the same Release ancestry.
        if (releaseVersion1.ReleaseId == releaseVersion2.ReleaseId)
        {
            // Delete the most recent version first.
            if (releaseVersion1.Version != releaseVersion2.Version)
            {
                return -releaseVersion1.Version.CompareTo(releaseVersion2.Version);
            }

            // Delete non-cancelled ReleaseVersions first.
            if (releaseVersion1.SoftDeleted != releaseVersion2.SoftDeleted)
            {
                return releaseVersion1.SoftDeleted ? 1 : -1;
            }

            return -releaseVersion1.Created.CompareTo(releaseVersion2.Created);
        }

        // If one ReleaseVersion contains a later version of a Public API DataSet than the other, order it
        // towards the top of the list so that it is deleted prior to a previous version of that DataSet
        // being deleted.
        foreach (var dataSetVersion in version1.DataSetVersions)
        {
            var matchingDataSetVersion = version2.DataSetVersions.SingleOrDefault(dsv2 =>
                dsv2.DataSetId == dataSetVersion.DataSetId
            );

            if (matchingDataSetVersion == null)
            {
                continue;
            }

            return -dataSetVersion.SemVersion().ComparePrecedenceTo(matchingDataSetVersion.SemVersion());
        }

        // Fall back to deleting the ReleaseVersion from the newest Release series first.
        return -releaseVersion1.Release.Created.CompareTo(releaseVersion2.Release.Created);
    }
}
