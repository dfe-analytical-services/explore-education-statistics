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
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
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
    private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper; // for Footnotes only
    private readonly IUserService _userService;
    private readonly IDataBlockService _dataBlockService;
    private readonly IFootnoteRepository _footnoteRepository;
    private readonly IReleaseSubjectRepository _releaseSubjectRepository;
    private readonly IStorageDataSetResolver _storageDataSetResolver;

    public FootnoteService(
        StatisticsDbContext context,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        IUserService userService,
        IDataBlockService dataBlockService,
        IFootnoteRepository footnoteRepository,
        IReleaseSubjectRepository releaseSubjectRepository,
        IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
        IStorageDataSetResolver storageDataSetResolver
    )
    {
        _context = context;
        _contentPersistenceHelper = contentPersistenceHelper;
        _userService = userService;
        _dataBlockService = dataBlockService;
        _footnoteRepository = footnoteRepository;
        _releaseSubjectRepository = releaseSubjectRepository;
        _statisticsPersistenceHelper = statisticsPersistenceHelper;
        _storageDataSetResolver = storageDataSetResolver;
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
                var releaseHasFootnotes = await _context.ReleaseFootnote.AnyAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                );

                var order = releaseHasFootnotes
                    ? await _context
                        .ReleaseFootnote.Include(rf => rf.Footnote)
                        .Where(rf => rf.ReleaseVersionId == releaseVersionId)
                        .MaxAsync(rf => rf.Footnote.Order) + 1
                    : 0;

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
            .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId))
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
                _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId, HydrateFootnote)
            )
            .OnSuccess(async linkSubjectIdsAndFootnote =>
            {
                var (linkSubjectIds, footnote) = linkSubjectIdsAndFootnote;

                // NOTE: At the time of writing, footnotes are now always exclusive to a particular release, but
                // in the past this wasn't always the case.
                // TODO EES-2979 Remove this check once all footnotes only belong to one release
                if (
                    await _footnoteRepository.IsFootnoteExclusiveToRelease(
                        releaseVersionId: releaseVersionId,
                        footnoteId: footnote.Id
                    )
                )
                {
                    _context.Update(footnote);

                    footnote.Content = content;

                    UpdateFilterLinks(footnote, linkSubjectIds.Filters);
                    UpdateFilterGroupLinks(footnote, linkSubjectIds.FilterGroups);
                    UpdateFilterItemLinks(footnote, linkSubjectIds.FilterItems);
                    UpdateIndicatorLinks(footnote, linkSubjectIds.Indicators);
                    UpdateSubjectLinks(footnote, subjectIds);

                    await _context.SaveChangesAsync();
                    return await _footnoteRepository.GetFootnote(footnoteId);
                }

                // TODO EES-2979 Remove this delete link and create call once all footnotes only belong to one
                // release If this amendment of the footnote affects other release then break the link with the old
                // and create a new one
                await _footnoteRepository.DeleteReleaseFootnoteLink(
                    releaseVersionId: releaseVersionId,
                    footnoteId: footnote.Id
                );

                return await _footnoteRepository.CreateFootnote(
                    releaseVersionId,
                    content,
                    filterSubjectIds: linkSubjectIds.Filters,
                    filterGroupSubjectIds: linkSubjectIds.FilterGroups,
                    filterItemSubjectIds: linkSubjectIds.FilterItems,
                    indicatorSubjectIds: linkSubjectIds.Indicators,
                    subjectIds: subjectIds,
                    footnote.Order
                );
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

                if (footnote.Releases.All(rf => rf.ReleaseVersionId != release.Id))
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
            .ReleaseFootnote.Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Select(rf => rf.FootnoteId)
            .ToListAsync();

        if (!SequencesAreEqualIgnoringOrder(allReleaseFootnoteIds, footnoteIds))
        {
            return ValidationResult(ValidationErrorMessages.FootnotesDifferFromReleaseFootnotes);
        }

        return Unit.Instance;
    }

    /// <summary>
    /// Finds the subject of each of the given filters, filter groups, filter items and indicators among the
    /// release version's data sets, failing validation if any of them, or any of the given subjects, are not
    /// linked to the release version.
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
        var releaseVersionSubjectIds = (await _releaseSubjectRepository.FindAll(releaseVersionId))
            .Select(rs => rs.SubjectId)
            .ToList();

        var unlinkedSubjectIds = subjectIds.Except(releaseVersionSubjectIds).ToList();
        if (unlinkedSubjectIds.Count > 0)
        {
            return ValidationResult(
                ValidationMessages.GenerateErrorFootnoteSubjectNotAttachedToRelease(
                    releaseVersionId,
                    unlinkedSubjectIds
                )
            );
        }

        var linkSubjectIds = new LinkSubjectIds(
            Filters: new Dictionary<Guid, Guid>(),
            FilterGroups: new Dictionary<Guid, Guid>(),
            FilterItems: new Dictionary<Guid, Guid>(),
            Indicators: new Dictionary<Guid, Guid>()
        );

        // We look for these specified ids in the release's datasets, stopping when all have been found
        var unlinkedFilterIds = filterIds.ToHashSet();
        var unlinkedFilterGroupIds = filterGroupIds.ToHashSet();
        var unlinkedFilterItemIds = filterItemIds.ToHashSet();
        var unlinkedIndicatorIds = indicatorIds.ToHashSet();

        if (AllLinked())
        {
            return linkSubjectIds;
        }

        foreach (var subjectId in releaseVersionSubjectIds)
        {
            var dataSet = await _storageDataSetResolver.Resolve(subjectId);

            if (unlinkedFilterGroupIds.Count > 0 || unlinkedFilterItemIds.Count > 0)
            {
                var filters = await dataSet.ListFilters(includeItems: true);
                var filterGroups = filters.SelectMany(f => f.FilterGroups).ToList();

                Link(filters.Select(f => f.Id), unlinkedFilterIds, linkSubjectIds.Filters, subjectId);
                Link(filterGroups.Select(fg => fg.Id), unlinkedFilterGroupIds, linkSubjectIds.FilterGroups, subjectId);
                Link(
                    filterGroups.SelectMany(fg => fg.FilterItems).Select(fi => fi.Id),
                    unlinkedFilterItemIds,
                    linkSubjectIds.FilterItems,
                    subjectId
                );
            }
            else if (unlinkedFilterIds.Count > 0)
            {
                var filters = await dataSet.ListFilters();

                Link(filters.Select(f => f.Id), unlinkedFilterIds, linkSubjectIds.Filters, subjectId);
            }

            if (unlinkedIndicatorIds.Count > 0)
            {
                var indicatorGroups = await dataSet.ListIndicatorGroups();

                Link(
                    indicatorGroups.SelectMany(ig => ig.Indicators).Select(i => i.Id),
                    unlinkedIndicatorIds,
                    linkSubjectIds.Indicators,
                    subjectId
                );
            }

            if (AllLinked())
            {
                break;
            }
        }

        if (!AllLinked())
        {
            return ValidationResult(
                ValidationMessages.GenerateErrorFootnoteFilterOrIndicatorNotAttachedToRelease(
                    releaseVersionId,
                    unlinkedFilterIds,
                    unlinkedFilterGroupIds,
                    unlinkedFilterItemIds,
                    unlinkedIndicatorIds
                )
            );
        }

        return linkSubjectIds;

        bool AllLinked() =>
            unlinkedFilterIds.Count == 0
            && unlinkedFilterGroupIds.Count == 0
            && unlinkedFilterItemIds.Count == 0
            && unlinkedIndicatorIds.Count == 0;

        static void Link(
            IEnumerable<Guid> dataSetIds,
            HashSet<Guid> unlinkedIds,
            Dictionary<Guid, Guid> subjectIdsById,
            Guid subjectId
        )
        {
            foreach (var id in dataSetIds)
            {
                if (unlinkedIds.Remove(id))
                {
                    subjectIdsById[id] = subjectId;
                }
            }
        }
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
