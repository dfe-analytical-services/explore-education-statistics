#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.ComparerUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FootnoteService : IFootnoteService
{
    private readonly StatisticsDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
    private readonly IUserService _userService;
    private readonly IDataBlockService _dataBlockService;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly IReleaseSubjectRepository _releaseSubjectRepository;

    public FootnoteService(
        StatisticsDbContext context,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IUserService userService,
        IDataBlockService dataBlockService,
        IFootnoteRepository footnoteRepository,
        IReleaseSubjectRepository releaseSubjectRepository,
        IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper
    )
    {
        _context = context;
        _contentPersistenceHelper = contentPersistenceHelper;
        _userService = userService;
        _dataBlockService = dataBlockService;
        _footnoteRepository = footnoteRepository;
        _releaseSubjectRepository = releaseSubjectRepository;
        _statisticsPersistenceHelper = statisticsPersistenceHelper;
    }

    public async Task<Either<ActionResult, Footnote>> CreateFootnote(
        Guid releaseVersionId,
        string content,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds,
        IReadOnlySet<Guid> subjectIds
    )
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                GetFootnoteLinks(releaseVersionId, subjectIds, filterIds, filterGroupIds, filterItemIds, indicatorIds)
            )
            .OnSuccess(async footnoteLinks =>
            {
                var currentMaxOrder = await _context
                    .Footnote.Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                    .MaxAsync(footnote => (int?)footnote.Order);

                var order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                var footnote = await _footnoteRepository.CreateFootnote(
                    releaseVersionId,
                    content,
                    filterLinks: footnoteLinks.Filters,
                    filterGroupLinks: footnoteLinks.FilterGroups,
                    filterItemLinks: footnoteLinks.FilterItems,
                    indicatorLinks: footnoteLinks.Indicators,
                    subjectIds: subjectIds,
                    order
                );

                await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId);
                return await GetFootnote(releaseVersionId, footnote.Id);
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                _statisticsPersistenceHelper.CheckEntityExists<Footnote>(
                    footnoteId,
                    query => query.Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccessVoid(async footnote =>
            {
                await _footnoteRepository.DeleteFootnote(releaseVersionId: releaseVersionId, footnoteId: footnote.Id);

                await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId);
            });
    }

    public async Task<Either<ActionResult, Footnote>> UpdateFootnote(
        Guid releaseVersionId,
        Guid footnoteId,
        string content,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds,
        IReadOnlySet<Guid> subjectIds
    )
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ =>
                GetFootnoteLinks(releaseVersionId, subjectIds, filterIds, filterGroupIds, filterItemIds, indicatorIds)
            )
            .OnSuccessCombineWith(_ =>
                _statisticsPersistenceHelper.CheckEntityExists<Footnote>(
                    footnoteId,
                    query => HydrateFootnote(query).Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccess(async footnoteLinksAndFootnote =>
            {
                var (footnoteLinks, footnote) = footnoteLinksAndFootnote;

                _context.Update(footnote);

                footnote.Content = content;

                UpdateFilterLinks(footnote, footnoteLinks.Filters);
                UpdateFilterGroupLinks(footnote, footnoteLinks.FilterGroups);
                UpdateFilterItemLinks(footnote, footnoteLinks.FilterItems);
                UpdateIndicatorLinks(footnote, footnoteLinks.Indicators);
                UpdateSubjectLinks(footnote, subjectIds);

                await _context.SaveChangesAsync();
                return await _footnoteRepository.GetFootnote(footnoteId);
            })
            .OnSuccessDo(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId));
    }

    public async Task<Either<ActionResult, Unit>> UpdateFootnotes(Guid releaseVersionId, FootnotesUpdateRequest request)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() => ValidateFootnoteIdsForRelease(releaseVersionId, request.FootnoteIds))
            .OnSuccessVoid(async _ =>
            {
                // Set the order of each footnote based on the order observed in the request
                await request
                    .FootnoteIds.ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        async (footnoteId, index) =>
                        {
                            var footnote = await _context.Footnote.SingleAsync(footnote => footnote.Id == footnoteId);
                            _context.Update(footnote);
                            footnote.Order = index;
                        }
                    );
                await _context.SaveChangesAsync();
            })
            .OnSuccessVoid(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId));
    }

    public Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess<ActionResult, ReleaseVersion, Footnote>(async release =>
            {
                var footnote = await _footnoteRepository.GetFootnote(footnoteId);

                if (footnote.ReleaseVersionId != release.Id)
                {
                    return new NotFoundResult();
                }

                return footnote;
            });
    }

    public async Task<Either<ActionResult, List<Footnote>>> GetFootnotes(Guid releaseVersionId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ => await _footnoteRepository.GetFootnotes(releaseVersionId));
    }

    private List<SubjectFootnote> CreateSubjectLinks(Guid footnoteId, IReadOnlyCollection<Guid> subjectIds)
    {
        return subjectIds.Select(id => new SubjectFootnote { FootnoteId = footnoteId, SubjectId = id }).ToList();
    }

    private void UpdateFilterLinks(Footnote footnote, List<FilterFootnote> filterLinks)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.Filters.Select(link => link.FilterId),
                filterLinks.Select(link => link.FilterId)
            )
        )
        {
            footnote.Filters = filterLinks;
        }
    }

    private void UpdateFilterGroupLinks(Footnote footnote, List<FilterGroupFootnote> filterGroupLinks)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.FilterGroups.Select(link => link.FilterGroupId),
                filterGroupLinks.Select(link => link.FilterGroupId)
            )
        )
        {
            footnote.FilterGroups = filterGroupLinks;
        }
    }

    private void UpdateFilterItemLinks(Footnote footnote, List<FilterItemFootnote> filterItemLinks)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.FilterItems.Select(link => link.FilterItemId),
                filterItemLinks.Select(link => link.FilterItemId)
            )
        )
        {
            footnote.FilterItems = filterItemLinks;
        }
    }

    private void UpdateIndicatorLinks(Footnote footnote, List<IndicatorFootnote> indicatorLinks)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.Indicators.Select(link => link.IndicatorId),
                indicatorLinks.Select(link => link.IndicatorId)
            )
        )
        {
            footnote.Indicators = indicatorLinks;
        }
    }

    private void UpdateSubjectLinks(Footnote footnote, IReadOnlyCollection<Guid> subjectIds)
    {
        if (!SequencesAreEqualIgnoringOrder(footnote.Subjects.Select(link => link.SubjectId), subjectIds))
        {
            footnote.Subjects = CreateSubjectLinks(footnote.Id, subjectIds);
        }
    }

    private static IQueryable<Footnote> HydrateFootnote(IQueryable<Footnote> query)
    {
        return query
            .Include(f => f.Filters)
            .Include(f => f.FilterGroups)
            .Include(f => f.FilterItems)
            .Include(f => f.Indicators)
            .Include(f => f.Subjects);
    }

    private async Task<Either<ActionResult, Unit>> ValidateFootnoteIdsForRelease(
        Guid releaseVersionId,
        IEnumerable<Guid> footnoteIds
    )
    {
        var allReleaseFootnoteIds = await _context
            .Footnote.Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
            .Select(footnote => footnote.Id)
            .ToListAsync();

        if (!SequencesAreEqualIgnoringOrder(allReleaseFootnoteIds, footnoteIds))
        {
            return ValidationResult(ValidationErrorMessages.FootnotesDifferFromReleaseFootnotes);
        }

        return Unit.Instance;
    }

    /// <summary>
    /// Finds the subject of each of the given filters, filter groups, filter items and indicators among the
    /// release version's subjects, failing if any of them, or any of the given subjects, are not linked to the
    /// release version.
    /// </summary>
    private async Task<Either<ActionResult, FootnoteLinks>> GetFootnoteLinks(
        Guid releaseVersionId,
        IReadOnlySet<Guid> subjectIds,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds
    )
    {
        var releaseSubjects = await _releaseSubjectRepository.FindAll(releaseVersionId, HydrateReleaseSubjects);

        var filters = releaseSubjects.SelectMany(rs => rs.Subject.Filters).ToList();

        var footnoteLinks = new FootnoteLinks(
            Filters: filters
                .Where(filter => filterIds.Contains(filter.Id))
                .Select(filter => new FilterFootnote { FilterId = filter.Id, SubjectId = filter.SubjectId })
                .ToList(),
            FilterGroups: filters
                .SelectMany(
                    filter => filter.FilterGroups,
                    (filter, filterGroup) =>
                        new FilterGroupFootnote
                        {
                            FilterGroupId = filterGroup.Id,
                            FilterId = filter.Id,
                            SubjectId = filter.SubjectId,
                        }
                )
                .Where(link => filterGroupIds.Contains(link.FilterGroupId))
                .ToList(),
            FilterItems: filters
                .SelectMany(filter =>
                    filter.FilterGroups.SelectMany(
                        filterGroup => filterGroup.FilterItems,
                        (filterGroup, filterItem) =>
                            new FilterItemFootnote
                            {
                                FilterItemId = filterItem.Id,
                                FilterGroupId = filterGroup.Id,
                                FilterId = filter.Id,
                                SubjectId = filter.SubjectId,
                            }
                    )
                )
                .Where(link => filterItemIds.Contains(link.FilterItemId))
                .ToList(),
            Indicators: releaseSubjects
                .SelectMany(rs => rs.Subject.IndicatorGroups)
                .SelectMany(
                    indicatorGroup => indicatorGroup.Indicators,
                    (indicatorGroup, indicator) =>
                        new IndicatorFootnote
                        {
                            IndicatorId = indicator.Id,
                            IndicatorGroupId = indicatorGroup.Id,
                            SubjectId = indicatorGroup.SubjectId,
                        }
                )
                .Where(link => indicatorIds.Contains(link.IndicatorId))
                .ToList()
        );

        if (
            !releaseSubjects.Select(rs => rs.SubjectId).ContainsAll(subjectIds)
            || footnoteLinks.Filters.Count != filterIds.Count
            || footnoteLinks.FilterGroups.Count != filterGroupIds.Count
            || footnoteLinks.FilterItems.Count != filterItemIds.Count
            || footnoteLinks.Indicators.Count != indicatorIds.Count
        )
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return footnoteLinks;
    }

    private IQueryable<ReleaseSubject> HydrateReleaseSubjects(IQueryable<ReleaseSubject> queryable)
    {
        return queryable
            .Include(rs => rs.Subject)
                .ThenInclude(s => s.IndicatorGroups)
                    .ThenInclude(ig => ig.Indicators)
            .Include(rs => rs.Subject)
                .ThenInclude(s => s.Filters)
                    .ThenInclude(f => f.FilterGroups)
                        .ThenInclude(fg => fg.FilterItems);
    }

    /// <summary>
    /// The links from a footnote to each filter, filter group, filter item and indicator it applies to.
    /// </summary>
    private record FootnoteLinks(
        List<FilterFootnote> Filters,
        List<FilterGroupFootnote> FilterGroups,
        List<FilterItemFootnote> FilterItems,
        List<IndicatorFootnote> Indicators
    );
}
