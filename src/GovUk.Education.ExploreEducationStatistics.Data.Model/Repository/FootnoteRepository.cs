#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class FootnoteRepository : IFootnoteRepository
{
    private readonly StatisticsDbContext _context;

    public FootnoteRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<Footnote> CreateFootnote(
        Guid releaseVersionId,
        string content,
        List<FilterFootnote> filterLinks,
        List<FilterGroupFootnote> filterGroupLinks,
        List<FilterItemFootnote> filterItemLinks,
        List<IndicatorFootnote> indicatorLinks,
        IReadOnlySet<Guid> subjectIds,
        int order
    )
    {
        var footnote = new Footnote
        {
            ReleaseVersionId = releaseVersionId,
            Content = content,
            Order = order,
            Filters = filterLinks,
            FilterGroups = filterGroupLinks,
            FilterItems = filterItemLinks,
            Indicators = indicatorLinks,
            Subjects = subjectIds.Select(subjectId => new SubjectFootnote { SubjectId = subjectId }).ToList(),
        };

        await _context.Footnote.AddAsync(footnote);
        await _context.SaveChangesAsync();

        return footnote;
    }

    public async Task<List<Footnote>> GetFootnotesForSelection(
        Guid releaseVersionId,
        Guid subjectId,
        IReadOnlyCollection<Guid> filterIds,
        IReadOnlyCollection<Guid> filterGroupIds,
        IReadOnlyCollection<Guid> filterItemIds,
        IReadOnlyCollection<Guid> indicatorIds
    )
    {
        // A footnote must belong to the release version
        var footnotesQueryable = _context.Footnote.Where(footnote => footnote.ReleaseVersionId == releaseVersionId);

        // If the list of filter and indicator id's are empty,
        // return early with only footnotes that apply directly to this subject
        if (filterItemIds.Count == 0 && indicatorIds.Count == 0)
        {
            return await footnotesQueryable
                .Where(footnote =>
                    footnote.Subjects.Any(subjectFootnote => subjectFootnote.SubjectId == subjectId)
                    // We don't allow a footnote to apply to directly to a subject and to specific subject data
                    // of the same subject at the same time, so these additional conditions aren't strictly necessary
                    && footnote.FilterItems.All(itemFootnote => itemFootnote.SubjectId != subjectId)
                    && footnote.FilterGroups.All(filterGroupFootnote => filterGroupFootnote.SubjectId != subjectId)
                    && footnote.Filters.All(filterFootnote => filterFootnote.SubjectId != subjectId)
                    && footnote.Indicators.All(indicatorFootnote => indicatorFootnote.SubjectId != subjectId)
                )
                .OrderBy(footnote => footnote.Order)
                .ThenBy(footnote => footnote.Id)
                .ToListAsync();
        }

        footnotesQueryable = footnotesQueryable.Where(footnote =>
            // A footnote must be applied directly to this subject id
            footnote.Subjects.Any(subjectFootnote => subjectFootnote.SubjectId == subjectId)
            ||
            // or be applicable to it through filters / filter groups / filter items or indicators.
            (
                // It must be applicable to it in some way
                // rather than just being applicable exclusively to other subjects
                (
                    footnote.FilterItems.Any(filterItemFootnote => filterItemFootnote.SubjectId == subjectId)
                    || footnote.FilterGroups.Any(filterGroupFootnote => filterGroupFootnote.SubjectId == subjectId)
                    || footnote.Filters.Any(filterFootnote => filterFootnote.SubjectId == subjectId)
                    || footnote.Indicators.Any(indicatorFootnote => indicatorFootnote.SubjectId == subjectId)
                )
                &&
                // and it must be applicable to one or more filters / filter groups / filter items if there are any.
                (
                    // It's either not got any applicable filters / filter groups / filter items for this subject id
                    (
                        footnote.FilterItems.All(filterItemFootnote => filterItemFootnote.SubjectId != subjectId)
                        && footnote.FilterGroups.All(filterGroupFootnote => filterGroupFootnote.SubjectId != subjectId)
                        && footnote.Filters.All(filterFootnote => filterFootnote.SubjectId != subjectId)
                    )
                    ||
                    // or it's got a filter item for this subject id applicable to one of the filter item id's
                    footnote.FilterItems.Any(filterItemFootnote =>
                        filterItemFootnote.SubjectId == subjectId
                        && filterItemIds.Contains(filterItemFootnote.FilterItemId)
                    )
                    ||
                    // or it's got a filter group for this subject id applicable to one of the filter group id's
                    footnote.FilterGroups.Any(filterGroupFootnote =>
                        filterGroupFootnote.SubjectId == subjectId
                        && filterGroupIds.Contains(filterGroupFootnote.FilterGroupId)
                    )
                    ||
                    // or it's got a filter for this subject id applicable to one of the filter id's.
                    footnote.Filters.Any(filterFootnote =>
                        filterFootnote.SubjectId == subjectId && filterIds.Contains(filterFootnote.FilterId)
                    )
                )
                &&
                // and it must be applicable to one or more of the indicators if there are any.
                (
                    // It's either not got any applicable indicators for this subject id
                    footnote.Indicators.All(indicatorFootnote => indicatorFootnote.SubjectId != subjectId)
                    ||
                    // or it's got an indicator for this subject id applicable to one of the indicator id's.
                    footnote.Indicators.Any(indicatorFootnote =>
                        indicatorFootnote.SubjectId == subjectId && indicatorIds.Contains(indicatorFootnote.IndicatorId)
                    )
                )
            )
        );

        // Execute the query
        return await footnotesQueryable
            .OrderBy(footnote => footnote.Order)
            .ThenBy(footnote => footnote.Id)
            .ToListAsync();
    }

    public async Task DeleteFootnotesBySubject(Guid releaseVersionId, Guid subjectId)
    {
        var footnotes = await GetFootnotes(releaseVersionId, subjectId);

        await footnotes
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async footnote =>
            {
                if (
                    await IsFootnoteLinkedToAnotherSubjectForThisRelease(
                        footnoteId: footnote.Id,
                        releaseVersionId: releaseVersionId,
                        subjectId: subjectId
                    )
                )
                {
                    // Don't delete the footnote because it's linked to other subjects of this release version

                    // We can't rely on this subject being deleted after this method call because a subject isn't
                    // deleted if its linked to previous release versions.
                    // Therefore we can't rely on the cascade delete of all the footnote links related to the subject
                    // which would happen when it's deleted. Explicitly delete the links instead

                    var filterItemFootnoteLinks = _context.FilterItemFootnote.Where(filterItemFootnote =>
                        filterItemFootnote.FootnoteId == footnote.Id && filterItemFootnote.SubjectId == subjectId
                    );
                    _context.FilterItemFootnote.RemoveRange(filterItemFootnoteLinks);

                    var filterGroupFootnoteLinks = _context.FilterGroupFootnote.Where(filterGroupFootnote =>
                        filterGroupFootnote.FootnoteId == footnote.Id && filterGroupFootnote.SubjectId == subjectId
                    );
                    _context.FilterGroupFootnote.RemoveRange(filterGroupFootnoteLinks);

                    var filterFootnoteLinks = _context.FilterFootnote.Where(filterFootnote =>
                        filterFootnote.FootnoteId == footnote.Id && filterFootnote.SubjectId == subjectId
                    );
                    _context.FilterFootnote.RemoveRange(filterFootnoteLinks);

                    var indicatorFootnoteLinks = _context.IndicatorFootnote.Where(indicatorFootnote =>
                        indicatorFootnote.FootnoteId == footnote.Id && indicatorFootnote.SubjectId == subjectId
                    );
                    _context.IndicatorFootnote.RemoveRange(indicatorFootnoteLinks);

                    var subjectFootnoteLinks = _context.SubjectFootnote.Where(subjectFootnote =>
                        subjectFootnote.FootnoteId == footnote.Id && subjectFootnote.SubjectId == subjectId
                    );
                    _context.SubjectFootnote.RemoveRange(subjectFootnoteLinks);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // It's safe to delete the footnote as it's not linked to any other subject of this release version
                    await DeleteFootnote(releaseVersionId: releaseVersionId, footnoteId: footnote.Id);
                }
            });
    }

    public async Task DeleteFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        var footnoteToRemove = await _context
            .Footnote
            // The database cascades the delete to these links, but the in-memory test provider only cascades to
            // tracked entities, so these includes are only needed for tests
            .Include(footnote => footnote.Subjects)
            .Include(footnote => footnote.Filters)
            .Include(footnote => footnote.FilterGroups)
            .Include(footnote => footnote.FilterItems)
            .Include(footnote => footnote.Indicators)
            .SingleAsync(f => f.Id == footnoteId && f.ReleaseVersionId == releaseVersionId);

        var footnotesToReorder = await _context
            .Footnote.Where(f => f.ReleaseVersionId == releaseVersionId && f.Order > footnoteToRemove.Order)
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

    public Task<Footnote> GetFootnote(Guid id)
    {
        return GetBaseFootnoteQuery().SingleAsync(f => f.Id == id);
    }

    public async Task<List<Footnote>> GetFootnotes(Guid releaseVersionId, Guid? subjectId = null)
    {
        if (subjectId.HasValue)
        {
            return await GetFootnotes(releaseVersionId, new List<Guid> { subjectId.Value });
        }

        var subjectsIdsForReleaseVersion = await _context
            .ReleaseSubject.Where(rs => rs.ReleaseVersionId == releaseVersionId)
            .Select(rs => rs.SubjectId)
            .ToListAsync();

        var footnotes = await GetFootnotes(releaseVersionId, subjectsIdsForReleaseVersion);

        // Remove the below check as part of EES-2971
        var releaseVersionFootnoteCount = await _context.Footnote.CountAsync(footnote =>
            footnote.ReleaseVersionId == releaseVersionId
        );
        if (footnotes.Count != releaseVersionFootnoteCount)
        {
            throw new Exception(
                $"Number of footnotes linked to Subjects of ReleaseVersion {releaseVersionId} different from number of footnotes belonging to it"
            );
        }

        return footnotes;
    }

    private async Task<List<Footnote>> GetFootnotes(Guid releaseVersionId, IEnumerable<Guid> subjectIds)
    {
        return (
            await GetBaseFootnoteQuery()
                .Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                .Select(f => new Footnote
                {
                    Id = f.Id,
                    Content = f.Content,
                    Order = f.Order,
                    ReleaseVersionId = f.ReleaseVersionId,
                    Filters = f.Filters.Where(filterFootnote => subjectIds.Contains(filterFootnote.SubjectId)).ToList(),
                    Indicators = f
                        .Indicators.Where(indicatorFootnote => subjectIds.Contains(indicatorFootnote.SubjectId))
                        .ToList(),
                    FilterGroups = f
                        .FilterGroups.Where(filterGroupFootnote => subjectIds.Contains(filterGroupFootnote.SubjectId))
                        .ToList(),
                    FilterItems = f
                        .FilterItems.Where(filterItemFootnote => subjectIds.Contains(filterItemFootnote.SubjectId))
                        .ToList(),
                    Subjects = f
                        .Subjects.Where(subjectFootnote => subjectIds.Contains(subjectFootnote.SubjectId))
                        .ToList(),
                })
                .ToListAsync()
        ).Where(f => f.Filters.Any() || f.Indicators.Any() || f.FilterGroups.Any() || f.FilterItems.Any() || f.Subjects.Any()).OrderBy(f => f.Order).ThenBy(f => f.Id).ToList();
    }

    private IQueryable<Footnote> GetBaseFootnoteQuery()
    {
        return _context
            .Footnote.Include(footnote => footnote.Filters)
            .Include(footnote => footnote.FilterGroups)
            .Include(footnote => footnote.FilterItems)
            .Include(footnote => footnote.Indicators)
            .Include(footnote => footnote.Subjects);
    }

    public async Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseVersionId)
    {
        return await _context
            .ReleaseSubject.Where(releaseSubject => releaseSubject.ReleaseVersionId == releaseVersionId)
            .Where(releaseSubject =>
                !releaseSubject.Subject.Footnotes.Any()
                && !_context.FilterFootnote.Any(filterFootnote => filterFootnote.SubjectId == releaseSubject.SubjectId)
                && !_context.FilterGroupFootnote.Any(filterGroupFootnote =>
                    filterGroupFootnote.SubjectId == releaseSubject.SubjectId
                )
                && !_context.FilterItemFootnote.Any(filterItemFootnote =>
                    filterItemFootnote.SubjectId == releaseSubject.SubjectId
                )
                && !_context.IndicatorFootnote.Any(indicatorFootnote =>
                    indicatorFootnote.SubjectId == releaseSubject.SubjectId
                )
            )
            .Select(releaseSubject => releaseSubject.Subject)
            .ToListAsync();
    }

    private async Task<bool> IsFootnoteLinkedToAnotherSubjectForThisRelease(
        Guid footnoteId,
        Guid releaseVersionId,
        Guid subjectId
    )
    {
        var otherSubjectIds = await _context
            .ReleaseSubject.Where(rs => rs.ReleaseVersionId == releaseVersionId && rs.SubjectId != subjectId)
            .Select(rs => rs.SubjectId)
            .ToListAsync();

        var footnote = await GetFootnote(footnoteId);
        return footnote.Subjects.Any(subject => otherSubjectIds.Contains(subject.SubjectId))
            || footnote.Filters.Any(filter => otherSubjectIds.Contains(filter.SubjectId))
            || footnote.Indicators.Any(indicator => otherSubjectIds.Contains(indicator.SubjectId))
            || footnote.FilterGroups.Any(filterGroup => otherSubjectIds.Contains(filterGroup.SubjectId))
            || footnote.FilterItems.Any(filterItem => otherSubjectIds.Contains(filterItem.SubjectId));
    }
}
