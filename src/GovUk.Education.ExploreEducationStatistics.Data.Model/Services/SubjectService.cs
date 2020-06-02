using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, Guid>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(StatisticsDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestPublishedRelease(Guid subjectId)
        {
            var publicationId = GetPublicationForSubjectAsync(subjectId).Result.Id;
            var latestRelease = _releaseService.GetLatestPublishedRelease(publicationId);

            if (!latestRelease.HasValue)
            {
                return false;
            }
            
            return _context
                .ReleaseSubject
                .Any(r => r.ReleaseId == latestRelease.Value && r.SubjectId == subjectId);
        }
        
        public bool Exists(Guid releaseId, string name)
        {
            return GetAsync(releaseId, name).Result != null;
        }

        public async Task<Subject> GetAsync(Guid releaseId, string name)
        {
            return await _context
                .ReleaseSubject
                .Include(r => r.Subject)
                .Where(r => r.ReleaseId == releaseId && r.Subject.Name == name)
                .Select(r => r.Subject)
                .FirstOrDefaultAsync();
        }

        public Task<Publication> GetPublicationForSubjectAsync(Guid subjectId)
        {
            return _context
                .ReleaseSubject
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .Where(r => r.SubjectId == subjectId)
                .Select(r => r.Release.Publication)
                .FirstAsync();
        }

        public Task<List<Subject>> GetSubjectsForReleaseAsync(Guid releaseId)
        {
            return _context
                .ReleaseSubject
                .Include(s => s.Subject)
                .Where(s => s.ReleaseId == releaseId)
                .Select(s => s.Subject)
                .ToListAsync();
        } 
    }
}