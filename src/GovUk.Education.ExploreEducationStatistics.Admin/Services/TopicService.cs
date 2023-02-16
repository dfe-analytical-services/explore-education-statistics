#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
using Microsoft.Extensions.Configuration;
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
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly IReleaseDataFileService _releaseDataFileService;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IPublishingService _publishingService;
        private readonly IMethodologyService _methodologyService;
        private readonly IBlobCacheService _cacheService;
        private readonly bool _topicDeletionAllowed;

        public TopicService(
            IConfiguration configuration,
            ContentDbContext contentContext,
            StatisticsDbContext statisticsContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IUserService userService,
            IReleaseSubjectRepository releaseSubjectRepository,
            IReleaseDataFileService releaseDataFileService,
            IReleaseFileService releaseFileService,
            IPublishingService publishingService,
            IMethodologyService methodologyService,
            IBlobCacheService cacheService)
        {
            _contentContext = contentContext;
            _statisticsContext = statisticsContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _userService = userService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _releaseDataFileService = releaseDataFileService;
            _releaseFileService = releaseFileService;
            _publishingService = publishingService;
            _methodologyService = methodologyService;
            _cacheService = cacheService;
            _topicDeletionAllowed = configuration.GetValue<bool>("enableThemeDeletion");
        }

        public async Task<Either<ActionResult, TopicViewModel>> CreateTopic(TopicSaveViewModel created)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccessDo(() => ValidateSelectedTheme(created.ThemeId))
                .OnSuccess(
                    async _ =>
                    {
                        if (_contentContext.Topics.Any(
                            topic => topic.Slug == created.Slug
                                     && topic.ThemeId == created.ThemeId
                        ))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        var saved = await _contentContext.Topics.AddAsync(
                            new Topic
                            {
                                Title = created.Title,
                                Slug = created.Slug,
                                ThemeId = created.ThemeId,
                            }
                        );

                        await _contentContext.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

                        return await GetTopic(saved.Entity.Id);
                    }
                );
        }

        public async Task<Either<ActionResult, TopicViewModel>> UpdateTopic(
            Guid topicId,
            TopicSaveViewModel updated)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(() => _persistenceHelper.CheckEntityExists<Topic>(topicId))
                .OnSuccessDo(() => ValidateSelectedTheme(updated.ThemeId))
                .OnSuccess(
                    async topic =>
                    {
                        if (_contentContext.Topics.Any(
                            t => t.Slug == updated.Slug
                                 && t.Id != topicId
                                 && t.ThemeId == updated.ThemeId
                        ))
                        {
                            return ValidationActionResult(ValidationErrorMessages.SlugNotUnique);
                        }

                        topic.Title = updated.Title;
                        topic.Slug = updated.Slug;
                        topic.ThemeId = updated.ThemeId;

                        _contentContext.Topics.Update(topic);
                        await _contentContext.SaveChangesAsync();

                        await _publishingService.TaxonomyChanged();

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
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(_mapper.Map<TopicViewModel>);
        }

        public async Task<Either<ActionResult, Unit>> DeleteTopic(Guid topicId)
        {
            return await _userService.CheckCanManageAllTaxonomy()
                .OnSuccess(
                    () => _persistenceHelper.CheckEntityExists<Topic>(
                        topicId,
                        q => q.Include(t => t.Publications)
                    )
                )
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccess(CheckCanDeleteTopic)
                .OnSuccessDo(async topic =>
                {
                    var publicationIdsToDelete = topic.Publications
                        .Select(p => p.Id)
                        .ToList();

                    return await DeleteMethodologiesForPublications(publicationIdsToDelete)
                        .OnSuccess(() => DeleteReleasesForPublications(publicationIdsToDelete));
                })
                .OnSuccess(async topic =>
                {
                    _contentContext.Topics.Remove(topic);
                    await _contentContext.SaveChangesAsync();
                    return await _publishingService.TaxonomyChanged();
                });
        }

        private async Task<Either<ActionResult, Unit>> DeleteMethodologiesForPublications(IEnumerable<Guid> publicationIds)
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
            // Some Content Db Releases may be soft-deleted and therefore not visible.
            // Ignore the query filter to make sure they are found
            var releasesIdsToDelete = _contentContext
                .Releases
                .IgnoreQueryFilters()
                .Where(r => publicationIds.Contains(r.PublicationId))
                .Select(release => new IdAndPreviousVersionIdPair<string>(release.Id.ToString(), release.PreviousVersionId != null ? release.PreviousVersionId.ToString() : null))
                .ToList();
            
            var releaseIdsInDeleteOrder = VersionedEntityDeletionOrderUtil
                .Sort(releasesIdsToDelete)
                .Select(ids => Guid.Parse(ids.Id));
                
            return await releaseIdsInDeleteOrder
                .Select(DeleteContentAndStatsRelease)
                .OnSuccessAll()
                .OnSuccessVoid();
        }

        private async Task<Either<ActionResult, Unit>> DeleteContentAndStatsRelease(Guid releaseId)
        {
            var contentRelease = await _contentContext
                .Releases
                .AsQueryable()
                .SingleOrDefaultAsync(r => r.Id == releaseId);

            if (contentRelease == null)
            {
                // The Content Db Release may already be soft-deleted and therefore not visible.
                // Attempt to hard delete the Content Db Release by ignoring the query filter
                await DeleteSoftDeletedContentDbRelease(releaseId);
                await DeleteStatsDbRelease(releaseId);
                return Unit.Instance;
            }

            return await _releaseDataFileService.DeleteAll(releaseId, forceDelete: true)
                .OnSuccessDo(() => _releaseFileService.DeleteAll(releaseId, forceDelete: true))
                .OnSuccessDo(() => DeleteCachedReleaseContent(releaseId))
                .OnSuccessVoid(async () =>
                {
                    _contentContext.Releases.Remove(contentRelease);

                    var keyStats = _contentContext.KeyStatistics
                        .Where(ks => ks.ReleaseId == releaseId);
                    _contentContext.KeyStatistics.RemoveRange(keyStats);

                    await _contentContext.SaveChangesAsync();

                    await DeleteStatsDbRelease(releaseId);
                });
        }

        private Task DeleteCachedReleaseContent(Guid releaseId)
        {
            return _cacheService.DeleteCacheFolder(new PrivateReleaseContentFolderCacheKey(releaseId));
        }

        private async Task DeleteSoftDeletedContentDbRelease(Guid releaseId)
        {
            var contentRelease = await _contentContext.Releases
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(r => r.Id == releaseId && r.SoftDeleted);

            if (contentRelease != null)
            {
                _contentContext.Releases.Remove(contentRelease);
                await _contentContext.SaveChangesAsync();
            }
        }

        private async Task DeleteStatsDbRelease(Guid releaseId)
        {
            var statsRelease = await _statisticsContext
                .Release
                .AsQueryable()
                .SingleOrDefaultAsync(r => r.Id == releaseId);

            if (statsRelease != null)
            {
                await _releaseSubjectRepository.DeleteAllReleaseSubjects(statsRelease.Id);
                _statisticsContext.Release.Remove(statsRelease);
                await _statisticsContext.SaveChangesAsync();
            }
        }

        private Either<ActionResult, Topic> CheckCanDeleteTopic(Topic topic)
        {
            if (!_topicDeletionAllowed)
            {
                return new ForbidResult();
            }

            // For now we only want to delete test topics as we
            // don't really have a mechanism to clean things up
            // properly across the entire application.
            // TODO: EES-1295 ability to completely delete releases
            if (!topic.Title.StartsWith("UI test topic"))
            {
                return new ForbidResult();
            }

            return topic;
        }

        private static IQueryable<Topic> HydrateTopicForTopicViewModel(IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
        }
    }
}
