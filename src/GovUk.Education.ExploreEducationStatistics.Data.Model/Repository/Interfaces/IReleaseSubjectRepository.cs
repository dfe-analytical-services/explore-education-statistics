#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IReleaseSubjectRepository
{
    Task<IReadOnlyList<ReleaseSubject>> FindAll(
        Guid releaseVersionId,
        Func<IQueryable<ReleaseSubject>, IQueryable<ReleaseSubject>>? queryExtender = null);

    Task DeleteReleaseSubject(Guid releaseVersionId, Guid subjectId, bool softDeleteOrphanedSubject = true);

    Task DeleteAllReleaseSubjects(Guid releaseVersionId, bool softDeleteOrphanedSubjects = true);
}
