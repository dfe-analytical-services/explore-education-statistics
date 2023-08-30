#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IReleaseSubjectRepository
{
    Task DeleteReleaseSubject(Guid releaseId, Guid subjectId, bool softDeleteOrphanedSubject = true);

    Task DeleteAllReleaseSubjects(Guid releaseId, bool softDeleteOrphanedSubjects = true);
}
