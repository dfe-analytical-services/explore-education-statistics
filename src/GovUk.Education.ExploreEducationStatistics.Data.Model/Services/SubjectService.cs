using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, Guid>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(StatisticsDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestRelease(Guid subjectId)
        {
            var subject = Find(subjectId, new List<Expression<Func<Subject, object>>> {s => s.Release});
            if (subject == null || !subject.Release.Live)
            {
                throw new ArgumentException("Subject does not exist", nameof(subjectId));
            }
            return _releaseService.GetLatestRelease(subject.Release.PublicationId).Equals(subject.ReleaseId);
        }
        
        public bool Exists(ReleaseId releaseId, string name)
        {
            return _context.Subject.Any(x => x.ReleaseId == releaseId && x.Name == name);
        }
        
        public async Task<bool> DeleteAsync(ReleaseId releaseId, string name)
        {
            var subject = await GetAsync(releaseId, name);
            if (subject != null)
            {
                _context.Subject.Remove(subject);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        
        public async Task<Subject> GetAsync(ReleaseId releaseId, string name)
        {
            return await _context.Subject.FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.Name == name);
        }
    }
}