using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        public ReleaseViewModel GetRelease(string id)
        {
            var queryable = _context.Releases
                .Include(r => r.Content)
                .ThenInclude(section => section.Content)
                .Include(r => r.KeyStatistics)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Updates);

            var release = Guid.TryParse(id, out var newGuid) ? 
                queryable.FirstOrDefault(r => r.Id == newGuid) : 
                queryable.FirstOrDefault(r => r.Slug == id);

            if (release != null)
            {
                var releases = _context.Releases.Where(x => x.Publication.Id == release.Publication.Id).ToList();
                release.Publication.Releases = new List<Release>();
                releases.ForEach(r => release.Publication.Releases.Add(new Release
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
            releaseViewModel.Content.Sort((x, y) => x.Order.CompareTo(y.Order));
            releaseViewModel.LatestRelease = IsLatestRelease(release.PublicationId, releaseViewModel.Id);

            return releaseViewModel;
        }

        public ReleaseViewModel GetLatestRelease(string id)
        {
            var queryable = _context.Releases
                .Include(r => r.Content)
                .ThenInclude(section => section.Content)
                .Include(r => r.KeyStatistics)
                .Include(r => r.Publication).ThenInclude(p => p.LegacyReleases)
                .Include(r => r.Publication).ThenInclude(p => p.Topic.Theme)
                .Include(r => r.Publication).ThenInclude(p => p.Contact)
                .Include(r => r.Updates)
                .OrderBy(r => r.Published);

            var release = Guid.TryParse(id, out var newGuid) ? 
                queryable.Last(r => r.PublicationId == newGuid) : 
                queryable.Last(r => r.Publication.Slug == id);

            if (release != null)
            {
                var otherReleases = _context.Releases.Where(r => r.Publication.Id == release.Publication.Id
                                                                 && r.Id != release.Id).ToList();
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
                releaseViewModel.Content.Sort((x, y) => x.Order.CompareTo(y.Order));
                releaseViewModel.DataFiles = ListFiles(release, ReleaseFileTypes.Data);
                releaseViewModel.ChartFiles = ListFiles(release, ReleaseFileTypes.Chart);
                releaseViewModel.AncillaryFiles = ListFiles(release, ReleaseFileTypes.Ancillary);
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

        private List<FileInfo> ListFiles(Release release, ReleaseFileTypes type)
        {
            return _fileStorageService.ListFiles(release.Publication.Slug, release.Slug, type).ToList();
        }
    }
}