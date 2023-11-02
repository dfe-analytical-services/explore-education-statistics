#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface ISubjectRepository
    {
        Task<IList<Subject>> FindAll(
            IEnumerable<Guid> subjectIds,
            Func<IQueryable<Subject>, IQueryable<Subject>>? hydrateSubjectFunction = null);

        Task<Subject?> Find(Guid subjectId);

        Task<Guid?> FindPublicationIdForSubject(Guid subjectId);
    }
}
