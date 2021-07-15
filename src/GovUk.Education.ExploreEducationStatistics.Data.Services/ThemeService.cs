using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.EntityFrameworkCore;
using FileType = GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;

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

        public IEnumerable<ThemeViewModel> ListThemesWithLiveSubjects()
        {
            return _contentDbContext.Themes
                .Include(theme => theme.Topics)
                .ThenInclude(topic => topic.Publications)
                .ThenInclude(publication => publication.Releases)
                .ToList()
                .Where(IsThemePublishedWithLiveSubjects)
                .Select(BuildThemeViewModel)
                .OrderBy(theme => theme.Title);
        }

        private ThemeViewModel BuildThemeViewModel(Theme theme)
        {
            var viewModel = _mapper.Map<ThemeViewModel>(theme);
            viewModel.Topics = theme.Topics
                .Where(IsTopicPublishedWithLiveSubjects)
                .Select(BuildTopicViewModel)
                .OrderBy(topic => topic.Title);
            return viewModel;
        }

        private TopicViewModel BuildTopicViewModel(Topic topic)
        {
            var viewModel = _mapper.Map<TopicViewModel>(topic);
            viewModel.Publications = topic.Publications
                .Where(IsPublicationPublishedWithLiveSubjects)
                .Select(BuildPublicationViewModel)
                .OrderBy(publication => publication.Title);
            return viewModel;
        }

        private TopicPublicationViewModel BuildPublicationViewModel(Publication publication)
        {
            return _mapper.Map<TopicPublicationViewModel>(publication);
        }

        private bool IsThemePublishedWithLiveSubjects(Theme theme)
        {
            return theme.Topics?.Any(IsTopicPublishedWithLiveSubjects) ?? false;
        }

        private bool IsTopicPublishedWithLiveSubjects(Topic topic)
        {
            return topic.Publications?.Any(IsPublicationPublishedWithLiveSubjects) ?? false;
        }

        private bool IsPublicationPublishedWithLiveSubjects(Publication publication)
        {
            var latestLiveRelease = publication.LatestPublishedRelease();
            return latestLiveRelease != null && _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Any(rf =>
                    rf.ReleaseId == latestLiveRelease.Id
                    && rf.File.Type == FileType.Data);
        }
    }
}
