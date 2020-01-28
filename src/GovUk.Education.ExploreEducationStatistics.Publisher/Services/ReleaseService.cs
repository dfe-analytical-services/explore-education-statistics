using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Updates)
                .SingleOrDefault(r => r.Id == id);

            if (release != null)
            {
                var otherReleases = _context.Releases
                    .Where(r => r.PublicationId == release.Publication.Id && r.Id != release.Id)
                    .ToList();

                release.Publication.Releases = new List<Release>();
                otherReleases.ForEach(r => release.Publication.Releases.Add(
                    new Release
                    {
                        Id = r.Id,
                        ReleaseName = r.ReleaseName,
                        Published = r.Published,
                        Slug = r.Slug,
                        Publication = r.Publication,
                        PublicationId = r.PublicationId,
                        Updates = r.Updates
                    }));
            }

            var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
            releaseViewModel.Content.Sort((x, y) => x.Order.CompareTo(y.Order));
            releaseViewModel.LatestRelease =
                GetLatestRelease(release.PublicationId, Enumerable.Empty<Guid>())?.Id == releaseViewModel.Id;
            releaseViewModel.DownloadFiles = _fileStorageService
                .ListPublicFiles(release.Publication.Slug, release.Slug).ToList();

            return releaseViewModel;
        }

        public Release GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            return _context.Releases
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => IsReleasePublished(release, includedReleaseIds))
                .OrderByDescending(release => release.Published)
                .FirstOrDefault();
        }

        public ReleaseViewModel GetLatestReleaseViewModel(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            var latestRelease = GetLatestRelease(publicationId, includedReleaseIds);
            var release = _context.Releases
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication).ThenInclude(p => p.LegacyReleases)
                .Include(r => r.Publication).ThenInclude(p => p.Topic.Theme)
                .Include(r => r.Publication).ThenInclude(p => p.Contact)
                .Include(r => r.Updates)
                .SingleOrDefault(r => r.Id == latestRelease.Id);

            if (release != null)
            {
                var otherReleases = _context.Releases
                    .Where(r => r.PublicationId == release.Publication.Id && r.Id != release.Id)
                    .ToList()
                    .Where(r => IsReleasePublished(r, includedReleaseIds))
                    .Select(r => new Release
                    {
                        Id = r.Id,
                        ReleaseName = r.ReleaseName,
                        Published = r.Published,
                        Slug = r.Slug,
                        Publication = r.Publication,
                        PublicationId = r.PublicationId,
                        Updates = r.Updates
                    }).ToList();
                
                release.Publication.Releases = otherReleases;
                
                var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                releaseViewModel.Content.Sort((x, y) => x.Order.CompareTo(y.Order));
                releaseViewModel.DownloadFiles =
                    _fileStorageService.ListPublicFiles(release.Publication.Slug, release.Slug).ToList();
                releaseViewModel.LatestRelease = true;

                return releaseViewModel;
            }

            return null;
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