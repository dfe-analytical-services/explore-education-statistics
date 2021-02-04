using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseRepository
    {
        Task<List<MyReleaseViewModel>> GetAllReleasesForReleaseStatusesAsync(
            params ReleaseStatus[] releaseStatuses);

        Task<List<MyReleaseViewModel>> GetReleasesForReleaseStatusRelatedToUserAsync(Guid userId,
            params ReleaseStatus[] releaseStatuses);

        Task<Guid> CreateReleaseAndSubjectHierarchy(
            Guid releaseId,
            string subjectFilename,
            string subjectName);
    }
}
