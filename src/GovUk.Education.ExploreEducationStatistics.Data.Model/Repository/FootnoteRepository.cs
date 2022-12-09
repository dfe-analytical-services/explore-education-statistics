#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class FootnoteRepository : IFootnoteRepository
    {
        private readonly StatisticsDbContext _context;

        public FootnoteRepository(StatisticsDbContext context)
        {
            _context = context;
        }

        public async Task<Footnote> CreateFootnote(Guid releaseId,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            var footnote = new Footnote
            {
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        ReleaseId = releaseId
                    }
                },
                Content = content,
                Filters = filterIds.Select(filterId => new FilterFootnote
                {
                    FilterId = filterId
                }).ToList(),
                FilterGroups = filterGroupIds.Select(filterGroupId => new FilterGroupFootnote
                {
                    FilterGroupId = filterGroupId
                }).ToList(),
                FilterItems = filterItemIds.Select(filterItemId => new FilterItemFootnote
                {
                    FilterItemId = filterItemId
                }).ToList(),
                Indicators = indicatorIds.Select(indicatorId => new IndicatorFootnote
                {
                    IndicatorId = indicatorId
                }).ToList(),
                Subjects = subjectIds.Select(subjectId => new SubjectFootnote
                {
                    SubjectId = subjectId
                }).ToList()
            };

            // TODO EES-2971 We could link Footnote with ReleaseSubject to simplify getting all foonotes for a subject.
            // Currently this involves getting all footnotes with criteria matching the subject directly or indirectly.
            // This would also prevent removing a subject from a release until all of its footnote links are removed.
            // Currently a footnote could remain linked to a release and a subject (either directly or indirectly via
            // its FilterItems/FilterGroups/Filters/Indicators) even though no ReleaseSubject exists.

            await _context.Footnote.AddAsync(footnote);
            await _context.SaveChangesAsync();

            return footnote;
        }

        public async Task<List<Footnote>> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds)
        {
            // Query the id's of the filter groups and filters corresponding with the filter item id's
            // Sanitise all id's to make sure they belong to the subject
            var filterIdsResult = await _context.FilterItem
                .Where(filterItem => filterItemIds.Contains(filterItem.Id)
                                     && filterItem.FilterGroup.Filter.SubjectId == subjectId)
                .Select(filterItem => new
                {
                    FilterItemId = filterItem.Id,
                    filterItem.FilterGroupId,
                    filterItem.FilterGroup.FilterId
                })
                .ToListAsync();

            var filterItemIdsSanitised = filterIdsResult
                .Select(filter => filter.FilterItemId)
                .ToList();

            var filterGroupsIdsSanitised = filterIdsResult
                .Select(filter => filter.FilterGroupId)
                .Distinct()
                .ToList();

            var filterIdsSanitised = filterIdsResult
                .Select(filter => filter.FilterId)
                .Distinct()
                .ToList();

            // Sanitise the indicator id's to make sure they belong to the subject
            var indicatorIdsSanitised = await _context.Indicator
                .Where(indicator => indicatorIds.Contains(indicator.Id)
                                    && indicator.IndicatorGroup.SubjectId == subjectId)
                .Select(indicator => indicator.Id)
                .ToListAsync();

            // A footnote must be linked to the release id
            var footnotesQueryable = _context
                .Footnote
                .Where(footnote => footnote.Releases.Any(releaseFootnote => releaseFootnote.ReleaseId == releaseId));

            // If the list of filter and indicator id's are empty,
            // return early with only footnotes that apply directly to this subject
            if (!filterIdsResult.Any() && !indicatorIdsSanitised.Any())
            {
                return await footnotesQueryable.Where(footnote =>
                        footnote.Subjects.Any(subjectFootnote => subjectFootnote.SubjectId == subjectId)

                        // We don't allow a footnote to apply to directly to a subject and to specific subject data
                        // of the same subject at the same time, so these additional conditions aren't strictly necessary
                        && footnote.FilterItems.All(itemFootnote =>
                            itemFootnote.FilterItem.FilterGroup.Filter.SubjectId != subjectId)
                        && footnote.FilterGroups.All(filterGroupFootnote =>
                            filterGroupFootnote.FilterGroup.Filter.SubjectId != subjectId)
                        && footnote.Filters.All(filterFootnote =>
                            filterFootnote.Filter.SubjectId != subjectId)
                        && footnote.Indicators.All(indicatorFootnote =>
                            indicatorFootnote.Indicator.IndicatorGroup.SubjectId != subjectId))
                    .ToListAsync();
            }

            // A footnote which applies directly to any subjects must include this subject id
            footnotesQueryable = footnotesQueryable.Where(footnote =>
                !footnote.Subjects.Any() || footnote.Subjects.Any(subjectFootnote =>
                    subjectFootnote.SubjectId == subjectId));

            // A footnote which applies to any indicators of this subject must include one of the indicator id's
            footnotesQueryable = footnotesQueryable.Where(footnote =>
                footnote.Indicators.All(indicatorFootnote =>
                    indicatorFootnote.Indicator.IndicatorGroup.SubjectId != subjectId) ||
                footnote.Indicators.Any(indicatorFootnote =>
                    indicatorIdsSanitised.Contains(indicatorFootnote.IndicatorId)));

            // A footnote which applies to any filter items of this subject must include one of the the filter item id's 
            footnotesQueryable = footnotesQueryable.Where(footnote =>
                footnote.FilterItems.All(filterItemFootnote =>
                    filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId != subjectId) ||
                footnote.FilterItems.Any(filterItemFootnote =>
                    filterItemIdsSanitised.Contains(filterItemFootnote.FilterItemId)));

            // A footnote which applies to any filter groups of this subject must include one of the the filter group id's
            footnotesQueryable = footnotesQueryable.Where(footnote =>
                footnote.FilterGroups.All(filterGroupFootnote =>
                    filterGroupFootnote.FilterGroup.Filter.SubjectId != subjectId) ||
                footnote.FilterGroups.Any(filterGroupFootnote =>
                    filterGroupsIdsSanitised.Contains(filterGroupFootnote.FilterGroupId)));

            // A footnote which applies to any filters of this subject must include one of the the filter id's
            footnotesQueryable = footnotesQueryable.Where(footnote =>
                footnote.Filters.All(filterFootnote =>
                    filterFootnote.Filter.SubjectId != subjectId) ||
                footnote.Filters.Any(filterFootnote =>
                    filterIdsSanitised.Contains(filterFootnote.FilterId)));

            // Execute the query
            return await footnotesQueryable.ToListAsync();
        }

        public async Task DeleteAllFootnotesBySubject(Guid releaseId, Guid subjectId)
        {
            var footnotesLinkedToSubject = await GetFootnotes(releaseId, subjectId);

            foreach (var footnote in footnotesLinkedToSubject)
            {
                var canRemoveReleaseFootnote = !await IsFootnoteLinkedToAnotherSubjectForThisRelease(footnote.Id, releaseId, subjectId);
                var canRemoveFootnote = canRemoveReleaseFootnote && !await IsFootnoteLinkedToAnotherSubject(footnote.Id, subjectId);

                await DeleteFootnote(releaseId, footnote, canRemoveReleaseFootnote, canRemoveFootnote);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFootnote(Guid releaseId, Guid id)
        {
            var footnote = await GetFootnote(id);
            await DeleteFootnote(releaseId, footnote, canRemoveReleaseFootnote: true, canRemoveFootnote: true);
            await _context.SaveChangesAsync();
        }

        private async Task DeleteFootnote(Guid releaseId, Footnote footnote, bool canRemoveReleaseFootnote, bool canRemoveFootnote)
        {
            // Break link with the whole release if this footnote is not related to any other subject for the release
            // Else, as the ReleaseSubject link will be broken then footnote will not be visible.
            if (canRemoveReleaseFootnote)
            {
                await DeleteReleaseFootnoteLinkAsync(releaseId, footnote.Id);
            }

            if (await IsFootnoteExclusiveToRelease(releaseId, footnote.Id))
            {
                if (canRemoveFootnote)
                {
                    var footnoteToRemove = await _context.Footnote.SingleAsync(f => f.Id == footnote.Id);
                    _context.Footnote.Remove(footnoteToRemove);
                }
            }
        }

        public async Task<bool> IsFootnoteExclusiveToRelease(Guid releaseId, Guid footnoteId)
        {
            var otherFootnoteReferences = await _context.ReleaseFootnote
                .CountAsync(rf => rf.FootnoteId == footnoteId && rf.ReleaseId != releaseId);

            return otherFootnoteReferences == 0;
        }

        public Task<Footnote> GetFootnote(Guid id)
        {
            return GetBaseFootnoteQuery().SingleAsync(f => f.Id == id);
        }

        public async Task<List<Footnote>> GetFootnotes(Guid releaseId, Guid? subjectId = null)
        {
            if (subjectId.HasValue)
            {
                return await GetFootnotes(releaseId, new List<Guid>
                {
                    subjectId.Value
                });
            }

            var subjectsIdsForRelease = await _context.ReleaseSubject
                    .Where(rs => rs.ReleaseId == releaseId)
                    .Select(rs => rs.SubjectId)
                    .ToListAsync();

            var footnotes = await GetFootnotes(releaseId, subjectsIdsForRelease);

            // Remove the below check as part of EES-2971
            var releaseFootnotes = _context.ReleaseFootnote
                .Where(rf => rf.ReleaseId == releaseId)
                .ToList();
            if (footnotes.Count != releaseFootnotes.Count)
            {
                throw new Exception(
                    $"Number of footnotes different from number of ReleaseFootnotes for release {releaseId}");
            }

            return footnotes;
        }

        private async Task<List<Footnote>> GetFootnotes(Guid releaseId, IEnumerable<Guid> subjectIds)
        {
            return (await GetBaseFootnoteQuery()
                    .Where(footnote => footnote.Releases.Any(releaseFootnote => releaseFootnote.ReleaseId == releaseId))
                    .Select(f => new Footnote
                    {
                        Id = f.Id,
                        Content = f.Content,
                        Filters = f.Filters
                            .Where(filterFootnote => subjectIds.Contains(filterFootnote.Filter.SubjectId))
                            .ToList(),
                        Indicators = f.Indicators
                            .Where(indicatorFootnote =>
                                subjectIds.Contains(indicatorFootnote.Indicator.IndicatorGroup.SubjectId))
                            .ToList(),
                        Releases = f.Releases
                            .Where(releaseFootnote => releaseFootnote.ReleaseId == releaseId)
                            .ToList(),
                        FilterGroups = f.FilterGroups
                            .Where(filterGroupFootnote =>
                                subjectIds.Contains(filterGroupFootnote.FilterGroup.Filter.SubjectId))
                            .ToList(),
                        FilterItems = f.FilterItems
                            .Where(filterItemFootnote =>
                                subjectIds.Contains(filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId))
                            .ToList(),
                        Subjects = f.Subjects
                            .Where(subjectFootnote => subjectIds.Contains(subjectFootnote.SubjectId))
                            .ToList()
                    })
                    .ToListAsync())
                .Where(f => f.Filters.Any() ||
                            f.Indicators.Any() ||
                            f.FilterGroups.Any() ||
                            f.FilterItems.Any() ||
                            f.Subjects.Any()
                )
                .ToList();
        }

        private IQueryable<Footnote> GetBaseFootnoteQuery()
        {
            return _context.Footnote
                .Include(footnote => footnote.Filters)
                .ThenInclude(filterFootnote => filterFootnote.Filter)
                .Include(footnote => footnote.FilterGroups)
                .ThenInclude(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .ThenInclude(filterGroup => filterGroup.Filter)
                .Include(footnote => footnote.FilterItems)
                .ThenInclude(filterItemFootnote => filterItemFootnote.FilterItem)
                .ThenInclude(filterItem => filterItem.FilterGroup)
                .ThenInclude(filterGroup => filterGroup.Filter)
                .Include(footnote => footnote.Indicators)
                .ThenInclude(indicatorFootnote => indicatorFootnote.Indicator)
                .ThenInclude(indicator => indicator.IndicatorGroup)
                .Include(footnote => footnote.Releases)
                .Include(footnote => footnote.Subjects);
        }

        public async Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseId)
        {
            return await _context.ReleaseSubject
                .Where(releaseSubject => releaseSubject.ReleaseId == releaseId)
                .Where(
                    releaseSubject =>
                        !releaseSubject.Subject.Footnotes.Any()
                        && !releaseSubject.Subject.Filters.SelectMany(filter => filter.Footnotes).Any()
                        && !releaseSubject.Subject.Filters
                            .SelectMany(filter => filter.FilterGroups)
                            .SelectMany(filterGroup => filterGroup.Footnotes)
                            .Any()
                        && !releaseSubject.Subject.Filters
                            .SelectMany(filter => filter.FilterGroups)
                            .SelectMany(filterGroup => filterGroup.FilterItems)
                            .SelectMany(filterItem => filterItem.Footnotes)
                            .Any()
                        && !releaseSubject.Subject.IndicatorGroups
                            .SelectMany(indicatorGroup => indicatorGroup.Indicators)
                            .SelectMany(indicator => indicator.Footnotes)
                            .Any()
                )
                .Select(releaseSubject => releaseSubject.Subject)
                .ToListAsync();
        }

        public async Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId)
        {
            var releaseFootnote = await _context.ReleaseFootnote
                .Where(rf => rf.ReleaseId == releaseId && rf.FootnoteId == footnoteId)
                .FirstAsync();

            _context.ReleaseFootnote.Remove(releaseFootnote);
        }

        private async Task<bool> IsFootnoteLinkedToAnotherSubject(Guid footnoteId, Guid subjectId)
        {
            var footnote = await GetFootnote(footnoteId);
            return footnote.Subjects.Any(subject => subject.SubjectId != subjectId) ||
                   footnote.Filters.Any(filter => filter.Filter.SubjectId != subjectId) ||
                   footnote.Indicators.Any(indicator => indicator.Indicator.IndicatorGroup.SubjectId != subjectId) ||
                   footnote.FilterGroups.Any(filterGroup => filterGroup.FilterGroup.Filter.SubjectId != subjectId) ||
                   footnote.FilterItems.Any(filterItem => filterItem.FilterItem.FilterGroup.Filter.SubjectId != subjectId);
        }

        private async Task<bool> IsFootnoteLinkedToAnotherSubjectForThisRelease(Guid footnoteId,
            Guid releaseId,
            Guid subjectId)
        {
            var otherSubjectIds = await _context.ReleaseSubject
                .Where(rs => rs.ReleaseId == releaseId && rs.SubjectId != subjectId)
                .Select(rs => rs.SubjectId)
                .ToListAsync();

            var footnote = await GetFootnote(footnoteId);
            return footnote.Subjects.Any(subject => otherSubjectIds.Contains(subject.SubjectId)) ||
                   footnote.Filters.Any(filter => otherSubjectIds.Contains(filter.Filter.SubjectId)) ||
                   footnote.Indicators.Any(indicator => otherSubjectIds.Contains(indicator.Indicator.IndicatorGroup.SubjectId)) ||
                   footnote.FilterGroups.Any(filterGroup => otherSubjectIds.Contains(filterGroup.FilterGroup.Filter.SubjectId)) ||
                   footnote.FilterItems.Any(filterItem => otherSubjectIds.Contains(filterItem.FilterItem.FilterGroup.Filter.SubjectId));
        }
    }
}
