using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseSubjectService
    {
        Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId);
    }
}