#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseRepository _releaseRepository;

        public ReleaseService(ContentDbContext contentDbContext,
            IReleaseRepository releaseRepository)
        {
            _contentDbContext = contentDbContext;
            _releaseRepository = releaseRepository;
        }

        public async Task<Release> Get(Guid id)
        {
            return await _contentDbContext.Releases
                .SingleAsync(release => release.Id == id);
        }

        public async Task<IEnumerable<Release>> List(IEnumerable<Guid> ids)
        {
            return await _contentDbContext.Releases
                .AsQueryable()
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .Include(release => release.PreviousVersion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetAmendedReleases(IEnumerable<Guid> releaseIds)
        {
            return await _contentDbContext.Releases
                .Include(r => r.PreviousVersion)
                .Include(r => r.Publication)
                .Where(r => releaseIds.Contains(r.Id) && r.PreviousVersionId != null)
                .ToListAsync();
        }

        public async Task<Release> GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            var releases = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToListAsync();

            return releases
                .Where(release => release.IsReleasePublished(includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .Last();
        }

        public async Task<List<File>> GetFiles(Guid releaseId, params FileType[] types)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseId == releaseId)
                .Select(rf => rf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }

        public async Task CompletePublishing(Guid releaseId, DateTime actualPublishedDate)
        {
            var release = await _contentDbContext
                .Releases
                .Include(release => release.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .SingleAsync(r => r.Id == releaseId);

            _contentDbContext.Releases.Update(release);
            release.Published = await _releaseRepository.GetPublishedDate(release.Id, actualPublishedDate);

            await UpdatePublishedDataBlockVersions(release);

            await _contentDbContext.SaveChangesAsync();
        }

        private async Task UpdatePublishedDataBlockVersions(Release release)
        {
            // Update all of the DataBlockParents to point their "LatestPublishedVersions" to the "latest" versions
            // on the Release. Mark the "latest" version as null until a Release Amendment is created.
            var latestDataBlockParents = release
                .DataBlockVersions
                .Select(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .ToList();

            latestDataBlockParents.ForEach(latestDataBlockParent =>
            {
                // Promote the LatestDraftVersion as the newly published version.
                latestDataBlockParent.LatestPublishedVersion = latestDataBlockParent.LatestDraftVersion;
                latestDataBlockParent.LatestPublishedVersionId = latestDataBlockParent.LatestDraftVersionId;
                latestDataBlockParent.LatestPublishedVersion!.Published = DateTime.UtcNow;

                // Remove the LatestDraftVersion completely until such a point in the future as a Release Amendment is
                // created that will contain this Data Block as a Draft.
                latestDataBlockParent.LatestDraftVersionId = null;
            });

            // Find all DataBlockVersions that were part of the previously published Release, if any, and update them
            // to no longer have a Published version.
            if (release.PreviousVersionId != null)
            {
                var latestDataBlockParentIds = latestDataBlockParents.Select(dataBlockParent => dataBlockParent.Id);

                var removedDataBlockVersions = await _contentDbContext
                    .DataBlockVersions
                    .Where(dataBlockVersion => dataBlockVersion.ReleaseId == release.PreviousVersionId &&
                                               !latestDataBlockParentIds.Contains(dataBlockVersion.DataBlockParentId))
                    .Include(dataBlockVersion => dataBlockVersion.DataBlockParent)
                    .ToListAsync();

                // Update all of the removed DataBlockVersions' DataBlockParents to point their
                // "LatestPublishedVersions" to null, as they should no longer be publicly accessible.
                removedDataBlockVersions.ForEach(latestDataBlockVersion =>
                {
                    var parent = latestDataBlockVersion.DataBlockParent;
                    parent.LatestPublishedVersionId = null;
                });
            }
        }
    }
}
