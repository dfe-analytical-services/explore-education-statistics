#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ISubjectRepository
    {
        Task<Subject?> Get(Guid subjectId);

        Task<Guid> GetPublicationIdForSubject(Guid subjectId);
    }
}
