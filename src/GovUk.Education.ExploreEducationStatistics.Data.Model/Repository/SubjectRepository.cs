#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class SubjectRepository : AbstractRepository<Subject, Guid>, ISubjectRepository
    {
        private readonly IReleaseRepository _releaseRepository;

        public SubjectRepository(
            StatisticsDbContext context,
            IReleaseRepository releaseRepository) : base(context)
        {
            _releaseRepository = releaseRepository;
        }

        public async Task<bool> IsSubjectForLatestPublishedRelease(Guid subjectId)
        {
            var publicationId = await GetPublicationIdForSubject(subjectId);
            var latestRelease = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (latestRelease == null)
            {
                return false;
            }

            return _context
                .ReleaseSubject
                .Any(r => r.ReleaseId == latestRelease.Id && r.SubjectId == subjectId);
        }

        public async Task<Subject?> Get(Guid subjectId)
        {
            return await _context
                .Subject
                .FindAsync(subjectId);
        }

        public async Task<Guid> GetPublicationIdForSubject(Guid subjectId)
        {
            var firstReleaseSubject = await _context.ReleaseSubject
                .Include(rs => rs.Release)
                .Where(rs => rs.SubjectId == subjectId)
                .FirstAsync();
            return firstReleaseSubject.Release.PublicationId;
        }
    }
}
