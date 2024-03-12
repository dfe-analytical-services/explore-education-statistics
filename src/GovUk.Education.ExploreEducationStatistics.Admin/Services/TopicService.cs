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
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
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
            IBlobCacheService cacheService,
            IReleasePublishingStatusRepository releasePublishingStatusRepository)
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
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
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
                        .OnSuccess(() => DeleteReleasesForPublications(publicationIdsToDelete))
                        .OnSuccess(() => DeletePublications(publicationIdsToDelete));
                })
                .OnSuccess(async topic =>
                {
                    _contentContext.Topics.Remove(topic);
                    await _contentContext.SaveChangesAsync();
                    return await _publishingService.TaxonomyChanged();
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
            var releaseVersionIdsToDelete = _contentContext
                .ReleaseVersions
                .IgnoreQueryFilters()
                .Where(rv => publicationIds.Contains(rv.PublicationId))
                .Select(rv => new IdAndPreviousVersionIdPair<string>(rv.Id.ToString(),
                    rv.PreviousVersionId != null ? rv.PreviousVersionId.ToString() : null))
                .ToList();

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

        private Task DeleteCachedReleaseContent(Guid releaseVersionId)
        {
            return _cacheService.DeleteCacheFolderAsync(new PrivateReleaseContentFolderCacheKey(releaseVersionId));
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
            if (!topic.Title.StartsWith("UI test topic") && !topic.Title.StartsWith("Seed topic"))
            {
                return new ForbidResult();
            }

            return topic;
        }

        private static IQueryable<Topic> HydrateTopicForTopicViewModel(IQueryable<Topic> values)
        {
            return values.Include(p => p.Publications);
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
    }
}
