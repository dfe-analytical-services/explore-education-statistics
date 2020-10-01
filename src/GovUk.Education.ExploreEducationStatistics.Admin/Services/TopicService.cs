using System;
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
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * This service is currently used by the test scripts for the purposes of creating data
     */
    public class TopicService : ITopicService
    {
        private readonly ContentDbContext _contentContext;
        private readonly StatisticsDbContext _statisticsContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IReleaseSubjectService _releaseSubjectService;
        private readonly IReleaseFilesService _releaseFilesService;

        public TopicService(
            ContentDbContext contentContext,
            StatisticsDbContext statisticsContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IUserService userService,
            IReleaseSubjectService releaseSubjectService,
            IReleaseFilesService releaseFilesService)
        {
            _contentContext = contentContext;
            _statisticsContext = statisticsContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _userService = userService;
            _releaseSubjectService = releaseSubjectService;
            _releaseFilesService = releaseFilesService;
        }

        public async Task<Either<ActionResult, TopicViewModel>> CreateTopic(SaveTopicViewModel createdTopic)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccessDo(() => ValidateSelectedTheme(createdTopic.ThemeId))
                .OnSuccess(
                    async _ =>
                    {
                        if (_contentContext.Topics.Any(
                            topic => topic.Slug == createdTopic.Slug
                                     && topic.ThemeId == createdTopic.ThemeId
                        ))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _contentContext.Topics.AddAsync(
                            new Topic
                            {
                                Title = createdTopic.Title,
                                Slug = createdTopic.Slug,
                                ThemeId = createdTopic.ThemeId,
                            }
                        );

                        await _contentContext.SaveChangesAsync();

                        return await GetTopic(saved.Entity.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, TopicViewModel>> UpdateTopic(
            Guid topicId,
            SaveTopicViewModel updatedTopic)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(() => _persistenceHelper.CheckEntityExists<Topic>(topicId))
                .OnSuccessDo(() => ValidateSelectedTheme(updatedTopic.ThemeId))
                .OnSuccess(
                    async topic =>
                    {
                        if (_contentContext.Topics.Any(
                            t => t.Slug == updatedTopic.Slug
                                 && t.Id != topicId
                                 && t.ThemeId == updatedTopic.ThemeId
                        ))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        topic.Title = updatedTopic.Title;
                        topic.Slug = updatedTopic.Slug;
                        topic.ThemeId = updatedTopic.ThemeId;

                        _contentContext.Topics.Update(topic);
                        await _contentContext.SaveChangesAsync();

                        return await GetTopic(topic.Id);
                    }
                );
        }

        private async Task<Either<ActionResult, Unit>> ValidateSelectedTheme(Guid themeId)
        {
            var theme = await _contentContext.Themes.FindAsync(themeId);

            if (theme == null)
            {
                return ValidationActionResult(ValidationErrorMessages.ThemeDoesNotExist);
            }

            return Unit.Instance;
        }

        public async Task<Either<ActionResult, TopicViewModel>> GetTopic(Guid topicId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Topic>(topicId, HydrateTopicForTopicViewModel)
                .OnSuccess(_userService.CheckCanViewTopic)
                .OnSuccess(_mapper.Map<TopicViewModel>);
        }

        public async Task<Either<ActionResult, Unit>> DeleteTopic(Guid topicId)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    () => _persistenceHelper.CheckEntityExists<Topic>(
                        topicId,
                        q => q.Include(t => t.Publications)
                            .ThenInclude(p => p.Releases)
                    )
                )
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccessVoid(
                    async topic =>
                    {
                        // For now we only want to delete test topics as we
                        // don't really have a mechanism to clean things up
                        // properly across the entire application.
                        // TODO: EES-1295 ability to completely delete releases
                        if (!topic.Title.StartsWith("UI test topic"))
                        {
                            return;
                        }

                        foreach (var release in topic.Publications.SelectMany(publication => publication.Releases))
                        {
                            await _releaseFilesService.DeleteAllFiles(release.Id, forceDelete: true);
                            await _releaseSubjectService.DeleteAllSubjectsOrBreakReleaseLinks(release.Id);
                        }

                        _statisticsContext.Release.RemoveRange(
                            _statisticsContext.Release.Where(r => r.Publication.TopicId == topic.Id)
                        );

                        _statisticsContext.Topic.RemoveRange(
                            _statisticsContext.Topic.Where(t => t.Id == topic.Id)
                        );

                        await _statisticsContext.SaveChangesAsync();

                        _contentContext.Topics.RemoveRange(
                            _contentContext.Topics.Where(t => t.Id == topic.Id)
                        );

                        await _contentContext.SaveChangesAsync();
                    }
                );
        }

        private static IQueryable<Topic> HydrateTopicForTopicViewModel(IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
        }
    }
}