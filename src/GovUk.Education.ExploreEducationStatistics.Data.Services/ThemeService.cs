using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.EntityFrameworkCore;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMapper _mapper;

        public ThemeService(ContentDbContext contentDbContext,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _mapper = mapper;
        }

        public IEnumerable<ThemeViewModel> ListThemes()
        {
            return _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .Where(IsThemePublished)
                .Select(BuildThemeViewModel)
                .OrderBy(theme => theme.Title);
        }

        private ThemeViewModel BuildThemeViewModel(Theme theme)
        {
            var viewModel = _mapper.Map<ThemeViewModel>(theme);
            viewModel.Topics = theme.Topics
                .Where(IsTopicPublished)
                .Select(BuildTopicViewModel)
                .OrderBy(publication => publication.Title);
            return viewModel;
        }

        private TopicViewModel BuildTopicViewModel(Topic topic)
        {
            var viewModel = _mapper.Map<TopicViewModel>(topic);
            viewModel.Publications = topic.Publications
                .Where(IsPublicationPublished)
                .Select(BuildPublicationViewModel)
                .OrderBy(publication => publication.Title);
            return viewModel;
        }

        private TopicPublicationViewModel BuildPublicationViewModel(Publication publication)
        {
            return _mapper.Map<TopicPublicationViewModel>(publication);
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