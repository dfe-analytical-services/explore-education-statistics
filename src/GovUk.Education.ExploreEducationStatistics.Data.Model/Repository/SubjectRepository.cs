#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly StatisticsDbContext _context;

        public SubjectRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<Subject?> Find(Guid subjectId)
        {
            return await _context
                .Subject
                .FindAsync(subjectId);
        }

        public async Task<Guid?> FindPublicationIdForSubject(Guid subjectId)
        {
            var releaseSubject = await _context.ReleaseSubject
                .Include(rs => rs.Release)
                .Where(rs => rs.SubjectId == subjectId)
                .FirstOrDefaultAsync();
            
            return releaseSubject?.Release.PublicationId;
        }
    }
}
