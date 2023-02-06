#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
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
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FootnoteService : IFootnoteService
    {
        private readonly StatisticsDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;
        private readonly IDataBlockService _dataBlockService;
        private readonly IFootnoteRepository _footnoteRepository;

        public FootnoteService(
            StatisticsDbContext context,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService,
            IDataBlockService dataBlockService,
            IFootnoteRepository footnoteRepository,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper)
        {
            _context = context;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _dataBlockService = dataBlockService;
            _footnoteRepository = footnoteRepository;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
        }

        public async Task<Either<ActionResult, Footnote>> CreateFootnote(
            Guid releaseId,
            string content,
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    var releaseHasFootnotes = await _context.ReleaseFootnote
                        .AnyAsync(rf => rf.ReleaseId == releaseId);

                    var order = releaseHasFootnotes
                        ? await _context.ReleaseFootnote
                            .Include(rf => rf.Footnote)
                            .Where(rf => rf.ReleaseId == releaseId)
                            .MaxAsync(rf => rf.Footnote.Order) + 1
                        : 0;

                    var footnote = await _footnoteRepository.CreateFootnote(
                        releaseId,
                        content,
                        filterIds: filterIds,
                        filterGroupIds: filterGroupIds,
                        filterItemIds: filterItemIds,
                        indicatorIds: indicatorIds,
                        subjectIds: subjectIds,
                        order);

                    await _dataBlockService.InvalidateCachedDataBlocks(releaseId);
                    return await GetFootnote(releaseId, footnote.Id);
                });
        }

        public async Task<Either<ActionResult, List<Footnote>>> CopyFootnotes(Guid sourceReleaseId,
            Guid destinationReleaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(destinationReleaseId)
                .OnSuccessDo(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => GetFootnotes(sourceReleaseId))
                .OnSuccess(async footnotes =>
                {
                    return await footnotes
                        .ToAsyncEnumerable()
                        .SelectAwait(async footnote =>
                        {
                            var filterIds = footnote.Filters
                                .Select(filterFootnote => filterFootnote.FilterId).ToHashSet();
                            var filterGroupIds = footnote.FilterGroups
                                .Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId).ToHashSet();
                            var filterItemIds = footnote.FilterItems
                                .Select(filterItemFootnote => filterItemFootnote.FilterItemId).ToHashSet();
                            var indicatorIds = footnote.Indicators
                                .Select(indicatorFootnote => indicatorFootnote.IndicatorId).ToHashSet();
                            var subjectIds = footnote.Subjects
                                .Select(subjectFootnote => subjectFootnote.SubjectId).ToHashSet();

                            return await _footnoteRepository.CreateFootnote(destinationReleaseId,
                                footnote.Content,
                                filterIds: filterIds,
                                filterGroupIds: filterGroupIds,
                                filterItemIds: filterItemIds,
                                indicatorIds: indicatorIds,
                                subjectIds: subjectIds,
                                footnote.Order);
                        })
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseId, Guid footnoteId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId)
                .OnSuccessVoid(async footnote =>
                {
                    await _footnoteRepository.DeleteFootnote(releaseId: releaseId,
                        footnoteId: footnote.Id);

                    await _dataBlockService.InvalidateCachedDataBlocks(releaseId);
                }));
        }

        public async Task<Either<ActionResult, Footnote>> UpdateFootnote(
            Guid releaseId,
            Guid footnoteId,
            string content,
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId, HydrateFootnote)
                .OnSuccess(async footnote =>
                {
                    // NOTE: At the time of writing, footnotes are now always exclusive to a particular release, but
                    // in the past this wasn't always the case.
                    // TODO EES-2979 Remove this check once all footnotes only belong to one release
                    if (await _footnoteRepository.IsFootnoteExclusiveToRelease(releaseId, footnote.Id))
                    {
                        _context.Update(footnote);

                        footnote.Content = content;

                        UpdateFilterLinks(footnote, filterIds);
                        UpdateFilterGroupLinks(footnote, filterGroupIds);
                        UpdateFilterItemLinks(footnote, filterItemIds);
                        UpdateIndicatorLinks(footnote, indicatorIds);
                        UpdateSubjectLinks(footnote, subjectIds);

                        await _context.SaveChangesAsync();
                        return await _footnoteRepository.GetFootnote(footnoteId);
                    }

                    // TODO EES-2979 Remove this delete link and create call once all footnotes only belong to one release
                    // If this amendment of the footnote affects other release then break the link with the old
                    // and create a new one
                    await _footnoteRepository.DeleteReleaseFootnoteLink(releaseId, footnote.Id);

                    return await _footnoteRepository.CreateFootnote(
                        releaseId,
                        content,
                        filterIds: filterIds,
                        filterGroupIds: filterGroupIds,
                        filterItemIds: filterItemIds,
                        indicatorIds: indicatorIds,
                        subjectIds: subjectIds,
                        footnote.Order);
                    })
                    .OnSuccessDo(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseId)));
        }

        public async Task<Either<ActionResult, Unit>> UpdateFootnotes(Guid releaseId,
            FootnotesUpdateRequest request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(() => ValidateFootnoteIdsForRelease(releaseId, request.FootnoteIds))
                .OnSuccessVoid(async _ =>
                {
                    // Set the order of each footnote based on the order observed in the request
                    await request.FootnoteIds
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(async (footnoteId, index) =>
                        {
                            var footnote = await _context.Footnote.SingleAsync(footnote => footnote.Id == footnoteId);
                            _context.Update(footnote);
                            footnote.Order = index;
                        });
                    await _context.SaveChangesAsync();
                })
                .OnSuccessVoid(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseId));
        }

        public Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseId, Guid footnoteId)
        {
            return _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess<ActionResult, Release, Footnote>(async release =>
                    {
                        var footnote = await _footnoteRepository.GetFootnote(footnoteId);

                        if (footnote.Releases.All(rf => rf.ReleaseId != release.Id))
                        {
                            return new NotFoundResult();
                        }

                        return footnote;
                    }
                );
        }

        public async Task<Either<ActionResult, List<Footnote>>> GetFootnotes(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ => await _footnoteRepository.GetFootnotes(releaseId));
        }

        private List<SubjectFootnote> CreateSubjectLinks(Guid footnoteId,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return subjectIds
                .Select(id =>
                    new SubjectFootnote
                    {
                        FootnoteId = footnoteId,
                        SubjectId = id
                    })
                .ToList();
        }

        private List<FilterFootnote> CreateFilterLinks(Guid footnoteId,
            IReadOnlyCollection<Guid> filterIds)
        {
            return filterIds.Select(id =>
                new FilterFootnote
                {
                    FootnoteId = footnoteId,
                    FilterId = id
                })
                .ToList();
        }

        private List<FilterGroupFootnote> CreateFilterGroupLinks(Guid footnoteId,
            IReadOnlyCollection<Guid> filterGroupIds)
        {
            return filterGroupIds.Select(id =>
                new FilterGroupFootnote
                {
                    FootnoteId = footnoteId,
                    FilterGroupId = id
                })
                .ToList();
        }

        private List<FilterItemFootnote> CreateFilterItemLinks(Guid footnoteId,
            IReadOnlyCollection<Guid> filterItemIds)
        {
            return filterItemIds.Select(id =>
                new FilterItemFootnote
                {
                    FootnoteId = footnoteId,
                    FilterItemId = id
                })
                .ToList();
        }

        private List<IndicatorFootnote> CreateIndicatorsLinks(Guid footnoteId,
            IReadOnlyCollection<Guid> indicatorIds)
        {
            return indicatorIds.Select(id =>
                new IndicatorFootnote
                {
                    FootnoteId = footnoteId,
                    IndicatorId = id
                })
                .ToList();
        }

        private void UpdateFilterLinks(Footnote footnote, IReadOnlyCollection<Guid> filterIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                    footnote.Filters.Select(link => link.FilterId), filterIds))
            {
                footnote.Filters = CreateFilterLinks(footnote.Id, filterIds);
            }
        }

        private void UpdateFilterGroupLinks(Footnote footnote, IReadOnlyCollection<Guid> filterGroupIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                    footnote.FilterGroups.Select(link => link.FilterGroupId), filterGroupIds))
            {
                footnote.FilterGroups = CreateFilterGroupLinks(footnote.Id, filterGroupIds);
            }
        }

        private void UpdateFilterItemLinks(Footnote footnote, IReadOnlyCollection<Guid> filterItemIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                    footnote.FilterItems.Select(link => link.FilterItemId), filterItemIds))
            {
                footnote.FilterItems = CreateFilterItemLinks(footnote.Id, filterItemIds);
            }
        }

        private void UpdateIndicatorLinks(Footnote footnote, IReadOnlyCollection<Guid> indicatorIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                    footnote.Indicators.Select(link => link.IndicatorId), indicatorIds))
            {
                footnote.Indicators = CreateIndicatorsLinks(footnote.Id, indicatorIds);
            }
        }

        private void UpdateSubjectLinks(Footnote footnote, IReadOnlyCollection<Guid> subjectIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                    footnote.Subjects.Select(link => link.SubjectId), subjectIds))
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
            Guid releaseId,
            IEnumerable<Guid> footnoteIds)
        {
            var allReleaseFootnoteIds = await _context.ReleaseFootnote
                .Where(rf => rf.ReleaseId == releaseId)
                .Select(rf => rf.FootnoteId)
                .ToListAsync();

            if (!SequencesAreEqualIgnoringOrder(allReleaseFootnoteIds, footnoteIds))
            {
                return ValidationResult(ValidationErrorMessages.FootnotesDifferFromReleaseFootnotes);
            }

            return Unit.Instance;
        }
    }
}
