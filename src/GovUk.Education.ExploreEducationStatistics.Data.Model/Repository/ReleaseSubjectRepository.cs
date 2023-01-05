#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class ReleaseSubjectRepository : IReleaseSubjectRepository
    {
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly SubjectDeleter _subjectDeleter;

        public ReleaseSubjectRepository(
            StatisticsDbContext statisticsDbContext,
            IFootnoteRepository footnoteRepository,
            SubjectDeleter? subjectDeleter = null)
        {
            _statisticsDbContext = statisticsDbContext;
            _footnoteRepository = footnoteRepository;
            _subjectDeleter = subjectDeleter ?? new SubjectDeleter();
        }

        public async Task SoftDeleteAllReleaseSubjects(Guid releaseId)
        {
            await DeleteAllReleaseSubjects(releaseId, true);
        }

        public async Task SoftDeleteReleaseSubject(Guid releaseId, Guid subjectId)
        {
            await DeleteReleaseSubject(releaseId, subjectId, true);
        }

        public async Task DeleteAllReleaseSubjects(Guid releaseId, bool softDeleteOrphanedSubjects = false)
        {
            var subjectIds = await _statisticsDbContext.ReleaseSubject
                .Include(rs => rs.Subject)
                .Where(rs => rs.ReleaseId == releaseId)
                .Select(rs => rs.SubjectId)
                .ToListAsync();

            foreach (var id in subjectIds)
            {
                await DeleteReleaseSubject(releaseId, id, softDeleteOrphanedSubjects);
            }
        }

        public async Task DeleteReleaseSubject(Guid releaseId, Guid subjectId, bool softDeleteOrphanedSubject = false)
        {
            var releaseSubject = await _statisticsDbContext
                .ReleaseSubject
                .Include(rs => rs.Subject)
                .FirstOrDefaultAsync(rs => rs.ReleaseId == releaseId && rs.SubjectId == subjectId);

            await DeleteReleaseSubjectIfExists(releaseSubject);
            await _footnoteRepository.DeleteFootnotesBySubject(releaseId, subjectId);

            if (releaseSubject?.Subject != null)
            {
                await DeleteSubjectIfOrphaned(releaseSubject.Subject, softDeleteOrphanedSubject);
            }
        }

        private async Task DeleteReleaseSubjectIfExists(ReleaseSubject? releaseSubject)
        {
            if (releaseSubject != null)
            {
                _statisticsDbContext.ReleaseSubject.Remove(releaseSubject);
                // Immediately save context as we will need to re-check
                // how many ReleaseSubjects are attached to the Subject
                // later when determining if the Subject has been orphaned.
                await _statisticsDbContext.SaveChangesAsync();
            }
        }

        private async Task DeleteSubjectIfOrphaned(Subject subject, bool isSoftDelete)
        {
            if (!IsSubjectOrphaned(subject))
            {
                return;
            }

            if (isSoftDelete)
            {
                subject.SoftDeleted = true;
                _statisticsDbContext.Subject.Update(subject);
            }
            else
            {
                _subjectDeleter.Delete(subject, _statisticsDbContext);
            }

            await _statisticsDbContext.SaveChangesAsync();
        }

        private bool IsSubjectOrphaned(Subject subject)
        {
            return _statisticsDbContext.ReleaseSubject.Count(rs => rs.SubjectId == subject.Id) == 0;
        }

        // Separate this into a separate class
        // for ease of mocking in tests.
        public class SubjectDeleter
        {
            public virtual void Delete(Subject subject, StatisticsDbContext context)
            {
                // Use raw delete as EF can't correctly figure out how to cascade the delete, whilst the database can.
                // N.B. This delete will be slow if there are a large number of observations but this is only
                // executed by the tests when the topic is torn down so ensure files used are < 1000 rows.
                context.Subject.FromSqlInterpolated($"DELETE Subject WHERE Id = {subject.Id}");
            }
        }
    }
}
