using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseSubjectService
    {
        Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId);
        
        Task<List<Footnote>> GetFootnotesOnlyForSubjectAsync(Guid subjectId);
    }
}