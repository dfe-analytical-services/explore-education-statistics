using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public ReleaseService(ContentDbContext context, IFileStorageService fileStorageService, IMapper mapper)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<Release> GetAsync(Guid id)
        {
            return await _context.Releases
                .Include(release => release.Publication)
                .SingleOrDefaultAsync(release => release.Id == id);
        }

        public async Task<IEnumerable<Release>> GetAsync(IEnumerable<Guid> ids)
        {
            return await _context.Releases
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .ToListAsync();
        }

        public ReleaseViewModel GetReleaseViewModel(Guid id)
        {
            var release = _context.Releases
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(releaseContentSection => releaseContentSection.ContentSection)
                .ThenInclude(section => section.Content)
                .Include( r => r.Publication)
                .Include(r => r.Updates)
                .Single(r => r.Id == id);

            var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
            releaseViewModel.Content.Sort((x, y) => x.Order.CompareTo(y.Order));
            releaseViewModel.DownloadFiles =
                _fileStorageService.ListPublicFiles(release.Publication.Slug, release.Slug).ToList();
            return releaseViewModel;
        }

        public Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            return _context.Releases
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => IsReleasePublished(release, includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .LastOrDefault();
        }

        public ReleaseViewModel GetLatestReleaseViewModel(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            var latestRelease = GetLatestRelease(publicationId, includedReleaseIds);
            return GetReleaseViewModel(latestRelease.Id);
        }

        public async Task SetPublishedDateAsync(Guid id)
        {
            var release = await _context.Releases
                .SingleOrDefaultAsync(r => r.Id == id);

            if (release == null)
            {
                throw new ArgumentException("Release does not exist", nameof(id));
            }

            release.Published = DateTime.UtcNow;
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
        }

        private static bool IsReleasePublished(Release release, IEnumerable<Guid> includedReleaseIds)
        {
            return release.Live || includedReleaseIds.Contains(release.Id);
        }
    }
}