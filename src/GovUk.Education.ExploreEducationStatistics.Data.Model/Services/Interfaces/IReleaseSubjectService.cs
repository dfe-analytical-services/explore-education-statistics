using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseSubjectService
    {
        Task<bool> SoftDeleteSubjectOrBreakReleaseLinkAsync(Guid releaseId, Guid subjectId);
    }
}