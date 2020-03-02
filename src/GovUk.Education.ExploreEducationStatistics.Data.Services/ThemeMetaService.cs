using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ThemeMetaService : IThemeMetaService
    {
        private StatisticsDbContext _context;
        private readonly IMapper _mapper;

        public ThemeMetaService(StatisticsDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<ThemeMetaViewModel> GetThemes()
        {
            return _context.Theme
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .Where(IsThemePublished)
                .Select(BuildThemeMetaViewModel)
                .OrderBy(theme => theme.Title);
        }

        private ThemeMetaViewModel BuildThemeMetaViewModel(Theme theme)
        {
            var viewModel = _mapper.Map<ThemeMetaViewModel>(theme);
            viewModel.Topics = theme.Topics
                .Where(IsTopicPublished)
                .Select(BuildTopicMetaViewModel)
                .OrderBy(publication => publication.Title);
            return viewModel;
        }

        private TopicMetaViewModel BuildTopicMetaViewModel(Topic topic)
        {
            var viewModel = _mapper.Map<TopicMetaViewModel>(topic);
            viewModel.Publications = topic.Publications
                .Where(IsPublicationPublished)
                .Select(BuildPublicationMetaViewModel)
                .OrderBy(publication => publication.Title);
            return viewModel;
        }

        private PublicationMetaViewModel BuildPublicationMetaViewModel(Publication publication)
        {
            return _mapper.Map<PublicationMetaViewModel>(publication);
        }

        private static bool IsThemePublished(Theme theme)
        {
            return theme.Topics?.Any(IsTopicPublished) ?? false;
        }

        private static bool IsTopicPublished(Topic topic)
        {
            return topic.Publications?.Any(IsPublicationPublished) ?? false;
        }

        private static bool IsPublicationPublished(Publication publication)
        {
            return publication.Releases?.Any(IsReleasePublished) ?? false;
        }

        private static bool IsReleasePublished(Release release)
        {
            return release.Live;
        }
    }
}