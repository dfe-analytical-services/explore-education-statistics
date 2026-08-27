#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Events;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

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

                await eventRaiser.OnThemeUpdated(theme);

                return await GetTheme(theme.Id);
            });
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

    public async Task<Either<ActionResult, Unit>> DeleteThemes(
        List<Guid> themeIds,
        CancellationToken cancellationToken = default
    )
    {
        // This deliberately runs without a transaction. Deleting a Theme cascades into deletions across the
        // content and statistics databases, the Public API's PostgreSQL database, Azure blob storage and Azure
        // storage tables, and reaches the latter three via a blocking HTTP call out to the Public Data
        // Processor. That call reads and writes the same content database rows as this operation, so holding a
        // transaction open across it makes the Processor wait on locks that cannot be released until the
        // Processor itself responds, which SQL Server cannot detect as a deadlock and which therefore blocks
        // until the command timeout expires. A transaction could not have made the operation atomic in any
        // case, as the blob and Public API deletions are already irreversible by the time it would roll back.
        // Instead, ReleaseVersions are deleted in a dependency-safe order so that a partial deletion can be
        // completed by retrying.
        return await CheckCanDeleteThemes()
            .OnSuccess(async _ => await userService.CheckCanManageAllTaxonomy())
            .OnSuccess<ActionResult, Unit, Unit>(async _ =>
            {
                var themes = await contentDbContext
                    .Themes.Where(t => themeIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                var notFoundThemeIds = themeIds.Except(themes.Select(theme => theme.Id)).ToList();

                var failures = new List<ActionResult>();
                var deletedThemeCount = 0;

                foreach (var theme in themes)
                {
                    var result = await DeletePublicationsForTheme(theme.Id, cancellationToken)
                        .OnSuccessDo(() => contentDbContext.Themes.Remove(theme));

                    if (result.IsLeft)
                    {
                        failures.AddRange(result.Left);
                    }
                    else
                    {
                        deletedThemeCount++;
                    }
                }

                await contentDbContext.SaveChangesAsync(cancellationToken);
                await publishingService.TaxonomyChanged(cancellationToken);

                if (notFoundThemeIds.Count > 0)
                {
                    logger.LogWarning(
                        "Themes not found during deletion, and therefore not deleted: {NotFoundThemeIds}",
                        notFoundThemeIds
                    );

                    // Only fail the request with a 404 if nothing was deleted. Returning one alongside
                    // successful deletions would misleadingly suggest that no Themes were deleted at all.
                    if (deletedThemeCount == 0)
                    {
                        failures.AddRange(
                            notFoundThemeIds.Select(themeId =>
                                ValidationUtils.NotFoundResult<Theme, Guid>(themeId, nameof(themeIds))
                            )
                        );
                    }
                }

                return failures.Count > 0 ? failures[0] : Unit.Instance;
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

                await eventRaiser.OnPublicationDeleted(publication.Id, publication.Slug, latestPublicationRelease);
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteUITestThemes(CancellationToken cancellationToken = default)
    {
        var themes = await contentDbContext.Themes.ToListAsync(cancellationToken);

        var testThemeIds = themes.Where(theme => theme.IsTestOrSeedTheme()).Select(theme => theme.Id).ToList();

        return themes.Count > 0
            ? await DeleteThemes(testThemeIds, cancellationToken)
            : new OkObjectResult("No test themes to delete.");
    }

    private async Task<Either<ActionResult, Unit>> CheckCanDeleteThemes()
    {
        if (!_themeDeletionAllowed)
        {
            return new ForbidResult();
        }

        return await Task.FromResult(Unit.Instance);
    }

    private async Task<List<Theme>> GetUserThemes()
    {
        var userId = userService.GetUserId();

        return await userPublicationRoleRepository
            .Query()
            .AsNoTracking()
            .WhereForUser(userId)
            .WhereRolesIn([PublicationRole.Drafter, PublicationRole.Approver])
            .Select(upr => upr.Publication.Theme)
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
