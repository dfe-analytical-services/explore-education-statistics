using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentContext;
        private readonly StatisticsDbContext _statisticsContext;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
        private readonly IMethodologyService _methodologyService;
        private readonly IPublishingService _publishingService;
        private readonly IBlobCacheService _cacheService;
        private readonly bool _themeDeletionAllowed;

        public ThemeService(
            IOptions<AppOptions> appOptions,
            ContentDbContext contentContext,
            StatisticsDbContext statisticsContext,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IMethodologyService methodologyService,
            IReleaseFileService releaseFileService,
            IReleaseSubjectRepository releaseSubjectRepository,
            IReleaseDataFileService releaseDataFileService,
            IReleasePublishingStatusRepository releasePublishingStatusRepository,
            IPublishingService publishingService,
            IBlobCacheService cacheService)
        {
            _contentContext = contentContext;
            _statisticsContext = statisticsContext;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _methodologyService = methodologyService;
            _releaseFileService = releaseFileService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _releaseDataFileService = releaseDataFileService;
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
            _publishingService = publishingService;
            _cacheService = cacheService;
            _themeDeletionAllowed = appOptions.Value.EnableThemeDeletion;
        }

        public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(ThemeSaveViewModel created)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    async _ =>
                    {
                        if (await _contentContext.Themes.AnyAsync(theme => theme.Slug == created.Slug))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _contentContext.Themes.AddAsync(
                            new Theme
                            {
                                Slug = created.Slug,
                                Summary = created.Summary,
                                Title = created.Title,
                            }
                        );

                        await _contentContext.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(saved.Entity.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> UpdateTheme(
            Guid id,
            ThemeSaveViewModel updated)
        {
            return await _persistenceHelper.CheckEntityExists<Theme>(id)
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(
                    async theme =>
                    {
                        if (await _contentContext.Themes.AnyAsync(t => t.Slug == updated.Slug && t.Id != id))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        theme.Title = updated.Title;
                        theme.Slug = updated.Slug;
                        theme.Summary = updated.Summary;

                        await _contentContext.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(theme.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(id)
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(_mapper.Map<ThemeViewModel>);
        }

        public async Task<Either<ActionResult, List<ThemeViewModel>>> GetThemes()
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(
                    async _ => await _userService
                        .CheckCanManageAllTaxonomy()
                        .OnSuccess(
                            async () => await _contentContext.Themes
                                .ToListAsync()
                        )
                        .OrElse(GetUserThemes)
                )
                .OnSuccess(
                    list =>
                        list.Select(_mapper.Map<ThemeViewModel>)
                            .OrderBy(theme => theme.Title)
                            .ToList()
                );
        }

        public async Task<Either<ActionResult, Unit>> DeleteTheme(
            Guid themeId,
            CancellationToken cancellationToken = default)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(() => _persistenceHelper.CheckEntityExists<Theme>(themeId, q => q.Include(t => t.Publications)))
                .OnSuccessDo(CheckCanDeleteTheme)
                .OnSuccessVoid(async theme =>
                {
                    var publicationIdsToDelete = theme.Publications
                        .Select(p => p.Id)
                        .ToList();

                    await DeleteMethodologiesForPublications(publicationIdsToDelete)
                       .OnSuccess(() => DeleteReleasesForPublications(publicationIdsToDelete))
                       .OnSuccess(() => DeletePublications(publicationIdsToDelete));

                    _contentContext.Themes.Remove(theme);
                    await _contentContext.SaveChangesAsync(cancellationToken);

                    await _publishingService.TaxonomyChanged(cancellationToken);
                });
        }

        private async Task<Either<ActionResult, Unit>> DeleteMethodologiesForPublications(
            IEnumerable<Guid> publicationIds)
        {
            var methodologyIdsToDelete = await _contentContext
                .PublicationMethodologies
                .AsQueryable()
                .Where(pm => pm.Owner && publicationIds.Contains(pm.PublicationId))
                .Select(pm => pm.MethodologyId)
                .ToListAsync();

            return await methodologyIdsToDelete
                .Select(methodologyId => _methodologyService.DeleteMethodology(methodologyId, true))
                .OnSuccessAll()
                .OnSuccessVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeleteReleasesForPublications(IEnumerable<Guid> publicationIds)
        {
            var publications = await _contentContext
                .Publications
                .Where(publication => publicationIds.Contains(publication.Id))
                .ToListAsync();

            publications.ForEach(publication =>
            {
                publication.LatestPublishedReleaseVersion = null;
                publication.LatestPublishedReleaseVersionId = null;
            });

            await _contentContext.SaveChangesAsync();

            // Some Content Db Releases may be soft-deleted and therefore not visible.
            // Ignore the query filter to make sure they are found
            var releaseVersionIdsToDelete = await _contentContext
                .ReleaseVersions
                .IgnoreQueryFilters()
                .Where(rv => publicationIds.Contains(rv.PublicationId))
                .Select(rv => new IdAndPreviousVersionIdPair<string>(rv.Id.ToString(),
                    rv.PreviousVersionId != null ? rv.PreviousVersionId.ToString() : null))
                .ToListAsync();

            var releaseVersionIdsInDeleteOrder = VersionedEntityDeletionOrderUtil
                .Sort(releaseVersionIdsToDelete)
                .Select(ids => Guid.Parse(ids.Id))
                .ToList();

            // Delete release entries in the Azure Storage ReleaseStatus table - if not it will attempt to publish
            // deleted releases that were left scheduled
            await _releasePublishingStatusRepository.RemovePublisherReleaseStatuses(releaseVersionIdsInDeleteOrder);

            return await releaseVersionIdsInDeleteOrder
                .Select(DeleteContentAndStatsRelease)
                .OnSuccessAll()
                .OnSuccessVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeletePublications(IEnumerable<Guid> publicationIds)
        {
            var publications = await _contentContext
                .Publications
                .Where(publication => publicationIds.Contains(publication.Id))
                .ToListAsync();

            _contentContext.Publications.RemoveRange(publications);
            await _contentContext.SaveChangesAsync();
            return Unit.Instance;
        }

        private async Task<Either<ActionResult, Unit>> DeleteContentAndStatsRelease(Guid releaseVersionId)
        {
            var contentReleaseVersion = await _contentContext
                .ReleaseVersions
                .IgnoreQueryFilters()
                .SingleAsync(rv => rv.Id == releaseVersionId);

            if (!contentReleaseVersion.SoftDeleted)
            {
                var removeReleaseFilesAndCachedContent =
                    await _releaseDataFileService.DeleteAll(releaseVersionId, forceDelete: true)
                        .OnSuccessDo(() => _releaseFileService.DeleteAll(releaseVersionId, forceDelete: true))
                        .OnSuccessDo(() => DeleteCachedReleaseContent(releaseVersionId));

                if (removeReleaseFilesAndCachedContent.IsLeft)
                {
                    return removeReleaseFilesAndCachedContent;
                }
            }

            await RemoveReleaseDependencies(releaseVersionId);
            await DeleteStatsDbRelease(releaseVersionId);

            _contentContext.ReleaseVersions.Remove(contentReleaseVersion);
            await _contentContext.SaveChangesAsync();
            return Unit.Instance;
        }

        private async Task DeleteStatsDbRelease(Guid releaseVersionId)
        {
            var statsRelease = await _statisticsContext
                .ReleaseVersion
                .AsQueryable()
                .SingleOrDefaultAsync(rv => rv.Id == releaseVersionId);

            if (statsRelease != null)
            {
                await _releaseSubjectRepository.DeleteAllReleaseSubjects(releaseVersionId: statsRelease.Id,
                    softDeleteOrphanedSubjects: false);
                _statisticsContext.ReleaseVersion.Remove(statsRelease);
                await _statisticsContext.SaveChangesAsync();
            }
        }

        private Task DeleteCachedReleaseContent(Guid releaseVersionId)
        {
            return _cacheService.DeleteCacheFolderAsync(new PrivateReleaseContentFolderCacheKey(releaseVersionId));
        }

        private async Task RemoveReleaseDependencies(Guid releaseVersionId)
        {
            var keyStats = _contentContext
                .KeyStatistics
                .Where(ks => ks.ReleaseVersionId == releaseVersionId);

            _contentContext.KeyStatistics.RemoveRange(keyStats);

            var dataBlockVersions = await _contentContext
                .DataBlockVersions
                .Include(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
                .ToListAsync();

            var dataBlockParents = dataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .Distinct()
                .ToList();

            // Unset the DataBlockVersion references from the DataBlockParent.
            dataBlockParents.ForEach(dataBlockParent =>
            {
                dataBlockParent.LatestDraftVersionId = null;
                dataBlockParent.LatestPublishedVersionId = null;
            });

            await _contentContext.SaveChangesAsync();

            // Then remove the now-unreferenced DataBlockVersions.
            _contentContext.DataBlockVersions.RemoveRange(dataBlockVersions);
            await _contentContext.SaveChangesAsync();

            // And finally, delete the DataBlockParents if they are now orphaned.
            var orphanedDataBlockParents = dataBlockParents
                .Where(dataBlockParent =>
                    !_contentContext
                        .DataBlockVersions
                        .Any(dataBlockVersion => dataBlockVersion.DataBlockParentId == dataBlockParent.Id))
                .ToList();

            _contentContext.DataBlockParents.RemoveRange(orphanedDataBlockParents);
            await _contentContext.SaveChangesAsync();
        }

        public async Task<Either<ActionResult, Unit>> DeleteUITestThemes(CancellationToken cancellationToken = default)
        {
            return !_themeDeletionAllowed
                ? (Either<ActionResult, Unit>)new ForbidResult()
                : await _userService.CheckCanManageAllTaxonomy()
                    .OnSuccess(_ => _contentContext.Themes
                        .Where(theme => theme.Title.Contains("UI test")))
                    .OnSuccessVoid(async themes =>
                    {
                        foreach (var theme in themes)
                        {
                            await DeleteTheme(theme.Id, cancellationToken);
                        }

                        await _contentContext.SaveChangesAsync(cancellationToken);
                        await _publishingService.TaxonomyChanged(cancellationToken);
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
            if (!theme.Title.StartsWith("UI test theme") && !theme.Title.StartsWith("Seed theme"))
            {
                return new ForbidResult();
            }

            return await Task.FromResult(Unit.Instance);
        }

        private async Task<List<Theme>> GetUserThemes()
        {
            var userId = _userService.GetUserId();

            return await _contentContext
                .UserReleaseRoles
                .AsQueryable()
                .Where(userReleaseRole =>
                    userReleaseRole.UserId == userId &&
                    userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.ReleaseVersion.Publication)
                .Concat(_contentContext
                    .UserPublicationRoles
                    .AsQueryable()
                    .Where(userPublicationRole =>
                        userPublicationRole.UserId == userId &&
                        ListOf(Owner, Approver).Contains(userPublicationRole.Role))
                    .Select(userPublicationRole => userPublicationRole.Publication))
                .Select(publication => publication.Theme)
                .Distinct()
                .ToListAsync();
        }
    }
}
