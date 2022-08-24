using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseRepository
    {
        Task<List<Release>> ListReleases(
            params ReleaseApprovalStatus[] releaseStatuses);

        Task<List<Release>> ListReleasesForUser(Guid userId,
            params ReleaseApprovalStatus[] releaseApprovalStatuses);

        Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseId);

        public Task<List<Guid>> GetAllReleaseVersionIds(Release release);
    }
}
