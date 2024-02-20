#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseRepository
    {
        Task<List<ReleaseVersion>> ListReleases(
            params ReleaseApprovalStatus[] releaseStatuses);

        Task<List<ReleaseVersion>> ListReleasesForUser(Guid userId,
            params ReleaseApprovalStatus[] releaseApprovalStatuses);

        Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseVersionId);
    }
}
