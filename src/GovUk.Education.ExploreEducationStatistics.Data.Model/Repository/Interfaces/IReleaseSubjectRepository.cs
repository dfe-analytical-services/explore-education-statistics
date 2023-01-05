#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IReleaseSubjectRepository
{
    Task SoftDeleteAllReleaseSubjects(Guid releaseId);

    Task SoftDeleteReleaseSubject(Guid releaseId, Guid subjectId);

    Task DeleteAllReleaseSubjects(Guid releaseId, bool softDeleteOrphanedSubjects = false);
}