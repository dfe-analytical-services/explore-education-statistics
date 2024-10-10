using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ITopicService _topicService;
        private readonly IPublishingService _publishingService;
        private readonly bool _themeDeletionAllowed;

        public ThemeService(
            IOptions<AppOptions> appOptions,
            ContentDbContext context,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            ITopicService topicService,
            IPublishingService publishingService)
        {
            _context = context;
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _topicService = topicService;
            _publishingService = publishingService;
            _themeDeletionAllowed = appOptions.Value.EnableThemeDeletion;
        }

        public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(ThemeSaveViewModel created)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    async _ =>
                    {
                        if (await _context.Themes.AnyAsync(theme => theme.Slug == created.Slug))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _context.Themes.AddAsync(
                            new Theme
                            {
                                Slug = created.Slug,
                                Summary = created.Summary,
                                Title = created.Title,
                            }
                        );

                        await _context.SaveChangesAsync();

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
                        if (await _context.Themes.AnyAsync(t => t.Slug == updated.Slug && t.Id != id))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        theme.Title = updated.Title;
                        theme.Slug = updated.Slug;
                        theme.Summary = updated.Summary;

                        await _context.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(theme.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(id, q =>
                    q.Include(t => t.Topics))
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
                            async () => await _context.Themes
                                .Include(theme => theme.Topics)
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

        public async Task<Either<ActionResult, Unit>> DeleteTheme(Guid themeId)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    () => _persistenceHelper.CheckEntityExists<Theme>(
                        themeId,
                        q => q.Include(t => t.Topics)
                    )
                )
                .OnSuccessDo(CheckCanDeleteTheme)
                .OnSuccessVoid(async theme =>
                {
                    var topicIds = theme.Topics.Select(topic => topic.Id).ToList();
                    await topicIds.ForEachAsync(_topicService.DeleteTopic);

                    _context.Themes.Remove(theme);
                    await _context.SaveChangesAsync();

                    await _publishingService.TaxonomyChanged();
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteUITestThemes(CancellationToken cancellationToken = default)
        {
            return !_themeDeletionAllowed
                ? (Either<ActionResult, Unit>)new ForbidResult()
                : await _userService.CheckCanManageAllTaxonomy()
                    .OnSuccess(_ => _context.Themes
                        .Where(theme => theme.Title.Contains("UI test"))
                        .Include(theme => theme.Topics))
                    .OnSuccessVoid(async themes =>
                    {
                        foreach (var theme in themes)
                        {
                            var topicIds = theme.Topics.Select(topic => topic.Id).ToList();
                            await topicIds.ToAsyncEnumerable().ForEachAwaitAsync(_topicService.DeleteTopic, cancellationToken);

                            _context.Themes.Remove(theme);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
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

            var topics = await _context
                .UserReleaseRoles
                .AsQueryable()
                .Where(userReleaseRole =>
                    userReleaseRole.UserId == userId &&
                    userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
                .Select(userReleaseRole => userReleaseRole.ReleaseVersion.Publication)
                .Concat(_context
                    .UserPublicationRoles
                    .AsQueryable()
                    .Where(userPublicationRole =>
                        userPublicationRole.UserId == userId &&
                        ListOf(Owner, Approver).Contains(userPublicationRole.Role))
                    .Select(userPublicationRole => userPublicationRole.Publication))
                .Include(publication => publication.Topic)
                .ThenInclude(topic => topic.Theme)
                .Select(publication => publication.Topic)
                .Distinct()
                .ToListAsync();

            return topics
                .GroupBy(topic => topic.Theme)
                .Select(group =>
                    {
                        group.Key.Topics = group.ToList();
                        return group.Key;
                    })
                .ToList();
        }
    }
}
