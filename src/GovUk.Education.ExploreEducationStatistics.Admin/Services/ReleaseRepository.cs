#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public ReleaseRepository(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _mapper = mapper;
        }

        public async Task<List<Content.Model.Release>> ListReleases(
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            return await
                HydrateRelease(_contentDbContext.Releases)
                .Where(r => releaseApprovalStatuses.Contains(r.ApprovalStatus))
                .ToListAsync();
        }

        public async Task<List<Content.Model.Release>> ListReleasesForUser(Guid userId,
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            var userReleaseIds = await _contentDbContext
                .UserReleaseRoles
                .AsQueryable()
                .Where(r => r.UserId == userId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseId)
                .ToListAsync();

            var userPublicationIds = await _contentDbContext
                .UserPublicationRoles
                .AsQueryable()
                .Where(r => r.UserId == userId)
                .Select(r => r.PublicationId)
                .ToListAsync();
            var userPublicationRoleReleaseIds = await _contentDbContext
                .Releases
                .AsQueryable()
                .Where(r => userPublicationIds.Contains(r.PublicationId))
                .Select(r => r.Id)
                .ToListAsync();
            userReleaseIds.AddRange(userPublicationRoleReleaseIds);
            userReleaseIds = userReleaseIds.Distinct().ToList();

            return await
                HydrateRelease(_contentDbContext.Releases)
                .Where(r => userReleaseIds.Contains(r.Id) && releaseApprovalStatuses.Contains(r.ApprovalStatus))
                .ToListAsync();
        }

        public async Task<Guid> CreateStatisticsDbReleaseAndSubjectHierarchy(Guid releaseId)
        {
            var release = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .FirstOrDefaultAsync(r => r.Id == releaseId);

            var existingRelease =
                await _statisticsDbContext.Release.FindAsync(release.Id);
            if (existingRelease == null)
            {
                var statsRelease = _mapper.Map(release, new Data.Model.Release());
                await _statisticsDbContext.Release.AddAsync(statsRelease);
            }
            else
            {
                _mapper.Map(release, existingRelease);
                _statisticsDbContext.Release.Update(existingRelease);
            }

            var releaseSubject = (await _statisticsDbContext.ReleaseSubject.AddAsync(new ReleaseSubject
            {
                ReleaseId = release.Id,
                Subject = new Subject()
            })).Entity;

            await _statisticsDbContext.SaveChangesAsync();
            return releaseSubject.Subject.Id;
        }

        public async Task<List<Guid>> GetAllReleaseVersionIds(Content.Model.Release release)
        {
            var releaseIdList = new List<Guid> { release.Id };
            var currentRelease = release;

            while (currentRelease.PreviousVersionId != null)
            {
                currentRelease = await _contentDbContext.Releases
                    .AsQueryable()
                    .SingleAsync(r => r.Id == currentRelease.PreviousVersionId);
                releaseIdList.Add(currentRelease.Id);
            }

            return releaseIdList;
        }
    }
}
