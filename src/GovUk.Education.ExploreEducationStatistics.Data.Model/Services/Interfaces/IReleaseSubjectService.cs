using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseSubjectService
    {
        Task SoftDeleteAllSubjectsOrBreakReleaseLinks(Guid releaseId);
        
        Task SoftDeleteSubjectOrBreakReleaseLink(Guid releaseId, Guid subjectId);
    }
}