using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                .Include(r => r.PreviousVersion)
                .SingleOrDefaultAsync(release => release.Id == id);
        }

        public async Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids)
        {
            return await _contentDbContext.Releases
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .ToListAsync();
        }

        public CachedReleaseViewModel GetReleaseViewModel(Guid id, PublishContext context)
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

            // If the release isn't live yet set the published date based on what we expect it to be
            releaseViewModel.Published ??= context.Published;

            return releaseViewModel;
        }

        public Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            return _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => !release.SoftDeleted && IsReleasePublished(release, includedReleaseIds) &&
                                  IsLatestVersionOfRelease(release.Publication, release.Id))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .LastOrDefault();
        }

        public CachedReleaseViewModel GetLatestReleaseViewModel(Guid publicationId,
            IEnumerable<Guid> includedReleaseIds, PublishContext context)
        {
            var latestRelease = GetLatestRelease(publicationId, includedReleaseIds);
            return GetReleaseViewModel(latestRelease.Id, context);
        }

        public async Task SetPublishedDatesAsync(Guid id, DateTime published)
        {
            var contentRelease = await _contentDbContext.Releases
                .Include(release => release.Publication)
                .ThenInclude(publication => publication.Methodology)
                .SingleOrDefaultAsync(r => r.Id == id);

            var statisticsRelease = await _statisticsDbContext.Release
                .SingleOrDefaultAsync(r => r.Id == id);

            var methodology = contentRelease.Publication.Methodology;

            if (contentRelease == null)
            {
                throw new ArgumentException("Content Release does not exist", nameof(id));
            }

            _contentDbContext.Releases.Update(contentRelease);
            contentRelease.Published ??= published;

            // TODO EES-913 Remove this when methodologies are published independently 
            if (methodology != null)
            {
                _contentDbContext.Methodologies.Update(methodology);
                methodology.Published ??= published;
            }

            await _contentDbContext.SaveChangesAsync();

            // The Release in the statistics database can be absent if no Subjects were created
            if (statisticsRelease != null)
            {
                _statisticsDbContext.Release.Update(statisticsRelease);
                statisticsRelease.Published ??= published;
                await _statisticsDbContext.SaveChangesAsync();
            }
        }
        
        public List<ReleaseFileReference> GetReleaseFileReferences(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return _contentDbContext
                .ReleaseFileReferences
                .Where(rfr => rfr.ReleaseId == releaseId && types.Contains(rfr.ReleaseFileType))
                .ToList();
        }

        private static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }

        private static bool IsLatestVersionOfRelease(Publication publication, Guid releaseId)
        {
            return !publication.Releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}