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
    private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
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
                CreateFootnoteLinks(
                    releaseVersionId,
                    subjectIds,
                    filterIds,
                    filterGroupIds,
                    filterItemIds,
                    indicatorIds
                )
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
                CreateFootnoteLinks(
                    releaseVersionId,
                    subjectIds,
                    filterIds,
                    filterGroupIds,
                    filterItemIds,
                    indicatorIds
                )
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

                footnote.Filters = footnoteLinks.Filters;
                footnote.FilterGroups = footnoteLinks.FilterGroups;
                footnote.FilterItems = footnoteLinks.FilterItems;
                footnote.Indicators = footnoteLinks.Indicators;
                footnote.Subjects = subjectIds
                    .Select(subjectId => new SubjectFootnote { SubjectId = subjectId })
                    .ToList();

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

    private async Task<Either<ActionResult, FootnoteLinks>> CreateFootnoteLinks(
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

        var footnoteLinks = new FootnoteLinks(Filters: [], FilterGroups: [], FilterItems: [], Indicators: []);

        // We look for these specified ids in the release's datasets, stopping when all have been found
        var unlinkedFilterIds = filterIds.ToHashSet();
        var unlinkedFilterGroupIds = filterGroupIds.ToHashSet();
        var unlinkedFilterItemIds = filterItemIds.ToHashSet();
        var unlinkedIndicatorIds = indicatorIds.ToHashSet();

        if (AllLinked())
        {
            return footnoteLinks;
        }

        foreach (var subjectId in releaseVersionSubjectIds)
        {
            var dataSet = await _storageDataSetResolver.Resolve(subjectId);

            if (unlinkedFilterGroupIds.Count > 0 || unlinkedFilterItemIds.Count > 0)
            {
                var filters = await dataSet.ListFilters(includeItems: true);

                foreach (var filter in filters)
                {
                    LinkFilter(filter, subjectId);

                    foreach (var filterGroup in filter.FilterGroups)
                    {
                        if (unlinkedFilterGroupIds.Remove(filterGroup.Id))
                        {
                            footnoteLinks.FilterGroups.Add(
                                new FilterGroupFootnote
                                {
                                    FilterGroupId = filterGroup.Id,
                                    FilterId = filter.Id,
                                    SubjectId = subjectId,
                                }
                            );
                        }

                        foreach (var filterItem in filterGroup.FilterItems)
                        {
                            if (unlinkedFilterItemIds.Remove(filterItem.Id))
                            {
                                footnoteLinks.FilterItems.Add(
                                    new FilterItemFootnote
                                    {
                                        FilterItemId = filterItem.Id,
                                        FilterGroupId = filterGroup.Id,
                                        FilterId = filter.Id,
                                        SubjectId = subjectId,
                                    }
                                );
                            }
                        }
                    }
                }
            }
            else if (unlinkedFilterIds.Count > 0)
            {
                var filters = await dataSet.ListFilters();

                foreach (var filter in filters)
                {
                    LinkFilter(filter, subjectId);
                }
            }

            if (unlinkedIndicatorIds.Count > 0)
            {
                var indicatorGroups = await dataSet.ListIndicatorGroups();

                foreach (var indicatorGroup in indicatorGroups)
                {
                    foreach (var indicator in indicatorGroup.Indicators)
                    {
                        if (unlinkedIndicatorIds.Remove(indicator.Id))
                        {
                            footnoteLinks.Indicators.Add(
                                new IndicatorFootnote
                                {
                                    IndicatorId = indicator.Id,
                                    IndicatorGroupId = indicatorGroup.Id,
                                    SubjectId = subjectId,
                                }
                            );
                        }
                    }
                }
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

        return footnoteLinks;

        bool AllLinked() =>
            unlinkedFilterIds.Count == 0
            && unlinkedFilterGroupIds.Count == 0
            && unlinkedFilterItemIds.Count == 0
            && unlinkedIndicatorIds.Count == 0;

        void LinkFilter(Filter filter, Guid subjectId)
        {
            if (unlinkedFilterIds.Remove(filter.Id))
            {
                footnoteLinks.Filters.Add(new FilterFootnote { FilterId = filter.Id, SubjectId = subjectId });
            }
        }
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
