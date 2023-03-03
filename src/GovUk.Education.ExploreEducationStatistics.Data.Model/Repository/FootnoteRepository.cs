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
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds,
            int order)
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
                Order = order,
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
            var filterItemIdsResult = await _context.FilterItem
                .Where(filterItem => filterItemIds.Contains(filterItem.Id)
                                     && filterItem.FilterGroup.Filter.SubjectId == subjectId)
                .Select(filterItem => new
                {
                    FilterItemId = filterItem.Id,
                    filterItem.FilterGroupId,
                    filterItem.FilterGroup.FilterId
                })
                .ToListAsync();

            var filterItemIdsSanitised = filterItemIdsResult
                .Select(filter => filter.FilterItemId)
                .ToList();

            var filterGroupsIdsSanitised = filterItemIdsResult
                .Select(filter => filter.FilterGroupId)
                .Distinct()
                .ToList();

            var filterIdsSanitised = filterItemIdsResult
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
            if (!filterItemIdsResult.Any() && !indicatorIdsSanitised.Any())
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
                    .OrderBy(footnote => footnote.Order)
                    .ThenBy(footnote => footnote.Id)
                    .ToListAsync();
            }

            footnotesQueryable = footnotesQueryable.Where(footnote =>
                // A footnote must be applied directly to this subject id    
                footnote.Subjects.Any(subjectFootnote =>
                    subjectFootnote.SubjectId == subjectId)
                ||
                // or be applicable to it through filters / filter groups / filter items or indicators.
                (
                    // It must be applicable to it in some way
                    // rather than just being applicable exclusively to other subjects
                    (footnote.FilterItems.Any(filterItemFootnote =>
                         filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId == subjectId) ||
                     footnote.FilterGroups.Any(filterGroupFootnote =>
                         filterGroupFootnote.FilterGroup.Filter.SubjectId == subjectId) ||
                     footnote.Filters.Any(filterFootnote =>
                         filterFootnote.Filter.SubjectId == subjectId) ||
                     footnote.Indicators.Any(indicatorFootnote =>
                         indicatorFootnote.Indicator.IndicatorGroup.SubjectId == subjectId))
                    &&
                    // and it must be applicable to one or more filters / filter groups / filter items if there are any.
                    (
                        // It's either not got any applicable filters / filter groups / filter items for this subject id
                        (footnote.FilterItems.All(filterItemFootnote =>
                             filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId != subjectId) &&
                         footnote.FilterGroups.All(filterGroupFootnote =>
                             filterGroupFootnote.FilterGroup.Filter.SubjectId != subjectId) &&
                         footnote.Filters.All(filterFootnote => filterFootnote.Filter.SubjectId != subjectId))
                        ||
                        // or it's got a filter item applicable to one of the filter item id's 
                        footnote.FilterItems.Any(filterItemFootnote =>
                            filterItemIdsSanitised.Contains(filterItemFootnote.FilterItemId))
                        ||
                        // or it's got a filter group applicable to one of the filter group id's
                        footnote.FilterGroups.Any(filterGroupFootnote =>
                            filterGroupsIdsSanitised.Contains(filterGroupFootnote.FilterGroupId))
                        ||
                        // or it's got a filter applicable to one of the filter id's.
                        footnote.Filters.Any(filterFootnote =>
                            filterIdsSanitised.Contains(filterFootnote.FilterId))
                    )
                    &&
                    // and it must be applicable to one or more of the indicators if there are any.
                    (
                        // It's either not got any applicable indicators for this subject id 
                        footnote.Indicators.All(indicatorFootnote =>
                            indicatorFootnote.Indicator.IndicatorGroup.SubjectId != subjectId)
                        ||
                        // or it's got an indicator applicable to one of the indicator id's.
                        footnote.Indicators.Any(indicatorFootnote =>
                            indicatorIdsSanitised.Contains(indicatorFootnote.IndicatorId))
                    )
                ));

            // Execute the query
            return await footnotesQueryable
                .OrderBy(footnote => footnote.Order)
                .ThenBy(footnote => footnote.Id)
                .ToListAsync();
        }

        public async Task DeleteFootnotesBySubject(Guid releaseId, Guid subjectId)
        {
            var footnotes = await GetFootnotes(releaseId, subjectId);

            await footnotes
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async footnote =>
                {
                    if (await IsFootnoteLinkedToAnotherSubject(footnote.Id, subjectId))
                    {
                        // Don't delete the footnote because it's linked to other subjects

                        // We can't rely on this subject being deleted after this method call because a subject isn't
                        // deleted if its linked to previous release versions.
                        // Therefore we can't rely on the cascade delete of all the footnote links related to the subject
                        // which would happen when it's deleted. Explicitly delete the links instead

                        var filterItemFootnoteLinks = _context.FilterItemFootnote
                            .Where(filterItemFootnote =>
                                filterItemFootnote.FootnoteId == footnote.Id &&
                                filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId == subjectId);
                        _context.FilterItemFootnote.RemoveRange(filterItemFootnoteLinks);

                        var filterGroupFootnoteLinks = _context.FilterGroupFootnote
                            .Where(filterGroupFootnote =>
                                filterGroupFootnote.FootnoteId == footnote.Id &&
                                filterGroupFootnote.FilterGroup.Filter.SubjectId == subjectId);
                        _context.FilterGroupFootnote.RemoveRange(filterGroupFootnoteLinks);

                        var filterFootnoteLinks = _context.FilterFootnote
                            .Where(filterFootnote =>
                                filterFootnote.FootnoteId == footnote.Id &&
                                filterFootnote.Filter.SubjectId == subjectId);
                        _context.FilterFootnote.RemoveRange(filterFootnoteLinks);

                        var indicatorFootnoteLinks = _context.IndicatorFootnote
                            .Where(indicatorFootnote =>
                                indicatorFootnote.FootnoteId == footnote.Id &&
                                indicatorFootnote.Indicator.IndicatorGroup.SubjectId == subjectId);
                        _context.IndicatorFootnote.RemoveRange(indicatorFootnoteLinks);

                        var subjectFootnoteLinks = _context.SubjectFootnote
                            .Where(subjectFootnote =>
                                subjectFootnote.FootnoteId == footnote.Id &&
                                subjectFootnote.SubjectId == subjectId);
                        _context.SubjectFootnote.RemoveRange(subjectFootnoteLinks);

                        if (!await IsFootnoteLinkedToAnotherSubjectForThisRelease(footnoteId: footnote.Id,
                                releaseId: releaseId,
                                subjectId: subjectId))
                        {
                            // Footnote will become exclusive to subjects which are not linked to this release
                            // so we can remove the footnote link to this release
                            // TODO EES-2979 Remove this else block when a footnote can only belong to one release
                            await DeleteReleaseFootnoteLink(releaseId: releaseId,
                                footnoteId: footnote.Id);
                        }

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // It's safe to delete the footnote as it's not linked to any other subject
                        await DeleteFootnote(releaseId: releaseId,
                            footnoteId: footnote.Id);
                    }
                });
        }

        public async Task DeleteFootnote(Guid releaseId, Guid footnoteId)
        {
            if (await IsFootnoteExclusiveToRelease(releaseId: releaseId, footnoteId: footnoteId))
            {
                var footnoteToRemove = await _context.Footnote
                    // These includes are necessary so that the related entities are marked for deletion  
                    .Include(footnote => footnote.Releases)
                    .Include(footnote => footnote.Subjects)
                    .Include(footnote => footnote.Filters)
                    .Include(footnote => footnote.FilterGroups)
                    .Include(footnote => footnote.FilterItems)
                    .Include(footnote => footnote.Indicators)
                    .SingleAsync(f => f.Id == footnoteId);

                var footnotesToReorder = await _context.ReleaseFootnote
                    .Include(rf => rf.Footnote)
                    .Where(rf => rf.ReleaseId == releaseId &&
                                 rf.Footnote.Order > footnoteToRemove.Order)
                    .Select(rf => rf.Footnote)
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                _context.Footnote.Remove(footnoteToRemove);
                _context.Footnote.UpdateRange(footnotesToReorder);
                footnotesToReorder.ForEach(footnoteToReorder =>
                {
                    if (footnoteToReorder.Order > 0)
                    {
                        footnoteToReorder.Order--;
                    }
                });
                await _context.SaveChangesAsync();
            }
            else
            {
                // TODO EES-2979 Remove this once a footnote can only belong to one release
                // It's only be possible to reach this code if editing a very old draft release amendment,
                // created circa September 2020 when we began copying footnotes when creating new versions.
                await DeleteReleaseFootnoteLink(releaseId: releaseId,
                    footnoteId: footnoteId);
                await _context.SaveChangesAsync();

                // TODO Because the footnote isn't deleted this could be leaving footnote links to subjects or
                // filters/filter groups/filter items of subjects that only belong to this release.
                // It seems unlikely we will need to fix this, but prioritise EES-2979 instead. 
            }
        }

        public async Task<bool> IsFootnoteExclusiveToRelease(Guid releaseId, Guid footnoteId)
        {
            // TODO EES-2979 Remove this method
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
                        Order = f.Order,
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
                .OrderBy(f => f.Order)
                .ThenBy(f => f.Id)
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

        public async Task DeleteReleaseFootnoteLink(Guid releaseId, Guid footnoteId)
        {
            // TODO EES-2979 Remove this method
            var releaseFootnote = await _context.ReleaseFootnote
                .FirstAsync(rf => rf.ReleaseId == releaseId && rf.FootnoteId == footnoteId);
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
