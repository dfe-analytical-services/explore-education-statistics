#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseVersionRepository(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext,
    IUserPublicationRoleRepository userPublicationRoleRepository
) : IReleaseVersionRepository
{
    public async Task<List<ReleaseVersion>> ListReleases(params ReleaseApprovalStatus[] releaseApprovalStatuses)
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .Include(rv => rv.ReleaseStatuses)
            .Where(rv => releaseApprovalStatuses.Contains(rv.ApprovalStatus))
            .ToListAsync();
    }

    public async Task<List<ReleaseVersion>> ListReleasesForUser(
        Guid userId,
        params ReleaseApprovalStatus[] releaseApprovalStatuses
    )
    {
        var userPublicationRoleIds = await userPublicationRoleRepository
            .Query()
            .WhereForUser(userId)
            .Select(upr => upr.PublicationId)
            .Distinct()
            .ToListAsync();

        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .Include(rv => rv.ReleaseStatuses)
            .Where(rv => releaseApprovalStatuses.Contains(rv.ApprovalStatus))
            .Where(rv => userPublicationRoleIds.Contains(rv.Release.PublicationId))
            .ToListAsync();
    }

    public async Task<Guid> CreateStatisticsDbSubject()
    {
        var subject = new Subject();

        statisticsDbContext.Subject.Add(subject);
        await statisticsDbContext.SaveChangesAsync();

        return subject.Id;
    }
}
