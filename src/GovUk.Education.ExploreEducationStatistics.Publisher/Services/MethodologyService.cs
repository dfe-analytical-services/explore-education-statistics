﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public MethodologyService(ContentDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Methodology> Get(Guid id)
        {
            return await _context.Methodologies.FindAsync(id);
        }

        public async Task<MethodologyViewModel> GetViewModelAsync(Guid id, PublishContext context)
        {
            var methodology = await Get(id);

            var methodologyViewModel =  _mapper.Map<MethodologyViewModel>(methodology);

            // If the methodology isn't live yet set the published date based on what we expect it to be
            methodologyViewModel.Published ??= context.Published;

            return methodologyViewModel;
        }

        public List<ThemeTree<MethodologyTreeNode>> GetTree(IEnumerable<Guid> includedReleaseIds)
        {
            return _context.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Methodology)
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToList()
                .Where(theme => IsThemePublished(theme, includedReleaseIds))
                .Select(theme => BuildThemeTree(theme, includedReleaseIds))
                .OrderBy(theme => theme.Title)
                .ToList();
        }

        public async Task SetPublishedDate(Guid id, DateTime published)
        {
            var methodology = await Get(id);

            if (methodology == null)
            {
                throw new ArgumentException("Methodology does not exist", nameof(id));
            }

            _context.Update(methodology);
            methodology.Published = published;
            await _context.SaveChangesAsync();
        }

        private static ThemeTree<MethodologyTreeNode> BuildThemeTree(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return new ThemeTree<MethodologyTreeNode>
            {
                Id = theme.Id,
                Title = theme.Title,
                Topics = theme.Topics
                    .Where(topic => IsTopicPublished(topic, includedReleaseIds))
                    .Select(topic => BuildTopicTree(topic, includedReleaseIds))
                    .OrderBy(topic => topic.Title)
                    .ToList()
            };
        }

        private static TopicTree<MethodologyTreeNode> BuildTopicTree(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return new TopicTree<MethodologyTreeNode>
            {
                Id = topic.Id,
                Title = topic.Title,
                Publications = topic.Publications
                    .Where(publication => IsPublicationPublished(publication, includedReleaseIds))
                    .Select(BuildPublicationNode)
                    .OrderBy(publication => publication.Title)
                    .ToList()
            };
        }

        private static MethodologyTreeNode BuildPublicationNode(Publication publication)
        {
            return new MethodologyTreeNode
            {
                Id = publication.Id,
                Title = publication.Title,
                Summary = publication.Summary,
                Slug = publication.Slug,
                Methodology = BuildMethodology(publication.Methodology)
            };
        }

        private static MethodologySummaryViewModel BuildMethodology(Methodology methodology)
        {
            return new MethodologySummaryViewModel
            {
                Id = methodology.Id,
                Slug = methodology.Slug,
                Summary = methodology.Summary,
                Title = methodology.Title
            };
        }

        private static bool IsThemePublished(Theme theme, IEnumerable<Guid> includedReleaseIds)
        {
            return theme.Topics.Any(topic => IsTopicPublished(topic, includedReleaseIds));
        }

        private static bool IsTopicPublished(Topic topic, IEnumerable<Guid> includedReleaseIds)
        {
            return topic.Publications.Any(publication => IsPublicationPublished(publication, includedReleaseIds));
        }

        private static bool IsPublicationPublished(Publication publication, IEnumerable<Guid> includedReleaseIds)
        {
            return publication.MethodologyId != null &&
                   publication.Releases.Any(release => release.IsReleasePublished(includedReleaseIds));
        }
    }
}