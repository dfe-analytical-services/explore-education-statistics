using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private static readonly VersionedEntityDeletionOrderComparer VersionedEntityComparer = 
            new VersionedEntityDeletionOrderComparer();

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
            IMethodologyService methodologyService)
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
                            .ThenInclude(p => p.Releases)
                    )
                )
                .OnSuccessDo(_userService.CheckCanManageAllTaxonomy)
                .OnSuccessDo(CheckCanDeleteTopic)
                .OnSuccessVoid(async topic =>
                {
                    var publicationIdsToDelete = topic.Publications.Select(p => p.Id);

                    var releaseIdsToDelete = _statisticsContext
                        .Release
                        .Where(r => publicationIdsToDelete.Contains(r.PublicationId))
                        .ToList()
                        .OrderBy(release => new IdAndPreviousVersionIdPair(release.Id, release.PreviousVersionId),
                            VersionedEntityComparer)
                        .Select(r => r.Id);

                    foreach (var releaseId in releaseIdsToDelete)
                    {
                        await _releaseDataFileService.DeleteAll(releaseId, forceDelete: true);
                        await _releaseFileService.DeleteAll(releaseId, forceDelete: true);
                        await _releaseSubjectRepository.DeleteAllReleaseSubjects(releaseId);
                        
                        var statsRelease = await _statisticsContext
                            .Release
                            .Where(r => r.Id == releaseId)
                            .SingleAsync();
                        
                        var contentRelease = await _contentContext
                            .Releases
                            .Where(r => r.Id == releaseId)
                            .SingleAsync();

                        _statisticsContext.Release.Remove(statsRelease);
                        _contentContext.Releases.Remove(contentRelease);
                    }

                    await _contentContext.SaveChangesAsync();
                    await _statisticsContext.SaveChangesAsync();
                    
                    var methodologiesToDelete = _contentContext
                        .PublicationMethodologies
                        .Include(pm => pm.MethodologyParent)
                        .ThenInclude(pm => pm.Versions)
                        .Where(pm => publicationIdsToDelete.Contains(pm.PublicationId))
                        .SelectMany(pm => pm.MethodologyParent.Versions)
                        .ToList()
                        .OrderBy(m => new IdAndPreviousVersionIdPair(m.Id, m.PreviousVersionId),
                            VersionedEntityComparer);

                    await methodologiesToDelete.ForEachAsync(methodology => 
                        _methodologyService.DeleteMethodology(methodology.Id, forceDelete: true));
                    
                    _contentContext.Topics.Remove(
                        await _contentContext
                            .Topics
                            .Where(t => t.Id == topic.Id)
                            .SingleAsync()
                    );

                    await _contentContext.SaveChangesAsync();
                    await _statisticsContext.SaveChangesAsync();

                    await _publishingService.TaxonomyChanged();
                });
        }

        private async Task<Either<ActionResult, Unit>> CheckCanDeleteTopic(Topic topic)
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

            return Unit.Instance;
        }

        private static IQueryable<Topic> HydrateTopicForTopicViewModel(IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
        }
        
        /// <summary>
        /// An IComparer implementation that orders entities based upon having previous versions.  This IComparer will
        /// order the entities so that entities that have no previous versions will appear at the end of the list,
        /// entities that have that set of entities as previous versions will appear before them etc. 
        /// </summary>
        public class VersionedEntityDeletionOrderComparer : IComparer<IdAndPreviousVersionIdPair>
        {
            public int Compare(IdAndPreviousVersionIdPair entity1Ids, IdAndPreviousVersionIdPair entity2Ids)
            {
                if (entity1Ids == null)
                {
                    return 1;
                }
                
                if (entity2Ids == null)
                {
                    return -1;
                }

                if (entity1Ids.PreviousVersionId == null)
                {
                    return 1;
                }
                
                if (entity2Ids.PreviousVersionId == null)
                {
                    return -1;
                }
                
                if (entity1Ids.PreviousVersionId == entity2Ids.Id)
                {
                    return -1;
                }

                return entity2Ids.PreviousVersionId == entity1Ids.Id ? 1 : -1;
            }
        }

        public class IdAndPreviousVersionIdPair
        {
            public readonly Guid Id;
            public readonly Guid? PreviousVersionId;

            public IdAndPreviousVersionIdPair(Guid id, Guid? previousVersionId)
            {
                Id = id;
                PreviousVersionId = previousVersionId;
            }
        }
    }
}
