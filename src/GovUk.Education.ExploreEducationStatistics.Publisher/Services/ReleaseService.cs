#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Utils;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly PublicStatisticsDbContext _publicStatisticsDbContext;
        private readonly IMethodologyService _methodologyService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;

        public ReleaseService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            PublicStatisticsDbContext publicStatisticsDbContext,
            IMethodologyService methodologyService,
            IReleaseSubjectRepository releaseSubjectRepository)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _publicStatisticsDbContext = publicStatisticsDbContext;
            _methodologyService = methodologyService;
            _releaseSubjectRepository = releaseSubjectRepository;
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

        public async Task SetPublishedDates(Guid id, DateTime published)
        {
            var contentRelease = await _contentDbContext.Releases
                .Include(release => release.Publication)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (contentRelease == null)
            {
                throw new ArgumentException("Content Release does not exist", nameof(id));
            }

            if (contentRelease.Amendment)
            {
                var previousVersion = await _contentDbContext.Releases.AsNoTracking()
                    .SingleOrDefaultAsync(r => r.Id == contentRelease.PreviousVersionId);

                if (previousVersion?.Published == null)
                {
                    throw new ArgumentException("Previous version of release does not exist or is not live",
                        nameof(contentRelease.PreviousVersionId));
                }

                published = previousVersion.Published.Value;
            }

            _contentDbContext.Releases.Update(contentRelease);
            contentRelease.Published ??= published;
            contentRelease.DataLastPublished = DateTime.UtcNow;

            // Update the publication published date
            contentRelease.Publication.Published = published;

            // Set the published date on any methodologies used by this publication that are now publicly accessible
            // as a result of this release being published
            await _methodologyService.SetPublishedDatesByPublication(contentRelease.PublicationId, published);

            await _contentDbContext.SaveChangesAsync();

            var statisticsRelease = await _statisticsDbContext.Release
                .AsQueryable()
                .SingleOrDefaultAsync(r => r.Id == id);

            var publicStatisticsRelease = await _publicStatisticsDbContext.Release
                .AsQueryable()
                .SingleOrDefaultAsync(r => r.Id == id);

            // The Release in the statistics database can be absent if no data files were ever created
            if (statisticsRelease != null)
            {
                statisticsRelease.Published ??= published;
                _statisticsDbContext.Release.Update(statisticsRelease);
                await _statisticsDbContext.SaveChangesAsync();
            }

            if (publicStatisticsRelease != null)
            {
                publicStatisticsRelease.Published ??= published;
                _publicStatisticsDbContext.Release.Update(publicStatisticsRelease);
                await _publicStatisticsDbContext.SaveChangesAsync();
            }
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

        public async Task CreatePublicStatisticsRelease(Guid releaseId)
        {
            if (!EnvironmentUtils.IsLocalEnvironment())
            {
                var statisticsRelease = await _statisticsDbContext.Release
                    .AsQueryable()
                    .SingleOrDefaultAsync(r => r.Id == releaseId);

                var publicStatisticsRelease = await _publicStatisticsDbContext.Release
                    .AsQueryable()
                    .SingleOrDefaultAsync(r => r.Id == releaseId);

                if (statisticsRelease != null && publicStatisticsRelease == null)
                {
                    await _publicStatisticsDbContext.Release.AddAsync(new Data.Model.Release
                    {
                        Id = statisticsRelease.Id,
                        PublicationId = statisticsRelease.PublicationId,
                        Year = statisticsRelease.Year,
                        TimeIdentifier = statisticsRelease.TimeIdentifier,
                        Slug = statisticsRelease.Slug,
                        PreviousVersionId = statisticsRelease.PreviousVersionId
                        // Published date is omitted here as it will be set when publishing completes
                    });
                    await _publicStatisticsDbContext.SaveChangesAsync();
                }
            }
        }

        public async Task DeletePreviousVersionsStatisticalData(params Guid[] releaseIds)
        {
            var releases = await GetAmendedReleases(releaseIds);
            var previousVersions = releases.Select(r => r.PreviousVersionId)
                .Where(id => id.HasValue)
                .Cast<Guid>()
                .ToList();

            foreach (var previousVersion in previousVersions)
            {
                await _releaseSubjectRepository.SoftDeleteAllReleaseSubjects(previousVersion);
            }

            // Remove Statistical Releases for each of the Content Releases
            // TODO EES-2817 There's a missing foreign key on PreviousVersionId back to Release
            // so this removes the previous versions successfully but leaves PreviousVersionId's that won't exist
            await RemoveStatisticalReleases(previousVersions);

            await _publicStatisticsDbContext.SaveChangesAsync();
        }

        private async Task RemoveStatisticalReleases(IEnumerable<Guid> releaseIds)
        {
            var releases = await _publicStatisticsDbContext.Release
                .AsQueryable()
                .Where(r => releaseIds.Contains(r.Id))
                .ToListAsync();

            _publicStatisticsDbContext.Release.RemoveRange(releases);
        }
    }
}
