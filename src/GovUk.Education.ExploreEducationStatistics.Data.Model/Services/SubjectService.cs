#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FileType = GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, Guid>, ISubjectService
    {
        private readonly IReleaseService _releaseService;
        private readonly ContentDbContext _contentDbContext;

        public SubjectService(StatisticsDbContext statisticsDbContext,
            ILogger<SubjectService> logger, 
            IReleaseService releaseService,
            ContentDbContext contentDbContext) : base(statisticsDbContext, logger)
        {
            _releaseService = releaseService;
            _contentDbContext = contentDbContext;
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

        public async Task<string> GetName(Guid releaseId, Guid subjectId)
        {
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .FirstAsync(rf =>
                    rf.FileId == rf.File.Id
                    && rf.ReleaseId == releaseId
                    && rf.File.Type == FileType.Data
                    && rf.File.SubjectId == subjectId);
            return releaseFile.Name ?? "Unknown";
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

        public Task<List<Subject>> GetSubjectsForRelease(Guid releaseId)
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
