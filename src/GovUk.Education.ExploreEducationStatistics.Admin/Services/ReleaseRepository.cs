using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly ContentDbContext _context;
        private readonly IMapper _mapper;

        public ReleaseRepository(ContentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ReleaseViewModel>> GetAllReleasesForReleaseStatusesAsync(
            params ReleaseStatus[] releaseStatuses)
        {
            var releases = await 
                HydrateReleaseForReleaseViewModel(_context.Releases)
                .Where(r => releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<ReleaseViewModel>>(releases);
        }

        public async Task<List<ReleaseViewModel>> GetReleasesForReleaseStatusRelatedToUserAsync(Guid userId,
            params ReleaseStatus[] releaseStatuses)
        {
            var userReleaseIds = await _context
                .UserReleaseRoles
                .Where(r => r.UserId == userId)
                .Select(r => r.ReleaseId)
                .ToListAsync();
            
            var releases = await 
                HydrateReleaseForReleaseViewModel(_context.Releases)
                .Where(r => userReleaseIds.Contains(r.Id) && releaseStatuses.Contains(r.Status))
                .ToListAsync();
            
            return _mapper.Map<List<ReleaseViewModel>>(releases);
        }
    }
}