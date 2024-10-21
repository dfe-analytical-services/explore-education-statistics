using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IMethodologyService _methodologyService;
        private readonly IPublishingService _publishingService;
        private readonly IReleaseService _releaseService;
        private readonly bool _themeDeletionAllowed;

        public ThemeService(
            IOptions<AppOptions> appOptions,
            ContentDbContext contentDbContext,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IMethodologyService methodologyService,
            IPublishingService publishingService,
            IReleaseService releaseService)
        {
            _contentDbContext = contentDbContext;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _methodologyService = methodologyService;
            _publishingService = publishingService;
            _releaseService = releaseService;
            _themeDeletionAllowed = appOptions.Value.EnableThemeDeletion;
        }

        public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(ThemeSaveViewModel created)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    async _ =>
                    {
                        if (await _contentDbContext.Themes.AnyAsync(theme => theme.Slug == created.Slug))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _contentDbContext.Themes.AddAsync(
                            new Theme
                            {
                                Slug = created.Slug,
                                Summary = created.Summary,
                                Title = created.Title,
                            }
                        );

                        await _contentDbContext.SaveChangesAsync();

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
                        if (await _contentDbContext.Themes.AnyAsync(t => t.Slug == updated.Slug && t.Id != id))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        theme.Title = updated.Title;
                        theme.Slug = updated.Slug;
                        theme.Summary = updated.Summary;

                        await _contentDbContext.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(theme.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id)
        {
            return await _userService
                .CheckCanManageAllTaxonomy()
                .OnSuccess(() => _persistenceHelper.CheckEntityExists<Theme>(id))
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
                            async () => await _contentDbContext.Themes
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
                .OnSuccess(() =>
                    _persistenceHelper.CheckEntityExists<Theme>(themeId, q => q.Include(t => t.Publications)))
                .OnSuccessDo(CheckCanDeleteTheme)
                .OnSuccessDo(theme => DeletePublicationsForTheme(theme, cancellationToken))
                .OnSuccessVoid(async theme =>
                {
                    _contentDbContext.Themes.Remove(theme);
                    await _contentDbContext.SaveChangesAsync(cancellationToken);

                    await _publishingService.TaxonomyChanged(cancellationToken);
                });
        }

        private async Task<Either<List<ActionResult>, Unit>> DeletePublicationsForTheme(
            Theme theme, CancellationToken cancellationToken)
        {
            var publicationIds = theme
                .Publications
                .Select(publication => publication.Id)
                .ToList();

            var deletePublicationResults = await publicationIds
                .ToAsyncEnumerable()
                .SelectAwait(async publicationId =>
                    await DeleteMethodologiesForPublication(publicationId)
                        .OnSuccess(() => DeleteReleasesForPublication(publicationId))
                        .OnSuccess(() => DeletePublication(publicationId)))
                .ToListAsync(cancellationToken);

            return deletePublicationResults
                .AggregateSuccessesAndFailures()
                .OnSuccessVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeleteMethodologiesForPublication(
            Guid publicationId)
        {
            var methodologyIdsToDelete = await _contentDbContext
                .PublicationMethodologies
                .AsQueryable()
                .Where(pm => pm.Owner && pm.PublicationId == publicationId)
                .Select(pm => pm.MethodologyId)
                .ToListAsync();

            return await methodologyIdsToDelete
                .Select(methodologyId => _methodologyService.DeleteMethodology(methodologyId, true))
                .OnSuccessAllReturnVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeleteReleasesForPublication(Guid publicationId)
        {
            var publications = await _contentDbContext
                .Publications
                .Where(publication => publication.Id == publicationId)
                .ToListAsync();

            publications.ForEach(publication =>
            {
                publication.LatestPublishedReleaseVersion = null;
                publication.LatestPublishedReleaseVersionId = null;
            });

            _contentDbContext.UpdateRange(publications);
            await _contentDbContext.SaveChangesAsync();

            // Some Content Db Releases may be soft-deleted and therefore not visible.
            // Ignore the query filter to make sure they are found
            var releaseVersionIdsToDelete = await _contentDbContext
                .ReleaseVersions
                .IgnoreQueryFilters()
                .Where(rv => rv.PublicationId == publicationId)
                .Select(rv => new IdAndPreviousVersionIdPair<string>(rv.Id.ToString(),
                    rv.PreviousVersionId != null ? rv.PreviousVersionId.ToString() : null))
                .ToListAsync();

            var releaseVersionIdsInDeleteOrder = VersionedEntityDeletionOrderUtil
                .Sort(releaseVersionIdsToDelete)
                .Select(ids => Guid.Parse(ids.Id))
                .ToList();

            return await releaseVersionIdsInDeleteOrder
                .Select(releaseVersionId => _releaseService.DeleteTestReleaseVersion(releaseVersionId))
                .OnSuccessAllReturnVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeletePublication(Guid publicationId)
        {
            var publication = await _contentDbContext
                .Publications
                .Include(publication => publication.Contact)
                .SingleAsync(publication => publication.Id == publicationId);

            _contentDbContext.Publications.RemoveRange(publication);
            _contentDbContext.Contacts.RemoveRange(publication.Contact);
            await _contentDbContext.SaveChangesAsync();
            return Unit.Instance;
        }

        public async Task<Either<ActionResult, Unit>> DeleteUITestThemes(CancellationToken cancellationToken = default)
        {
            return !_themeDeletionAllowed
                ? new ForbidResult()
                : await _userService.CheckCanManageAllTaxonomy()
                    .OnSuccess(async _ => (await _contentDbContext
                            .Themes
                            .ToListAsync(cancellationToken))
                        .Where(theme => theme.IsTestOrSeedTheme()))
                    .OnSuccessVoid(async themes =>
                    {
                        foreach (var theme in themes)
                        {
                            await DeleteTheme(theme.Id, cancellationToken);
                        }

                        await _contentDbContext.SaveChangesAsync(cancellationToken);
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
            if (!theme.IsTestOrSeedTheme())
            {
                return new ForbidResult();
            }

            return await Task.FromResult(Unit.Instance);
        }

        private async Task<List<Theme>> GetUserThemes()
        {
            var userId = _userService.GetUserId();

            return await _contentDbContext
                .UserReleaseRoles
                .AsQueryable()
                .Where(userReleaseRole =>
                    userReleaseRole.UserId == userId &&
                    userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.ReleaseVersion.Publication)
                .Concat(_contentDbContext
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
