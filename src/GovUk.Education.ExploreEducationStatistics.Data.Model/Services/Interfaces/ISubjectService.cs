using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, Guid>
    {
        bool IsSubjectForLatestPublishedRelease(Guid subjectId);

        bool Exists(Guid releaseId, string name);

        Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, string name);

        Task<bool> RemoveReleaseSubjectLinkAsync(Guid releaseId, Guid subjectId);

        Task<Subject> GetAsync(Guid releaseId, string name);

        Task<List<Footnote>> GetFootnotesOnlyForSubjectAsync(Guid subjectId);

        Task<List<Subject>> GetSubjectsForReleaseAsync(Guid releaseId);
    }
}