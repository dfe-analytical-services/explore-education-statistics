#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ISubjectRepository : IRepository<Subject, Guid>
    {
        Task<bool> IsSubjectForLatestPublishedRelease(Guid subjectId);

        Task<Subject?> Get(Guid subjectId);

        Task<Guid> GetPublicationIdForSubject(Guid subjectId);
    }
}
