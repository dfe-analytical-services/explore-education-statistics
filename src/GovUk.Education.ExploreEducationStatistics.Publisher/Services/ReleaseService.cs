using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NCrontab;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public ReleaseService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<Release> GetAsync(Guid id)
        {
            return await _contentDbContext.Releases
                .Include(release => release.Publication)
                .SingleOrDefaultAsync(release => release.Id == id);
        }

        public async Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids)
        {
            return await _contentDbContext.Releases
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .ToListAsync();
        }

        public CachedReleaseViewModel GetReleaseViewModel(Guid id)
        {
            var release = _contentDbContext.Releases
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication)
                .Include(r => r.Updates)
                .Single(r => r.Id == id);

            var releaseViewModel = _mapper.Map<CachedReleaseViewModel>(release);
            releaseViewModel.DownloadFiles =
                _fileStorageService.ListPublicFiles(release.Publication.Slug, release.Slug).ToList();

            if (!releaseViewModel.Published.HasValue)
            {
                // Release isn't live yet. Set the published date based on what we expect it to be
                releaseViewModel.Published = GetNextScheduledPublishingTime();
            }

            return releaseViewModel;
        }

        public Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            return _contentDbContext.Releases
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => IsReleasePublished(release, includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .LastOrDefault();
        }

        public CachedReleaseViewModel GetLatestReleaseViewModel(Guid publicationId,
            IEnumerable<Guid> includedReleaseIds)
        {
            var latestRelease = GetLatestRelease(publicationId, includedReleaseIds);
            return GetReleaseViewModel(latestRelease.Id);
        }

        public async Task SetPublishedDateAsync(Guid id)
        {
            var publishedDate = DateTime.UtcNow;
            
            var contentRelease = await _contentDbContext.Releases
                .SingleOrDefaultAsync(r => r.Id == id);

            var statisticsRelease = await _statisticsDbContext.Release
                .SingleOrDefaultAsync(r => r.Id == id);
            
            if (contentRelease == null)
            {
                throw new ArgumentException("Content Release does not exist", nameof(id));
            }
            
            _contentDbContext.Releases.Update(contentRelease);
            contentRelease.Published = publishedDate;
            await _contentDbContext.SaveChangesAsync();
            
            // The Release in the statistics database can be absent if no Subjects were created
            if (statisticsRelease != null)
            {
                _statisticsDbContext.Release.Update(statisticsRelease);
                statisticsRelease.Published = publishedDate;
                await _statisticsDbContext.SaveChangesAsync();
            }
        }

        private DateTime GetNextScheduledPublishingTime()
        {
            var publishReleasesCronSchedule = Environment.GetEnvironmentVariable("PublishReleaseContentCronSchedule");
            return TryParseCronSchedule(publishReleasesCronSchedule, out var cronSchedule)
                ? cronSchedule.GetNextOccurrence(DateTime.UtcNow)
                : DateTime.UtcNow;
        }

        private static bool TryParseCronSchedule(string cronExpression, out CrontabSchedule cronSchedule)
        {
            // ReSharper disable once IdentifierTypo
            cronSchedule = CrontabSchedule.TryParse(cronExpression, new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = CronExpressionHasSeconds(cronExpression)
            });
            return cronSchedule != null;
        }

        private static bool CronExpressionHasSeconds(string cronExpression)
        {
            return cronExpression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Length != 5;
        }

        private static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}