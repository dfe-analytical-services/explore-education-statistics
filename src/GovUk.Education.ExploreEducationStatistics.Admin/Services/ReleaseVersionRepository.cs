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
        var userPublicationRolesQuery = userPublicationRoleRepository
            .Query()
            .WhereForUser(userId)
            .Select(upr => upr.PublicationId);

        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .Include(rv => rv.ReleaseStatuses)
            .Where(rv => releaseApprovalStatuses.Contains(rv.ApprovalStatus))
            .Where(rv => userPublicationRolesQuery.Contains(rv.Release.PublicationId))
            .ToListAsync();
    }

    public async Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseVersionId)
    {
        var existingStatsReleaseVersion = await statisticsDbContext.ReleaseVersion.FirstOrDefaultAsync(rv =>
            rv.Id == releaseVersionId
        );

        if (existingStatsReleaseVersion == null)
        {
            var releaseVersion = await contentDbContext.ReleaseVersions.FirstAsync(rv => rv.Id == releaseVersionId);

            statisticsDbContext.ReleaseVersion.Add(
                new Data.Model.ReleaseVersion { Id = releaseVersionId, PublicationId = releaseVersion.PublicationId }
            );
        }

        var releaseSubject = new ReleaseSubject { ReleaseVersionId = releaseVersionId, Subject = new Subject() };

        statisticsDbContext.ReleaseSubject.Add(releaseSubject);
        await statisticsDbContext.SaveChangesAsync();

        return releaseSubject.SubjectId;
    }
}
