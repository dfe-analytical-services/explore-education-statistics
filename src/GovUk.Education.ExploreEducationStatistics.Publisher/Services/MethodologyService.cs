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

        public MethodologyService(ContentDbContext context,
            IMethodologyVersionRepository methodologyVersionRepository)
        {
            _context = context;
            _methodologyVersionRepository = methodologyVersionRepository;
        }

        public async Task<MethodologyVersion> Get(Guid methodologyVersionId)
        {
            return await _context.MethodologyVersions.FindAsync(methodologyVersionId);
        }

        public async Task<List<MethodologyVersion>> GetLatestByRelease(Guid releaseId)
        {
            var release = await _context.Releases.FindAsync(releaseId);
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
            methodologyVersions.ForEach(methodology =>
            {
                methodology.Published ??= DateTime.UtcNow;
            });
            await _context.SaveChangesAsync();
        }
    }
}
