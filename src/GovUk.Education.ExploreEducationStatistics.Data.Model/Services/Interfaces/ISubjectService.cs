using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, Guid>
    {
        bool IsSubjectForLatestRelease(Guid subjectId);

        bool Exists(Guid releaseId, string name);

        Task<bool> DeleteAsync(Guid releaseId, string name);

        Task<bool> DeleteAsync(Guid subjectId);

        Task<Subject> GetAsync(Guid releaseId, string name);

        List<Footnote> GetFootnotesOnlyForSubject(Guid subjectId);
    }
}