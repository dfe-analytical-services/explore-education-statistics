#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseSubjectService : IReleaseSubjectService
    {
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFootnoteService _footnoteService;

        public ReleaseSubjectService(StatisticsDbContext statisticsDbContext, IFootnoteService footnoteService)
        {
            _statisticsDbContext = statisticsDbContext;
            _footnoteService = footnoteService;
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
            await _footnoteService.DeleteAllFootnotesBySubject(releaseId, subjectId);

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
                // Manually detach subject from state tracking to stop EF from
                // performing cascading deletes of child relationships in the wrong order.
                // By detaching it, we just let the database handle the cascading delete instead.
                // This is more efficient (fewer delete queries) and works correctly.
                _statisticsDbContext.Entry(subject).State = EntityState.Detached;

                // N.B. This delete will be slow if there are a large number of observations but this is only
                // executed by the tests when the topic is torn down so ensure files used are < 1000 rows.
                _statisticsDbContext.Subject.Remove(subject);
            }

            await _statisticsDbContext.SaveChangesAsync();
        }

        private bool IsSubjectOrphaned(Subject subject)
        {
            return _statisticsDbContext.ReleaseSubject.Count(rs => rs.SubjectId == subject.Id) == 0;
        }
    }
}