using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
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

        public Release GetRelease(string id)
        {
            var release = Guid.TryParse(id, out var newGuid)
                ? _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                    .Include(x => x.Updates).FirstOrDefault(x => x.Id == newGuid)
                : _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                    .Include(x => x.Updates).FirstOrDefault(x => x.Slug == id);

            if (release != null)
            {
                var releases = _context.Releases.Where(x => x.Publication.Id == release.Publication.Id).ToList();
                release.Publication.Releases = new List<Release>();
                releases.ForEach(r => release.Publication.Releases.Add(new Release
                {
                    Id = r.Id,
                    Title = r.Title,
                    ReleaseName = r.ReleaseName,
                    Published = r.Published,
                    Slug = r.Slug,
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));
            }

            return release;
        }

        public ReleaseViewModel GetLatestRelease(string id)
        {
            Release release;

            if (Guid.TryParse(id, out var newGuid))
            {
                release = _context.Releases.Include(x => x.Updates).Include(x => x.Publication)
                    .ThenInclude(x => x.LegacyReleases).Include(x => x.Updates).OrderBy(x => x.Published)
                    .Last(t => t.PublicationId == newGuid);
            }
            else
            {
                release = _context.Releases.Include(x => x.Publication).ThenInclude(x => x.LegacyReleases)
                    .Include(x => x.Updates).OrderBy(x => x.Published).Last(t => t.Publication.Slug == id);
            }


            if (release != null)
            {
                var releases = _context.Releases.Where(x => x.Publication.Id == release.Publication.Id).ToList();
                release.Publication.Releases = new List<Release>();
                releases.ForEach(r => release.Publication.Releases.Add(new Release
                {
                    Id = r.Id,
                    Title = r.Title,
                    ReleaseName = r.ReleaseName,
                    Published = r.Published,
                    Slug = r.Slug,
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId = r.PublicationId,
                    Updates = r.Updates
                }));
                
                var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                releaseViewModel.DataFiles = ListFiles(release);
                return releaseViewModel;
            }

            return null;
        }

        private List<FileInfo> ListFiles(Release release)
        {
            return _fileStorageService.ListFiles(release.Publication.Slug, release.Slug).ToList();
        }
    }
}