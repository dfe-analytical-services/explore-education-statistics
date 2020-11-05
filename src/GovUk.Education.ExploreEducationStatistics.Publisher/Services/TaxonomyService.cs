using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class TaxonomyService : ITaxonomyService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IMapper _mapper;

        public TaxonomyService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _mapper = mapper;
        }

        public async Task SyncTaxonomy()
        {
            await SyncThemes();
            await _statisticsDbContext.SaveChangesAsync();

            await SyncTopics();
            await _statisticsDbContext.SaveChangesAsync();
        }

        private async Task SyncThemes()
        {
            var themes = await _contentDbContext.Themes
                .ToListAsync();

            var themeIds = themes.Select(theme => theme.Id).ToList();

            var statisticsThemes = await _statisticsDbContext.Theme
                .Where(theme => themeIds.Contains(theme.Id))
                .ToDictionaryAsync(theme => theme.Id);

            foreach (var theme in themes)
            {
                await SyncTheme(theme, statisticsThemes);
            }

            _statisticsDbContext.Theme.RemoveRange(
                _statisticsDbContext.Theme.Where(theme => !themeIds.Contains(theme.Id))
            );
        }

        private async Task SyncTheme(Theme theme, IReadOnlyDictionary<Guid, Data.Model.Theme> statisticsThemes)
        {
            var matchingStatsTheme = statisticsThemes.GetValueOrDefault(theme.Id);

            if (matchingStatsTheme == null)
            {
                var newStatsTheme = _mapper.Map(theme, new Data.Model.Theme());
                await _statisticsDbContext.Theme.AddAsync(newStatsTheme);
            }
            else
            {
                _mapper.Map(theme, matchingStatsTheme);
                _statisticsDbContext.Theme.Update(matchingStatsTheme);
            }
        }

        private async Task SyncTopics()
        {
            var topics = await _contentDbContext.Topics
                .ToListAsync();

            var topicIds = topics.Select(topic => topic.Id).ToList();

            var statisticsTopics = await _statisticsDbContext.Topic
                .Where(topic => topicIds.Contains(topic.Id))
                .ToDictionaryAsync(topic => topic.Id);

            foreach (var topic in topics)
            {
                await SyncTopic(topic, statisticsTopics);
            }

            _statisticsDbContext.Topic.RemoveRange(
                _statisticsDbContext.Topic.Where(topic => !topicIds.Contains(topic.Id))
            );
        }

        private async Task SyncTopic(Topic topic, IReadOnlyDictionary<Guid, Data.Model.Topic> statsTopics)
        {
            var matchingStatsTopic = statsTopics.GetValueOrDefault(topic.Id);

            if (matchingStatsTopic == null)
            {
                var newStatsTopic = _mapper.Map(topic, new Data.Model.Topic());
                await _statisticsDbContext.Topic.AddAsync(newStatsTopic);
            }
            else
            {
                _mapper.Map(topic, matchingStatsTopic);
                _statisticsDbContext.Topic.Update(matchingStatsTopic);
            }
        }
    }
}