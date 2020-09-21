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

        public async Task SoftDeleteAllSubjectsOrBreakReleaseLinks(Guid releaseId)
        {
            await DeleteAllSubjectsOrBreakReleaseLinks(releaseId, true);
        }

        public async Task SoftDeleteSubjectOrBreakReleaseLink(Guid releaseId, Guid subjectId)
        {
            await DeleteSubjectOrBreakReleaseLink(releaseId, subjectId, true);
        }

        public async Task DeleteAllSubjectsOrBreakReleaseLinks(Guid releaseId, bool isSoftDelete = false)
        {
            var subjectIds = await _statisticsDbContext.ReleaseSubject
                .Include(rs => rs.Subject)
                .Where(rs => rs.ReleaseId == releaseId).Select(rs => rs.SubjectId).ToListAsync();

            foreach (var id in subjectIds)
            {
                await DeleteSubjectOrBreakReleaseLink(releaseId, id, isSoftDelete);
            }
        }

        public async Task DeleteSubjectOrBreakReleaseLink(Guid releaseId, Guid subjectId, bool isSoftDelete = false)
        {
            await RemoveReleaseSubjectLinkIfExists(releaseId, subjectId);
            await _footnoteService.DeleteAllFootnotesBySubject(releaseId, subjectId);
            await DeleteSubjectIfOrphanedAndExists(subjectId, isSoftDelete);

            await _statisticsDbContext.SaveChangesAsync();
        }

        private async Task RemoveReleaseSubjectLinkIfExists(Guid releaseId, Guid subjectId)
        {
            var releaseSubject = await _statisticsDbContext
                .ReleaseSubject
                .FirstOrDefaultAsync(rs => rs.ReleaseId == releaseId && rs.SubjectId == subjectId);

            if (releaseSubject != null)
            {
                _statisticsDbContext.ReleaseSubject.Remove(releaseSubject);
            }
        }

        private async Task DeleteSubjectIfOrphanedAndExists(Guid subjectId, bool isSoftDelete)
        {
            var subject = await _statisticsDbContext.Subject
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (CanDeleteSubject(subject))
            {
                if (isSoftDelete)
                {
                    subject.SoftDeleted = true;
                    _statisticsDbContext.Subject.Update(subject);
                }
                else
                {
                    // N.B. This delete will be slow if there are a large number of observations but this is only
                    // executed by the tests when the topic is torn down so ensure files used are < 1000 rows.
                    var observations =  _statisticsDbContext.Observation
                        .Where(o => o.SubjectId == subjectId);
                    _statisticsDbContext.Observation.RemoveRange(observations);
                    _statisticsDbContext.Subject.Remove(subject);
                }
            }
        }

        private bool CanDeleteSubject(Subject subject)
        {
            return subject != null
                   && _statisticsDbContext.ReleaseSubject.Count(rs => rs.SubjectId == subject.Id) < 2;
        }
    }
}