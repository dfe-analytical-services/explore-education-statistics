using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

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

        public ThemeService(
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
        }

        public async Task<Either<ActionResult, ThemeViewModel>> CreateTheme(SaveThemeViewModel createdTheme)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    async _ =>
                    {
                        if (_context.Themes.Any(theme => theme.Slug == createdTheme.Slug))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _context.Themes.AddAsync(
                            new Theme
                            {
                                Slug = createdTheme.Slug,
                                Summary = createdTheme.Summary,
                                Title = createdTheme.Title,
                            }
                        );

                        await _context.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(saved.Entity.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> UpdateTheme(
            Guid themeId,
            SaveThemeViewModel updatedTheme)
        {
            return await _persistenceHelper.CheckEntityExists<Theme>(themeId)
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(
                    async theme =>
                    {
                        if (_context.Themes.Any(t => t.Slug == updatedTheme.Slug && t.Id != themeId))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        theme.Title = updatedTheme.Title;
                        theme.Slug = updatedTheme.Slug;
                        theme.Summary = updatedTheme.Summary;

                        await _context.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTheme(theme.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, ThemeViewModel>> GetTheme(Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Theme>(id)
                .OnSuccess(_userService.CheckCanViewTheme)
                .OnSuccess(_mapper.Map<ThemeViewModel>);
        }

        public async Task<Either<ActionResult, List<ThemeViewModel>>> GetThemes()
        {
            return await _userService
                .CheckCanAccessSystem()
                .OnSuccess(
                    async _ => await _userService
                        .CheckCanViewAllTopics()
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
                .OnSuccessVoid(
                    async theme =>
                    {
                        // For now we only want to delete test themes as we
                        // don't really have a mechanism to clean things up
                        // properly across the entire application.
                        // TODO: EES-1295 ability to completely delete releases
                        if (!theme.Title.StartsWith("UI test theme"))
                        {
                            return;
                        }

                        var topicIds = theme.Topics.Select(topic => topic.Id).ToList();

                        foreach (var topicId in topicIds)
                        {
                            await _topicService.DeleteTopic(topicId);
                        }

                        _context.Themes.Remove(theme);
                        await _context.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();
                    }
                );
        }

        private async Task<List<Theme>> GetUserThemes()
        {
            var userId = _userService.GetUserId();

            var topics = await _context
                .UserReleaseRoles
                .Include(r => r.Release)
                .ThenInclude(release => release.Publication)
                .ThenInclude(publication => publication.Topic)
                .ThenInclude(topic => topic.Theme)
                .Where(r => r.UserId == userId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.Release.Publication.Topic)
                .Distinct()
                .ToListAsync();

            return topics
                .GroupBy(topic => topic.Theme)
                .Select(
                    group =>
                    {
                        group.Key.Topics = group.ToList();
                        return group.Key;
                    }
                )
                .ToList();
        }
    }
}