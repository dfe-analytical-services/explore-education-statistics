#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, Guid>
    {
        Task<bool> IsSubjectForLatestPublishedRelease(Guid subjectId);

        Task<Publication> GetPublicationForSubject(Guid subjectId);

        Task<Subject?> Get(Guid releaseId, string subjectName);

        Task<Subject?> Get(Guid subjectId);

        Task<List<Subject>> GetSubjectsForRelease(Guid releaseId);
    }
}