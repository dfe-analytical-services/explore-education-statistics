using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;
        private readonly IMethodologyRepository _methodologyRepository;

        public MethodologyService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper,
            IMethodologyRepository methodologyRepository)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _methodologyRepository = methodologyRepository;
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> GetLatestMethodologyBySlug(string slug)
        {
            // TODO SOW4 EES-2375 lookup the MethodologyParent by slug when slug is moved to the parent
            // For now, this does a lookup on the parent via any Methodology with the slug
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(
                    query => query
                        .Include(mv => mv.MethodologyParent)
                        .ThenInclude(m => m.Versions)
                        .Where(mv => mv.Slug == slug))
                .OnSuccess<ActionResult, Methodology, MethodologyViewModel>(arbitraryVersion =>
                {
                    var latestPublishedVersion = arbitraryVersion.MethodologyParent.LatestPublishedVersion();
                    if (latestPublishedVersion == null)
                    {
                        return new NotFoundResult();
                    }

                    return _mapper.Map<MethodologyViewModel>(latestPublishedVersion);
                });
        }

        public async Task<Either<ActionResult, List<MethodologySummaryViewModel>>> GetSummariesByPublication(
            Guid publicationId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(publicationId)
                .OnSuccess(async publication =>
                {
                    var latestPublishedMethodologies =
                        await _methodologyRepository.GetLatestPublishedByPublication(publication.Id);
                    return _mapper.Map<List<MethodologySummaryViewModel>>(latestPublishedMethodologies);
                });
        }

        public async Task<Either<ActionResult, List<ThemeTree<PublicationMethodologiesTreeNode>>>> GetTree()
        {
            var themesWithMethodologies = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(t => t.Publications)
                .ThenInclude(p => p.Releases)
                .Include(t => t.Topics)
                .ThenInclude(t => t.Publications)
                .ThenInclude(p => p.Methodologies)
                .ThenInclude(pm => pm.MethodologyParent)
                .ThenInclude(m => m.Versions)
                .ToListAsync();

            return themesWithMethodologies.Where(IsThemePublished)
                .Select(BuildThemeTree)
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private static ThemeTree<PublicationMethodologiesTreeNode> BuildThemeTree(Theme theme)
        {
            return new ThemeTree<PublicationMethodologiesTreeNode>
            {
                Id = theme.Id,
                Summary = null,
                Title = theme.Title,
                Topics = theme.Topics
                    .Where(IsTopicPublished)
                    .Select(BuildTopicTree)
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private static TopicTree<PublicationMethodologiesTreeNode> BuildTopicTree(Topic topic)
        {
            return new TopicTree<PublicationMethodologiesTreeNode>
            {
                Id = topic.Id,
                Summary = null,
                Title = topic.Title,
                Publications = topic.Publications
                    .Where(IsPublicationPublished)
                    .Select(BuildPublicationNode)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static PublicationMethodologiesTreeNode BuildPublicationNode(Publication publication)
        {
            return new PublicationMethodologiesTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                Methodologies = publication.Methodologies.Select(BuildMethodologyForLatestVersion).ToList()
            };
        }

        private static bool IsThemePublished(Theme theme)
        {
            return theme.Topics.Any(IsTopicPublished);
        }

        private static bool IsTopicPublished(Topic topic)
        {
            return topic.Publications.Any(IsPublicationPublished);
        }

        private static bool IsPublicationPublished(Publication publication)
        {
            // TODO SOW4 Potentially remove this check on Releases in future
            var hasReleases = publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());

            if (hasReleases)
            {
                // TODO SOW4 There should be no LatestPublishedVersion of a Methodology returned if the Publication has no published releases
                // removing the need for the outer check on hasReleases
                var hasMethodologies = publication.Methodologies.Any(pm =>
                    pm.MethodologyParent.LatestPublishedVersion() != null);
                return hasMethodologies;
            }

            return false;
        }

        private static MethodologySummaryViewModel BuildMethodologyForLatestVersion(
            PublicationMethodology publicationMethodology)
        {
            var latestVersion = publicationMethodology.MethodologyParent.LatestPublishedVersion();
            return new MethodologySummaryViewModel
            {
                Id = latestVersion.Id,
                Slug = latestVersion.Slug,
                Title = latestVersion.Title
            };
        }
    }
}
