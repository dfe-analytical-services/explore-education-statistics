using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public bool IsSubjectForLatestPublishedRelease(Guid subjectId)
        {
            var subject = Find(subjectId, new List<Expression<Func<Subject, object>>> {s => s.Release});
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(subjectId));
            }

            return _releaseService.GetLatestPublishedRelease(subject.Release.PublicationId).Equals(subject.ReleaseId);
        }

        public bool Exists(Guid releaseId, string name)
        {
            return _context.Subject.Any(x => x.ReleaseId == releaseId && x.Name == name);
        }

        public List<Footnote> GetFootnotesOnlyForSubject(Guid subjectId)
        {
            return _context
                .Footnote
                .Include(f => f.Subjects)
                .Where(f => f.Subjects.Count == 1 && f.Subjects.First().SubjectId == subjectId)
                .ToList();
        }

        public async Task<bool> DeleteAsync(Guid subjectId)
        {
            var subject = await _context
                .Subject
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject != null)
            {
                var orphanFootnotes = GetFootnotesOnlyForSubject(subjectId);

                _context.Subject.Remove(subject);
                _context.Footnote.RemoveRange(orphanFootnotes);

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(Guid releaseId, string name)
        {
            var subject = await GetAsync(releaseId, name);
            return await DeleteAsync(subject.Id);
        }

        public async Task<Subject> GetAsync(Guid releaseId, string name)
        {
            return await _context
                .Subject
                .FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.Name == name);
        }
    }
}