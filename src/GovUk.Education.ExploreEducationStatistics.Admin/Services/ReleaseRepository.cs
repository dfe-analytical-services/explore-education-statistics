using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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

        public async Task<List<MyReleaseViewModel>> GetAllReleasesForReleaseStatusesAsync(
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            var releases = await 
                HydrateReleaseForReleaseViewModel(_contentDbContext.Releases)
                .Where(r => releaseApprovalStatuses.Contains(r.ApprovalStatus))
                .ToListAsync();
            
            return _mapper.Map<List<MyReleaseViewModel>>(releases);
        }

        public async Task<List<MyReleaseViewModel>> GetReleasesForReleaseStatusRelatedToUserAsync(Guid userId,
            params ReleaseApprovalStatus[] releaseApprovalStatuses)
        {
            var userReleaseIds = await _contentDbContext
                .UserReleaseRoles
                .Where(r => r.UserId == userId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseId)
                .ToListAsync();

            var userPublicationIds = await _contentDbContext
                .UserPublicationRoles
                .Where(r => r.UserId == userId && r.Role == PublicationRole.Owner)
                .Select(r => r.PublicationId)
                .ToListAsync();
            var userPublicationRoleReleaseIds = await _contentDbContext
                .Releases
                .Where(r => userPublicationIds.Contains(r.PublicationId))
                .Select(r => r.Id)
                .ToListAsync();
            userReleaseIds.AddRange(userPublicationRoleReleaseIds);

            var releases = await 
                HydrateReleaseForReleaseViewModel(_contentDbContext.Releases)
                .Where(r => userReleaseIds.Contains(r.Id) && releaseApprovalStatuses.Contains(r.ApprovalStatus))
                .ToListAsync();

            return _mapper.Map<List<MyReleaseViewModel>>(releases);
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
    }
}
