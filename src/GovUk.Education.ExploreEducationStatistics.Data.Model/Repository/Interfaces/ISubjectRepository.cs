#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ISubjectRepository
    {
        Task<Subject?> Find(Guid subjectId);

        Task<Guid?> FindPublicationIdForSubject(Guid subjectId);
    }
}
