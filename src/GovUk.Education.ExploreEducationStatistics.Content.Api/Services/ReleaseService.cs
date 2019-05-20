using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;

        public ReleaseService(ApplicationDbContext context)
        {
            _context = context;
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
                releases.ForEach(r => release.Publication.Releases.Add(new Release()
                {
                    Id = r.Id,
                    Title = r.Title,
                    ReleaseName = r.ReleaseName,
                    Published = r.Published,
                    Slug = r.Slug,
                    Summary = r.Summary,
                    Publication = r.Publication,
                    PublicationId =  r.PublicationId,
                    Updates = r.Updates
                }));
            }

            return release;
        } 
         
        public Release GetLatestRelease(string id)
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
                releases.ForEach(r => release.Publication.Releases.Add(new Release()
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
    }
}