#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseVersionRepository
{
    Task<List<ReleaseVersion>> ListReleases(params ReleaseApprovalStatus[] releaseStatuses);

    Task<List<ReleaseVersion>> ListReleasesForUser(Guid userId, params ReleaseApprovalStatus[] releaseApprovalStatuses);

    Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseVersionId);
}
