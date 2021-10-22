using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IReleaseSubjectRepository
    {
        Task SoftDeleteAllReleaseSubjects(Guid releaseId);

        Task SoftDeleteReleaseSubject(Guid releaseId, Guid subjectId);

        Task DeleteAllReleaseSubjects(Guid releaseId, bool softDeleteOrphanedSubjects = false);

        Task<Either<ActionResult, ReleaseSubject>> GetLatestPublishedReleaseSubjectForSubject(Guid subjectId);
    }
}