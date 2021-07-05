using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
                .CheckEntityExists<Methodology>(query => 
                    query.Where(mv => mv.Slug == slug))
                .OnSuccess<ActionResult, Methodology, MethodologyViewModel>(async arbitraryVersion =>
                {
                    var latestPublishedVersion = await _methodologyRepository.GetLatestPublishedByMethodologyParent(arbitraryVersion.MethodologyParentId);
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
                .OnSuccess(BuildMethodologiesForPublication);
        }

        public async Task<Either<ActionResult, List<ThemeTree<PublicationMethodologiesTreeNode>>>> GetTree()
        {
            var themesWithMethodologies = await _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ToListAsync();

            return (await themesWithMethodologies.WhereAsync(IsThemeIncluded)
                .SelectAsync(BuildThemeTree))
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        private async Task<ThemeTree<PublicationMethodologiesTreeNode>> BuildThemeTree(Theme theme)
        {
            var topics = (await theme.Topics
                    .WhereAsync(IsTopicIncluded)
                    .SelectAsync(BuildTopicTree))
                .OrderBy(topic => topic.Title)
                .ToList();

            return new ThemeTree<PublicationMethodologiesTreeNode>
            {
                Id = theme.Id,
                Summary = null,
                Title = theme.Title,
                Topics = topics
            };
        }

        private async Task<TopicTree<PublicationMethodologiesTreeNode>> BuildTopicTree(Topic topic)
        {
            var publications = (await topic.Publications
                .WhereAsync(IsPublicationIncluded)
                .SelectAsync(BuildPublicationNode))
                .OrderBy(publication => publication.Title)
                .ToList();

            return new TopicTree<PublicationMethodologiesTreeNode>
            {
                Id = topic.Id,
                Summary = null,
                Title = topic.Title,
                Publications = publications
            };
        }

        private async Task<PublicationMethodologiesTreeNode> BuildPublicationNode(Publication publication)
        {
            return new PublicationMethodologiesTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                Methodologies = await BuildMethodologiesForPublication(publication)
            };
        }

        private async Task<bool> IsThemeIncluded(Theme theme)
        {
            foreach (var topic in theme.Topics)
            {
                if (await IsTopicIncluded(topic))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsTopicIncluded(Topic topic)
        {
            foreach (var publication in topic.Publications)
            {
                if (await IsPublicationIncluded(publication))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsPublicationIncluded(Publication publication)
        {
            var publishedMethodologies = await _methodologyRepository.GetLatestPublishedByPublication(publication.Id);
            return publishedMethodologies.Any();
        }

        private async Task<List<MethodologySummaryViewModel>> BuildMethodologiesForPublication(Publication publication)
        {
            var latestPublishedMethodologies =
                await _methodologyRepository.GetLatestPublishedByPublication(publication.Id);
            return _mapper.Map<List<MethodologySummaryViewModel>>(latestPublishedMethodologies);
        }
    }
}
