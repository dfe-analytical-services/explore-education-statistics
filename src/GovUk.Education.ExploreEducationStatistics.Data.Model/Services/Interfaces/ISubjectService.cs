using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, Guid>
    {
        bool IsSubjectForLatestPublishedRelease(Guid subjectId);

        Task<Publication> GetPublicationForSubjectAsync(Guid subjectId);
        
        Task<Subject> GetAsync(Guid releaseId, string name);
        
        Task<Subject> GetAsync(Guid subjectId);

        Task<List<Subject>> GetSubjectsForReleaseAsync(Guid releaseId);
    }
}