#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseVersionRepository _releaseVersionRepository;

        public ReleaseService(ContentDbContext contentDbContext,
            IReleaseVersionRepository releaseVersionRepository)
        {
            _contentDbContext = contentDbContext;
            _releaseVersionRepository = releaseVersionRepository;
        }

        public async Task<ReleaseVersion> Get(Guid releaseVersionId)
        {
            return await _contentDbContext.ReleaseVersions
                .SingleAsync(releaseVersion => releaseVersion.Id == releaseVersionId);
        }

        public async Task<IEnumerable<ReleaseVersion>> List(IEnumerable<Guid> releaseVersionIds)
        {
            return await _contentDbContext.ReleaseVersions
                .Where(rv => releaseVersionIds.Contains(rv.Id))
                .Include(rv => rv.Publication)
                .Include(rv => rv.PreviousVersion)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReleaseVersion>> GetAmendedReleases(IEnumerable<Guid> releaseVersionIds)
        {
            return await _contentDbContext.ReleaseVersions
                .Include(rv => rv.PreviousVersion)
                .Include(rv => rv.Publication)
                .Where(rv => releaseVersionIds.Contains(rv.Id) && rv.PreviousVersionId != null)
                .ToListAsync();
        }

        public async Task<ReleaseVersion> GetLatestReleaseVersion(Guid publicationId,
            IEnumerable<Guid> includedReleaseVersionIds)
        {
            var releases = await _contentDbContext.ReleaseVersions
                .Include(rv => rv.Publication)
                .Where(rv => rv.PublicationId == publicationId)
                .ToListAsync();

            return releases
                .Where(rv => rv.IsReleasePublished(includedReleaseVersionIds))
                .OrderBy(rv => rv.Year)
                .ThenBy(rv => rv.TimePeriodCoverage)
                .Last();
        }

        public async Task<List<File>> GetFiles(Guid releaseVersionId, params FileType[] types)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId)
                .Select(rf => rf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }

        public async Task CompletePublishing(Guid releaseVersionId, DateTime actualPublishedDate)
        {
            var releaseVersion = await _contentDbContext
                .ReleaseVersions
                .Include(rv => rv.DataBlockVersions)
                .ThenInclude(dataBlockVersion => dataBlockVersion.DataBlockParent)
                .SingleAsync(rv => rv.Id == releaseVersionId);

            _contentDbContext.ReleaseVersions.Update(releaseVersion);
            releaseVersion.Published =
                await _releaseVersionRepository.GetPublishedDate(releaseVersion.Id, actualPublishedDate);

            await UpdatePublishedDataBlockVersions(releaseVersion);

            await _contentDbContext.SaveChangesAsync();
        }

        private async Task UpdatePublishedDataBlockVersions(ReleaseVersion releaseVersion)
        {
            // Update all of the DataBlockParents to point their "LatestPublishedVersions" to the "latest" versions
            // on the Release. Mark the "latest" version as null until a Release Amendment is created.
            var latestDataBlockParents = releaseVersion
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
            if (releaseVersion.PreviousVersionId != null)
            {
                var latestDataBlockParentIds = latestDataBlockParents.Select(dataBlockParent => dataBlockParent.Id);

                var removedDataBlockVersions = await _contentDbContext
                    .DataBlockVersions
                    .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersion.PreviousVersionId &&
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
