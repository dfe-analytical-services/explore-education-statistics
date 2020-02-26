using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, ReleaseId>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(StatisticsDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestRelease(ReleaseId subjectId)
        {
            var subject = Find(subjectId, new List<Expression<Func<Subject, object>>> {s => s.Release});
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(subjectId));
            }
            return _releaseService.GetLatestRelease(subject.Release.PublicationId).Equals(subject.ReleaseId);
        }
        
        public bool Exists(ReleaseId releaseId, string name)
        {
            return _context.Subject.Any(x => x.ReleaseId == releaseId && x.Name == name);
        }

        public async Task<List<Footnote>> GetFootnotesOnlyForSubjectAsync(ReleaseId subjectId)
        {
            var releaseId = _context
                .Subject
                .First(s => s.Id == subjectId)
                .ReleaseId;

            var releaseFootnotes = await _context
                .Footnote
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Include(f => f.FilterItems)
                .ThenInclude(fi => fi.FilterItem)
                .ThenInclude(fi => fi.FilterGroup)
                .ThenInclude(fg => fg.Filter)
                .Include(f => f.Indicators)
                .ThenInclude(i => i.Indicator)
                .ThenInclude(i => i.IndicatorGroup)
                .Include(f => f.Subjects)
                .Where(f => f.ReleaseId == releaseId)
                .ToListAsync();
             
                return releaseFootnotes
                    .Where(f => FootnoteLinkedToNoOtherSubject(subjectId, f))
                    .ToList();
        }

        public async Task<bool> DeleteAsync(ReleaseId subjectId)
        {
            var subject = await _context
                .Subject
                .FirstOrDefaultAsync(s => s.Id == subjectId);
                
            if (subject != null)
            {
                var orphanFootnotes = await GetFootnotesOnlyForSubjectAsync(subjectId);

                _context.Subject.Remove(subject);
                _context.Footnote.RemoveRange(orphanFootnotes);
                
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(ReleaseId releaseId, string name)
        {
            var subject = await GetAsync(releaseId, name);
            return await DeleteAsync(subject.Id);
        }
        
        public async Task<Subject> GetAsync(ReleaseId releaseId, string name)
        {
            return await _context
                .Subject
                .FirstOrDefaultAsync(s => s.ReleaseId == releaseId && s.Name == name);
        }

        private static bool FootnoteLinkedToNoOtherSubject(ReleaseId subjectId, Footnote f)
        {
            // if this Footnote is directly linked to any other Subjects than this one, then it's not just for
            // this Subject
            if (f.Subjects.Any(subject => subject.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter for any other Subject, then it's not just for
            // this Subject
            if (f.Filters.Any(filter => filter.Filter.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to an Indicator for any other Subject, then it's not just for
            // this Subject
            if (f.Indicators.Any(indicator => indicator.Indicator.IndicatorGroup.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter Group for any other Subject, then it's not just for
            // this Subject
            if (f.FilterGroups.Any(filterGroup => filterGroup.FilterGroup.Filter.SubjectId != subjectId))
            {
                return false;
            }

            // if this Footnote is directly linked to a Filter Group for any other Subject, then it's not just for
            // this Subject
            if (f.FilterItems.Any(filterItem => filterItem.FilterItem.FilterGroup.Filter.SubjectId != subjectId))
            {
                return false;
            }

            return true;
        }
    }
}