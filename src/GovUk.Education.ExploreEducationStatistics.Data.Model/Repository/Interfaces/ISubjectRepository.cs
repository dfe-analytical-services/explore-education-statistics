#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface ISubjectRepository
{
    Task<Guid?> FindPublicationIdForSubject(Guid subjectId,
        CancellationToken cancellationToken = default);
}
