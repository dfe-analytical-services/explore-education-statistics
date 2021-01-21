#nullable enable
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

        public async Task<bool> IsSubjectForLatestPublishedRelease(Guid subjectId)
        {
            var publication = await GetPublicationForSubject(subjectId);
            var latestRelease = _releaseService.GetLatestPublishedRelease(publication.Id);

            if (latestRelease == null)
            {
                return false;
            }

            return _context
                .ReleaseSubject
                .Any(r => r.ReleaseId == latestRelease.Id && r.SubjectId == subjectId);
        }

        public async Task<Subject?> Get(Guid subjectId)
        {
            return await _context
                .Subject
                .FindAsync(subjectId);
        }

        public async Task<ReleaseSubject?> Get(Guid releaseId, string subjectName)
        {
            return await _context.ReleaseSubject
                .FirstOrDefaultAsync(r => 
                    r.ReleaseId == releaseId 
                    && r.SubjectName == subjectName);
        }

        public Task<Publication> GetPublicationForSubject(Guid subjectId)
        {
            return QueryPublicationForSubject(subjectId).FirstAsync();
        }

        public async Task<Publication?> FindPublicationForSubject(Guid subjectId)
        {
            return await QueryPublicationForSubject(subjectId).FirstOrDefaultAsync();
        }

        private IQueryable<Publication> QueryPublicationForSubject(Guid subjectId)
        {
            return _context
                .ReleaseSubject
                .Include(r => r.Release)
                .ThenInclude(r => r.Publication)
                .Where(r => r.SubjectId == subjectId)
                .Select(r => r.Release.Publication);
        }

        public Task<List<ReleaseSubject>> GetSubjectsForRelease(Guid releaseId)
        {
            return _context
                .ReleaseSubject
                .Where(s => s.ReleaseId == releaseId)
                .ToListAsync();
        }
    }
}
