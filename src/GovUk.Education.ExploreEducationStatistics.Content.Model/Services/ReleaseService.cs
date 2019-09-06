using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services
{    
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public ReleaseService(ApplicationDbContext context, IFileStorageService fileStorageService, IMapper mapper)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public ReleaseViewModel GetRelease(Guid id)
        {
            var release = _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                .Include(x => x.Updates).FirstOrDefault(x => x.Id == id);

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
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));
            }

            var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
            releaseViewModel.LatestRelease = IsLatestRelease(release.PublicationId, releaseViewModel.Id);
            releaseViewModel.DownloadFiles = _fileStorageService.ListPublicFiles(release.Publication.Slug, release.Slug).ToList();

            return releaseViewModel;
        }

        public ReleaseViewModel GetLatestRelease(Guid id)
        {
            Release release;

            release = _context.Releases
                .Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                .Include(x => x.Publication).ThenInclude(p => p.Topic.Theme)
                .Include(x => x.Publication).ThenInclude(p => p.Contact)
                .Include(x => x.Updates).OrderBy(x => x.Published)
                .Last(t => t.PublicationId == id);

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
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));

                var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                releaseViewModel.DownloadFiles = _fileStorageService.ListPublicFiles(release.Publication.Slug, release.Slug).ToList();
                releaseViewModel.LatestRelease = true;

                return releaseViewModel;
            }

            return null;
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