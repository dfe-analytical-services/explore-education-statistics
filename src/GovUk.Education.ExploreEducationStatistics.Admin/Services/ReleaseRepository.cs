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
            params ReleaseStatus[] releaseStatuses)
        {
            var releases = await 
                HydrateReleaseForReleaseViewModel(_contentDbContext.Releases)
                .Where(r => releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<MyReleaseViewModel>>(releases);
        }

        public async Task<List<MyReleaseViewModel>> GetReleasesForReleaseStatusRelatedToUserAsync(Guid userId,
            params ReleaseStatus[] releaseStatuses)
        {
            var userReleaseIds = await _contentDbContext
                .UserReleaseRoles
                .Where(r => r.UserId == userId && r.Role != ReleaseRole.PrereleaseViewer)
                .Select(r => r.ReleaseId)
                .ToListAsync();
            
            var releases = await 
                HydrateReleaseForReleaseViewModel(_contentDbContext.Releases)
                .Where(r => userReleaseIds.Contains(r.Id) && releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<MyReleaseViewModel>>(releases);
        }

        public async Task<Guid> CreateReleaseAndSubjectHierarchy(
            Guid releaseId,
            string subjectFilename,
            string subjectName)
        {
            var release = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefaultAsync(r => r.Id == releaseId);

            var existingTheme =
                await _statisticsDbContext.Theme.FindAsync(
                    release.Publication.Topic.Theme.Id);
            if (existingTheme == null)
            {
                var statsTheme = _mapper.Map(release.Publication.Topic.Theme, new Data.Model.Theme());
                await _statisticsDbContext.Theme.AddAsync(statsTheme);
            }
            else
            {
                _mapper.Map(release.Publication.Topic.Theme, existingTheme);
                _statisticsDbContext.Theme.Update(existingTheme);
            }

            var existingTopic = await _statisticsDbContext.Topic.FindAsync(
                release.Publication.Topic.Id);
            if (existingTopic == null)
            {
                var statsTopic = _mapper.Map(release.Publication.Topic, new Data.Model.Topic());
                await _statisticsDbContext.Topic.AddAsync(statsTopic);
            }
            else
            {
                _mapper.Map(release.Publication.Topic, existingTopic);
                _statisticsDbContext.Topic.Update(existingTopic);
            }

            var existingPub =
                await _statisticsDbContext.Publication.FindAsync(
                    release.Publication.Id);
            if (existingPub == null)
            {
                var statsPub = _mapper.Map(release.Publication, new Data.Model.Publication());
                await _statisticsDbContext.Publication.AddAsync(statsPub);
            }
            else
            {
                _mapper.Map(release.Publication, existingPub);
                _statisticsDbContext.Publication.Update(existingPub);
            }

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
                Subject = new Subject
                {
                    Filename = subjectFilename,
                    Name = subjectName
                }
            })).Entity;

            await _statisticsDbContext.SaveChangesAsync();
            return releaseSubject.Subject.Id;
        }
    }
}
