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
                GetLinkSubjectIds(releaseVersionId, subjectIds, filterIds, filterGroupIds, filterItemIds, indicatorIds)
            )
            .OnSuccess(async linkSubjectIds =>
            {
                var currentMaxOrder = await _context
                    .Footnote.Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                    .MaxAsync(footnote => (int?)footnote.Order);

                var order = currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;

                var footnote = await _footnoteRepository.CreateFootnote(
                    releaseVersionId,
                    content,
                    filterSubjectIds: linkSubjectIds.Filters,
                    filterGroupSubjectIds: linkSubjectIds.FilterGroups,
                    filterItemSubjectIds: linkSubjectIds.FilterItems,
                    indicatorSubjectIds: linkSubjectIds.Indicators,
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
                GetLinkSubjectIds(releaseVersionId, subjectIds, filterIds, filterGroupIds, filterItemIds, indicatorIds)
            )
            .OnSuccessCombineWith(_ =>
                _statisticsPersistenceHelper.CheckEntityExists<Footnote>(
                    footnoteId,
                    query => HydrateFootnote(query).Where(footnote => footnote.ReleaseVersionId == releaseVersionId)
                )
            )
            .OnSuccess(async linkSubjectIdsAndFootnote =>
            {
                var (linkSubjectIds, footnote) = linkSubjectIdsAndFootnote;

                _context.Update(footnote);

                footnote.Content = content;

                UpdateFilterLinks(footnote, linkSubjectIds.Filters);
                UpdateFilterGroupLinks(footnote, linkSubjectIds.FilterGroups);
                UpdateFilterItemLinks(footnote, linkSubjectIds.FilterItems);
                UpdateIndicatorLinks(footnote, linkSubjectIds.Indicators);
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

    private List<FilterFootnote> CreateFilterLinks(Guid footnoteId, IReadOnlyDictionary<Guid, Guid> filterSubjectIds)
    {
        return filterSubjectIds
            .Select(pair => new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = pair.Key,
                SubjectId = pair.Value,
            })
            .ToList();
    }

    private List<FilterGroupFootnote> CreateFilterGroupLinks(
        Guid footnoteId,
        IReadOnlyDictionary<Guid, Guid> filterGroupSubjectIds
    )
    {
        return filterGroupSubjectIds
            .Select(pair => new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = pair.Key,
                SubjectId = pair.Value,
            })
            .ToList();
    }

    private List<FilterItemFootnote> CreateFilterItemLinks(
        Guid footnoteId,
        IReadOnlyDictionary<Guid, Guid> filterItemSubjectIds
    )
    {
        return filterItemSubjectIds
            .Select(pair => new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = pair.Key,
                SubjectId = pair.Value,
            })
            .ToList();
    }

    private List<IndicatorFootnote> CreateIndicatorsLinks(
        Guid footnoteId,
        IReadOnlyDictionary<Guid, Guid> indicatorSubjectIds
    )
    {
        return indicatorSubjectIds
            .Select(pair => new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = pair.Key,
                SubjectId = pair.Value,
            })
            .ToList();
    }

    private void UpdateFilterLinks(Footnote footnote, IReadOnlyDictionary<Guid, Guid> filterSubjectIds)
    {
        if (!SequencesAreEqualIgnoringOrder(footnote.Filters.Select(link => link.FilterId), filterSubjectIds.Keys))
        {
            footnote.Filters = CreateFilterLinks(footnote.Id, filterSubjectIds);
        }
    }

    private void UpdateFilterGroupLinks(Footnote footnote, IReadOnlyDictionary<Guid, Guid> filterGroupSubjectIds)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.FilterGroups.Select(link => link.FilterGroupId),
                filterGroupSubjectIds.Keys
            )
        )
        {
            footnote.FilterGroups = CreateFilterGroupLinks(footnote.Id, filterGroupSubjectIds);
        }
    }

    private void UpdateFilterItemLinks(Footnote footnote, IReadOnlyDictionary<Guid, Guid> filterItemSubjectIds)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.FilterItems.Select(link => link.FilterItemId),
                filterItemSubjectIds.Keys
            )
        )
        {
            footnote.FilterItems = CreateFilterItemLinks(footnote.Id, filterItemSubjectIds);
        }
    }

    private void UpdateIndicatorLinks(Footnote footnote, IReadOnlyDictionary<Guid, Guid> indicatorSubjectIds)
    {
        if (
            !SequencesAreEqualIgnoringOrder(
                footnote.Indicators.Select(link => link.IndicatorId),
                indicatorSubjectIds.Keys
            )
        )
        {
            footnote.Indicators = CreateIndicatorsLinks(footnote.Id, indicatorSubjectIds);
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
    private async Task<Either<ActionResult, LinkSubjectIds>> GetLinkSubjectIds(
        Guid releaseVersionId,
        IReadOnlySet<Guid> subjectIds,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds
    )
    {
        var releaseSubjects = await _releaseSubjectRepository.FindAll(releaseVersionId, HydrateReleaseSubjects);

        var linkSubjectIds = new LinkSubjectIds(
            Filters: FindSubjectIds(filterIds, subject => subject.Filters.Select(f => f.Id)),
            FilterGroups: FindSubjectIds(
                filterGroupIds,
                subject => subject.Filters.SelectMany(f => f.FilterGroups).Select(fg => fg.Id)
            ),
            FilterItems: FindSubjectIds(
                filterItemIds,
                subject =>
                    subject.Filters.SelectMany(f => f.FilterGroups).SelectMany(fg => fg.FilterItems).Select(fi => fi.Id)
            ),
            Indicators: FindSubjectIds(
                indicatorIds,
                subject => subject.IndicatorGroups.SelectMany(ig => ig.Indicators).Select(i => i.Id)
            )
        );

        if (
            !releaseSubjects.Select(rs => rs.SubjectId).ContainsAll(subjectIds)
            || linkSubjectIds.Filters.Count != filterIds.Count
            || linkSubjectIds.FilterGroups.Count != filterGroupIds.Count
            || linkSubjectIds.FilterItems.Count != filterItemIds.Count
            || linkSubjectIds.Indicators.Count != indicatorIds.Count
        )
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return linkSubjectIds;

        Dictionary<Guid, Guid> FindSubjectIds(
            IReadOnlySet<Guid> ids,
            Func<Subject, IEnumerable<Guid>> subjectIdsSelector
        ) =>
            releaseSubjects
                .SelectMany(rs =>
                    subjectIdsSelector(rs.Subject).Where(ids.Contains).Select(id => (Id: id, rs.SubjectId))
                )
                .ToDictionary(link => link.Id, link => link.SubjectId);
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
    /// The subject id of each filter, filter group, filter item and indicator a footnote is linked to, keyed by
    /// their ids.
    /// </summary>
    private record LinkSubjectIds(
        Dictionary<Guid, Guid> Filters,
        Dictionary<Guid, Guid> FilterGroups,
        Dictionary<Guid, Guid> FilterItems,
        Dictionary<Guid, Guid> Indicators
    );
}
