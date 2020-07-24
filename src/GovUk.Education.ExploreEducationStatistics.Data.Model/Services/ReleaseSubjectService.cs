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
            var subjectIds = await _statisticsDbContext.ReleaseSubject
                .Include(rs => rs.Subject)
                .Where(rs => rs.ReleaseId == releaseId).Select(rs => rs.SubjectId).ToListAsync();
            
            foreach (var id in subjectIds)
            {
                await SoftDeleteSubjectOrBreakReleaseLink(releaseId, id);
            }
        }

        public async Task SoftDeleteSubjectOrBreakReleaseLink(Guid releaseId, Guid subjectId)
        {
            await RemoveReleaseSubjectLink(releaseId, subjectId);
            await _footnoteService.DeleteAllFootnotesBySubject(releaseId, subjectId);
            await SoftDeleteSubjectIfOrphaned(subjectId);

            await _statisticsDbContext.SaveChangesAsync();
        }

        private async Task RemoveReleaseSubjectLink(Guid releaseId, Guid subjectId)
        {
            var releaseSubject = await _statisticsDbContext
                .ReleaseSubject
                .FirstOrDefaultAsync(rs => rs.ReleaseId == releaseId && rs.SubjectId == subjectId);

            if (releaseSubject == null)
            {
                throw new ArgumentException(
                    $"ReleaseSubject not found while trying to remove it. Release: {releaseId}, Subject: {subjectId}");
            }

            _statisticsDbContext.ReleaseSubject.Remove(releaseSubject);
        }

        private async Task SoftDeleteSubjectIfOrphaned(Guid id)
        {
            var subject = await _statisticsDbContext.Subject
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
            {
                throw new ArgumentException($"Subject: {id} not found while trying to soft delete it", nameof(id));
            }

            await SoftDeleteSubjectIfOrphaned(subject);
        }

        private async Task SoftDeleteSubjectIfOrphaned(Subject subject)
        {
            if (await CountReleasesWithSubject(subject.Id) > 0)
            {
                return;
            }

            subject.SoftDeleted = true;
            _statisticsDbContext.Subject.Update(subject);
        }

        private async Task<int> CountReleasesWithSubject(Guid subjectId)
        {
            return await _statisticsDbContext.ReleaseSubject
                .CountAsync(rs => rs.SubjectId == subjectId);
        }
    }
}