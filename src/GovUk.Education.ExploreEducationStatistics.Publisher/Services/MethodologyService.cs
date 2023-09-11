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

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly ContentDbContext _context;
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IPublicationRepository _publicationRepository;

        public MethodologyService(
            ContentDbContext context,
            IMethodologyVersionRepository methodologyVersionRepository,
            IPublicationRepository publicationRepository)
        {
            _context = context;
            _methodologyVersionRepository = methodologyVersionRepository;
            _publicationRepository = publicationRepository;
        }

        public async Task<MethodologyVersion> Get(Guid methodologyVersionId)
        {
            return await _context.MethodologyVersions.FindAsync(methodologyVersionId);
        }

        public async Task<List<MethodologyVersion>> GetLatestVersionByRelease(Release release)
        {
            return await _methodologyVersionRepository.GetLatestVersionByPublication(release.PublicationId);
        }

        public async Task<List<File>> GetFiles(Guid methodologyVersionId, params FileType[] types)
        {
            return await _context.MethodologyFiles
                .Include(mf => mf.File)
                .Where(mf => mf.MethodologyVersionId == methodologyVersionId)
                .Select(mf => mf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }

        public async Task SetPublishedDatesIfApplicable(Guid publicationId)
        {
            var methodologyVersions =
                await _methodologyVersionRepository.GetLatestPublishedVersionByPublication(publicationId);

            _context.MethodologyVersions.UpdateRange(methodologyVersions);
            methodologyVersions.ForEach(methodologyVersion =>
            {
                methodologyVersion.Published ??= DateTime.UtcNow;
            });
            await _context.SaveChangesAsync();
        }

        public async Task Publish(MethodologyVersion methodologyVersion)
        {
            // NOTE: Methodology files are published separately

            await _context.Entry(methodologyVersion)
                .Reference(mv => mv.Methodology)
                .LoadAsync();

            methodologyVersion.Methodology.LatestPublishedVersionId = methodologyVersion.Id;
            methodologyVersion.Published = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBeingPublishedAlongsideRelease(MethodologyVersion methodologyVersion, Release release)
        {
            if (!methodologyVersion.Approved)
            {
                return false;
            }

            var firstRelease = !await _publicationRepository.IsPublished(release.PublicationId);

            var firstReleaseAndMethodologyScheduledImmediately =
                firstRelease &&
                methodologyVersion.ScheduledForPublishingImmediately;

            var methodologyScheduledWithThisRelease =
                methodologyVersion.ScheduledForPublishingWithRelease
                && methodologyVersion.ScheduledWithReleaseId == release.Id;

            return firstReleaseAndMethodologyScheduledImmediately ||
                   methodologyScheduledWithThisRelease;
        }
    }
}
