#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleaseVersionRepository : IReleaseVersionRepository
{
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;

    public ReleaseVersionRepository(ContentDbContext contentDbContext, StatisticsDbContext statisticsDbContext)
    {
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
    }

    public async Task<List<ReleaseVersion>> ListReleases(params ReleaseApprovalStatus[] releaseApprovalStatuses)
    {
        return await _contentDbContext
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
        var releaseRoleReleaseVersionIds = await _contentDbContext
            .UserReleaseRoles.Where(urr => urr.UserId == userId)
            .Where(urr => urr.Role != ReleaseRole.PrereleaseViewer)
            .Select(r => r.ReleaseVersionId)
            .ToListAsync();

        var publicationRolePublicationIds = await _contentDbContext
            .UserPublicationRoles.Where(upr => upr.UserId == userId)
            .Select(upr => upr.PublicationId)
            .ToListAsync();

        var publicationRoleReleaseVersionIds = await _contentDbContext
            .ReleaseVersions.Where(rv => publicationRolePublicationIds.Contains(rv.Release.PublicationId))
            .Select(rv => rv.Id)
            .ToListAsync();

        var allReleaseVersionIds = releaseRoleReleaseVersionIds
            .Concat(publicationRoleReleaseVersionIds)
            .Distinct()
            .ToList();

        return await _contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .Include(rv => rv.ReleaseStatuses)
            .Where(rv => releaseApprovalStatuses.Contains(rv.ApprovalStatus))
            .Where(rv => allReleaseVersionIds.Contains(rv.Id))
            .ToListAsync();
    }

    public async Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseVersionId)
    {
        var existingStatsReleaseVersion = await _statisticsDbContext.ReleaseVersion.FirstOrDefaultAsync(rv =>
            rv.Id == releaseVersionId
        );

        if (existingStatsReleaseVersion == null)
        {
            var releaseVersion = await _contentDbContext.ReleaseVersions.FirstAsync(rv => rv.Id == releaseVersionId);

            _statisticsDbContext.ReleaseVersion.Add(
                new Data.Model.ReleaseVersion { Id = releaseVersionId, PublicationId = releaseVersion.PublicationId }
            );
        }

        var releaseSubject = new ReleaseSubject { ReleaseVersionId = releaseVersionId, Subject = new Subject() };

        _statisticsDbContext.ReleaseSubject.Add(releaseSubject);
        await _statisticsDbContext.SaveChangesAsync();

        return releaseSubject.SubjectId;
    }
}
