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

        public ReleaseViewModel GetRelease(Guid id)
        {
            var release = _context.Releases
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Updates)
                .FirstOrDefault(r => r.Id == id);

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
            releaseViewModel.LatestRelease = IsLatestRelease(release.PublicationId, releaseViewModel.Id);
            releaseViewModel.DownloadFiles = _fileStorageService
                .ListPublicFiles(release.Publication.Slug, release.Slug).ToList();

            return releaseViewModel;
        }

        public ReleaseViewModel GetLatestRelease(Guid id)
        {
            var release = _context.Releases
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content)
                .Include(r => r.Publication).ThenInclude(p => p.LegacyReleases)
                .Include(r => r.Publication).ThenInclude(p => p.Topic.Theme)
                .Include(r => r.Publication).ThenInclude(p => p.Contact)
                .Include(r => r.Updates)
                .OrderBy(r => r.Published)
                .Last(r => r.PublicationId == id);

            if (release != null)
            {
                var otherReleases = _context.Releases
                    .Where(r => r.PublicationId == release.Publication.Id && r.Id != release.Id)
                    .ToList();

                release.Publication.Releases = new List<Release>();
                otherReleases.ForEach(r => release.Publication.Releases.Add(new Release
                {
                    Id = r.Id,
                    ReleaseName = r.ReleaseName,
                    Published = r.Published,
                    Slug = r.Slug,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));

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
                .FirstOrDefaultAsync(r => r.Id == id);

            if (release == null)
            {
                throw new ArgumentException("Release does not exist", nameof(id));
            }

            release.Published = DateTime.UtcNow;
            _context.Releases.Update(release);
            await _context.SaveChangesAsync();
        }

        // TODO: This logic is flawed but will provide an accurate result with the current seed data
        private bool IsLatestRelease(Guid publicationId, Guid releaseId)
        {
            return (_context.Releases
                        .Where(x => x.PublicationId == publicationId)
                        .OrderBy(x => x.Published)
                        .Last().Id == releaseId);
        }
    }
}