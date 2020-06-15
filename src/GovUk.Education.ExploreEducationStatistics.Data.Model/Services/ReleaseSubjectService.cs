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

        public async Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId)
        {
            var releaseSubjectLinkToRemove = await _statisticsDbContext
                .ReleaseSubject
                .FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.SubjectId == subjectId);

            if (releaseSubjectLinkToRemove == null)
            {
                return false;
            }
            
            var numberOfReleaseSubjectLinks = _statisticsDbContext
                .ReleaseSubject
                .Count(s => s.SubjectId == subjectId);

            _statisticsDbContext.ReleaseSubject.Remove(releaseSubjectLinkToRemove);
            
            await _footnoteService.DeleteFootnotes(releaseId, subjectId);
            
            if (numberOfReleaseSubjectLinks == 1)
            {
                var subject = await _statisticsDbContext
                    .Subject
                    .FirstOrDefaultAsync(s => s.Id == subjectId);
            
                if (subject != null)
                {
                    var observationFilterItems = _statisticsDbContext.ObservationFilterItem
                            .Include(ofi => ofi.Observation)
                            .ThenInclude(o => o.Subject)
                            .Where(ofi => ofi.Observation.Subject.Id == subjectId);
                    
                    _statisticsDbContext.ObservationFilterItem.RemoveRange(observationFilterItems);

                    _statisticsDbContext.Subject.Remove(subject);
                }    
            }
        
            await _statisticsDbContext.SaveChangesAsync();
            return true;
        }
    }
}