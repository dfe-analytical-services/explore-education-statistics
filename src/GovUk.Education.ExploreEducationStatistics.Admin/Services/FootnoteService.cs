#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Http;
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
        IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper)
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
        IReadOnlySet<Guid> subjectIds)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccessDo(() => CheckSubjectsFiltersAndIndicatorsAreLinkedToRelease(
                releaseVersionId,
                subjectIds,
                filterIds,
                filterGroupIds,
                filterItemIds,
                indicatorIds))
            .OnSuccess(async () =>
            {
                var releaseHasFootnotes = await _context.ReleaseFootnote
                    .AnyAsync(rf => rf.ReleaseVersionId == releaseVersionId);

                var order = releaseHasFootnotes
                    ? await _context.ReleaseFootnote
                        .Include(rf => rf.Footnote)
                        .Where(rf => rf.ReleaseVersionId == releaseVersionId)
                        .MaxAsync(rf => rf.Footnote.Order) + 1
                    : 0;

                var footnote = await _footnoteRepository.CreateFootnote(
                    releaseVersionId,
                    content,
                    filterIds: filterIds,
                    filterGroupIds: filterGroupIds,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds,
                    subjectIds: subjectIds,
                    order);

                await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId);
                return await GetFootnote(releaseVersionId, footnote.Id);
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId))
            .OnSuccessVoid(async footnote =>
            {
                await _footnoteRepository.DeleteFootnote(releaseVersionId: releaseVersionId,
                    footnoteId: footnote.Id);

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
        IReadOnlySet<Guid> subjectIds)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => CheckSubjectsFiltersAndIndicatorsAreLinkedToRelease(
                releaseVersionId,
                subjectIds,
                filterIds,
                filterGroupIds,
                filterItemIds,
                indicatorIds))
            .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(footnoteId, HydrateFootnote))
            .OnSuccess(async footnote =>
            {
                // NOTE: At the time of writing, footnotes are now always exclusive to a particular release, but
                // in the past this wasn't always the case.
                // TODO EES-2979 Remove this check once all footnotes only belong to one release
                if (await _footnoteRepository.IsFootnoteExclusiveToRelease(releaseVersionId: releaseVersionId,
                        footnoteId: footnote.Id))
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

                // TODO EES-2979 Remove this delete link and create call once all footnotes only belong to one
                // release If this amendment of the footnote affects other release then break the link with the old
                // and create a new one
                await _footnoteRepository.DeleteReleaseFootnoteLink(releaseVersionId: releaseVersionId,
                    footnoteId: footnote.Id);

                return await _footnoteRepository.CreateFootnote(
                    releaseVersionId,
                    content,
                    filterIds: filterIds,
                    filterGroupIds: filterGroupIds,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds,
                    subjectIds: subjectIds,
                    footnote.Order);
            })
            .OnSuccessDo(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId));
    }

    public async Task<Either<ActionResult, Unit>> UpdateFootnotes(Guid releaseVersionId,
        FootnotesUpdateRequest request)
    {
        return await _contentPersistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() => ValidateFootnoteIdsForRelease(releaseVersionId, request.FootnoteIds))
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
            .OnSuccessVoid(async _ => await _dataBlockService.InvalidateCachedDataBlocks(releaseVersionId));
    }

    public Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess<ActionResult, ReleaseVersion, Footnote>(async release =>
                {
                    var footnote = await _footnoteRepository.GetFootnote(footnoteId);

                    if (footnote.Releases.All(rf => rf.ReleaseVersionId != release.Id))
                    {
                        return new NotFoundResult();
                    }

                    return footnote;
                }
            );
    }

    public async Task<Either<ActionResult, List<Footnote>>> GetFootnotes(Guid releaseVersionId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ => await _footnoteRepository.GetFootnotes(releaseVersionId));
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
        Guid releaseVersionId,
        IEnumerable<Guid> footnoteIds)
    {
        var allReleaseFootnoteIds = await _context.ReleaseFootnote
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Select(rf => rf.FootnoteId)
            .ToListAsync();

        if (!SequencesAreEqualIgnoringOrder(allReleaseFootnoteIds, footnoteIds))
        {
            return ValidationResult(ValidationErrorMessages.FootnotesDifferFromReleaseFootnotes);
        }

        return Unit.Instance;
    }

    private async Task<Either<ActionResult, Unit>> CheckSubjectsFiltersAndIndicatorsAreLinkedToRelease(
        Guid releaseVersionId,
        IReadOnlySet<Guid> subjectIds,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds)
    {
        var releaseSubjects = await _releaseSubjectRepository.FindAll(
            releaseVersionId,
            HydrateReleaseSubjects);

        if (!AllSpecifiedSubjectsAreLinkedToRelease(subjectIds, releaseSubjects)
            || !AllSpecifiedFiltersAreLinkedToRelease(filterIds, releaseSubjects)
            || !AllSpecifiedFilterGroupsAreLinkedToRelease(filterGroupIds, releaseSubjects)
            || !AllSpecifiedFilterItemsAreLinkedToRelease(filterItemIds, releaseSubjects)
            || !AllSpecifiedIndicatorsAreLinkedToRelease(indicatorIds, releaseSubjects))
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return Unit.Instance;
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

    private static bool AllSpecifiedSubjectsAreLinkedToRelease(
        IReadOnlySet<Guid> subjectIds,
        IReadOnlyList<ReleaseSubject> releaseSubjects)
    {
        if (!subjectIds.Any())
        {
            return true;
        }

        IReadOnlyList<Guid> releaseSubjectIds = releaseSubjects
            .Select(rs => rs.SubjectId)
            .ToList();

        return releaseSubjectIds.ContainsAll(subjectIds);
    }

    private static bool AllSpecifiedFiltersAreLinkedToRelease(
        IReadOnlySet<Guid> filterIds,
        IReadOnlyList<ReleaseSubject> releaseSubjects)
    {
        if (!filterIds.Any())
        {
            return true;
        }

        IReadOnlyList<Guid> releaseFilterIds = releaseSubjects
            .SelectMany(rs => rs.Subject.Filters)
            .Select(f => f.Id)
            .ToList();

        return releaseFilterIds.ContainsAll(filterIds);
    }

    private static bool AllSpecifiedFilterGroupsAreLinkedToRelease(
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlyList<ReleaseSubject> releaseSubjects)
    {
        if (!filterGroupIds.Any())
        {
            return true;
        }

        IReadOnlyList<Guid> releaseFilterGroupIds = releaseSubjects
            .SelectMany(rs => rs.Subject.Filters)
            .SelectMany(f => f.FilterGroups)
            .Select(fg => fg.Id)
            .ToList();

        return releaseFilterGroupIds.ContainsAll(filterGroupIds);
    }

    private static bool AllSpecifiedFilterItemsAreLinkedToRelease(
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlyList<ReleaseSubject> releaseSubjects)
    {
        if (!filterItemIds.Any())
        {
            return true;
        }

        IReadOnlyList<Guid> releaseFilterItemIds = releaseSubjects
            .SelectMany(rs => rs.Subject.Filters)
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .Select(fi => fi.Id)
            .ToList();

        return releaseFilterItemIds.ContainsAll(filterItemIds);
    }

    private static bool AllSpecifiedIndicatorsAreLinkedToRelease(
        IReadOnlySet<Guid> indicatorIds,
        IReadOnlyList<ReleaseSubject> releaseSubjects)
    {
        if (!indicatorIds.Any())
        {
            return true;
        }

        IReadOnlyList<Guid> releaseIndicatorIds = releaseSubjects
            .SelectMany(rs => rs.Subject.IndicatorGroups)
            .SelectMany(ig => ig.Indicators)
            .Select(i => i.Id)
            .ToList();

        return releaseIndicatorIds.ContainsAll(indicatorIds);
    }
}
