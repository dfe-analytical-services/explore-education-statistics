#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;

        public ReleaseRepository(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
        }

        public async Task<List<Content.Model.ReleaseVersion>> ListReleases(
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            return await
                HydrateReleaseVersion(_contentDbContext.ReleaseVersions)
                    .Where(rv => releaseApprovalStatuses.Contains(rv.ApprovalStatus))
                    .ToListAsync();
        }

        public async Task<List<Content.Model.ReleaseVersion>> ListReleasesForUser(Guid userId,
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            var userReleaseVersionIds = await _contentDbContext
                .UserReleaseRoles
                .AsQueryable()
                .Where(r => r.UserId == userId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseVersionId)
                .ToListAsync();

            var userPublicationIds = await _contentDbContext
                .UserPublicationRoles
                .AsQueryable()
                .Where(r => r.UserId == userId)
                .Select(r => r.PublicationId)
                .ToListAsync();

            var userPublicationRoleReleaseIds = await _contentDbContext
                .ReleaseVersions
                .AsQueryable()
                .Where(rv => userPublicationIds.Contains(rv.PublicationId))
                .Select(rv => rv.Id)
                .ToListAsync();

            userReleaseVersionIds.AddRange(userPublicationRoleReleaseIds);
            userReleaseVersionIds = userReleaseVersionIds.Distinct().ToList();

            return await
                HydrateReleaseVersion(_contentDbContext.ReleaseVersions)
                    .Where(rv =>
                        userReleaseVersionIds.Contains(rv.Id) && releaseApprovalStatuses.Contains(rv.ApprovalStatus))
                    .ToListAsync();
        }

        public async Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseVersionId)
        {
            var releaseVersion = await _contentDbContext.ReleaseVersions
                .FirstAsync(rv => rv.Id == releaseVersionId);

            var existingStatsReleaseVersion = await _statisticsDbContext.ReleaseVersion
                .FirstOrDefaultAsync(rv => rv.Id == releaseVersionId);

            if (existingStatsReleaseVersion == null)
            {
                _statisticsDbContext.ReleaseVersion.Add(new Data.Model.ReleaseVersion
                {
                    Id = releaseVersionId,
                    PublicationId = releaseVersion.PublicationId
                });
            }

            var releaseSubject = new ReleaseSubject
            {
                ReleaseVersionId = releaseVersion.Id,
                Subject = new Subject()
            };

            _statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await _statisticsDbContext.SaveChangesAsync();

            return releaseSubject.SubjectId;
        }
    }
}
