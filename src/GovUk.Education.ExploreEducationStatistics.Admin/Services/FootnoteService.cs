using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FootnoteService : IFootnoteService
    {
        private readonly StatisticsDbContext _context;
        private readonly IFilterService _filterService;
        private readonly IFilterGroupService _filterGroupService;
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorService _indicatorService;
        private readonly ISubjectService _subjectService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;
        private readonly IFootnoteRepository _footnoteRepository;

        public FootnoteService(
            StatisticsDbContext context,
            IFilterService filterService,
            IFilterGroupService filterGroupService,
            IFilterItemService filterItemService,
            IIndicatorService indicatorService,
            ISubjectService subjectService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService,
            IFootnoteRepository footnoteRepository,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper)
        {
            _context = context;
            _filterService = filterService;
            _filterGroupService = filterGroupService;
            _filterItemService = filterItemService;
            _indicatorService = indicatorService;
            _subjectService = subjectService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _footnoteRepository = footnoteRepository;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
        }

        public async Task<Either<ActionResult, Footnote>> CreateFootnote(
            Guid releaseId,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    var footnote = (await _context.Footnote.AddAsync(new Footnote
                    {
                        Content = content,
                        Filters = new List<FilterFootnote>(),
                        FilterGroups = new List<FilterGroupFootnote>(),
                        FilterItems = new List<FilterItemFootnote>(),
                        Indicators = new List<IndicatorFootnote>(),
                        Subjects = new List<SubjectFootnote>()
                    })).Entity;

                    CreateSubjectLinks(footnote, subjectIds);
                    CreateFilterLinks(footnote, filterIds);
                    CreateFilterGroupLinks(footnote, filterGroupIds);
                    CreateFilterItemLinks(footnote, filterItemIds);
                    CreateIndicatorsLinks(footnote, indicatorIds);

                    await _context.ReleaseFootnote.AddAsync(new ReleaseFootnote
                        {
                            ReleaseId = releaseId,
                            Footnote = footnote
                        });

                    await _context.SaveChangesAsync();
                    return await GetFootnote(releaseId, footnote.Id);
                });
        }

        public async Task<Either<ActionResult, Unit>> CopyFootnotes(Guid sourceReleaseId, Guid destinationReleaseId)
        {
            return await GetFootnotes(sourceReleaseId)
                .OnSuccess(async footnotes =>
                {
                    await footnotes.ForEachAsync(async footnote =>
                    {
                        var filters = footnote.Filters
                            .Select(filterFootnote => filterFootnote.FilterId).ToList();
                        var filterGroups = footnote.FilterGroups
                            .Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId).ToList();
                        var filterItems = footnote.FilterItems
                            .Select(filterItemFootnote => filterItemFootnote.FilterItemId).ToList();
                        var indicators = footnote.Indicators
                            .Select(indicatorFootnote => indicatorFootnote.IndicatorId).ToList();
                        var subjects = footnote.Subjects
                            .Select(subjectFootnote => subjectFootnote.SubjectId).ToList();

                        await CreateFootnote(destinationReleaseId,
                            footnote.Content,
                            filters,
                            filterGroups,
                            filterItems,
                            indicators,
                            subjects);
                    });

                    return Unit.Instance;
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseId, Guid id)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(id)
                .OnSuccess(async footnote =>
                {
                    await _footnoteRepository.DeleteFootnote(releaseId, footnote.Id);
                    await _context.SaveChangesAsync();
                    return Unit.Instance;
                }));
        }

        public async Task<Either<ActionResult, Footnote>> UpdateFootnote(
            Guid releaseId,
            Guid id,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(id, HydrateFootnote)
                .OnSuccess(async footnote =>
                {
                    if (await _footnoteRepository.IsFootnoteExclusiveToReleaseAsync(releaseId, footnote.Id))
                    {
                        _context.Update(footnote);

                        footnote.Content = content;

                        UpdateFilterLinks(footnote, filterIds.ToList());
                        UpdateFilterGroupLinks(footnote, filterGroupIds.ToList());
                        UpdateFilterItemLinks(footnote, filterItemIds.ToList());
                        UpdateIndicatorLinks(footnote, indicatorIds.ToList());
                        UpdateSubjectLinks(footnote, subjectIds.ToList());

                        await _context.SaveChangesAsync();
                        return await _footnoteRepository.GetFootnote(id);
                    }

                    // If this amendment of the footnote affects other release then break the link with the old
                    // and create a new one
                    await _footnoteRepository.DeleteReleaseFootnoteLinkAsync(releaseId, footnote.Id);

                    return await CreateFootnote(
                        releaseId,
                        content,
                        filterIds,
                        filterGroupIds,
                        filterItemIds,
                        indicatorIds,
                        subjectIds);
                }));
        }

        public Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseId, Guid id)
        {
            return _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess<ActionResult, Release, Footnote>(async release =>
                    {
                        var footnote = await _footnoteRepository.GetFootnote(id);

                        if (footnote == null
                            || footnote.Releases.All(rf => rf.ReleaseId != release.Id))
                        {
                            return new NotFoundResult();
                        }

                        return footnote;
                    }
                );
        }

        public async Task<Either<ActionResult, IEnumerable<Footnote>>> GetFootnotes(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ => _footnoteRepository.GetFootnotes(releaseId));
        }

        public IEnumerable<Footnote> GetFootnotes(Guid releaseId, Guid subjectId)
        {
            return _footnoteRepository.GetFootnotes(releaseId, subjectId);
        }

        private void CreateSubjectLinks(Footnote footnote, IEnumerable<Guid> subjectIds)
        {
            var subjects = _subjectService.FindMany(subject => subjectIds.Contains(subject.Id));

            foreach (var subject in subjects)
            {
                footnote.Subjects.Add(new SubjectFootnote
                {
                    FootnoteId = footnote.Id,
                    SubjectId = subject.Id
                });
            }
        }

        private void CreateFilterLinks(Footnote footnote, IEnumerable<Guid> filterIds)
        {
            var filters = _filterService.FindMany(filter => filterIds.Contains(filter.Id));
            foreach (var filter in filters)
            {
                footnote.Filters.Add(new FilterFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterId = filter.Id
                });
            }
        }

        private void CreateFilterGroupLinks(Footnote footnote, IEnumerable<Guid> filterGroupIds)
        {
            var filterGroups = _filterGroupService.FindMany(filterGroup => filterGroupIds.Contains(filterGroup.Id));

            foreach (var filterGroup in filterGroups)
            {
                footnote.FilterGroups.Add(new FilterGroupFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterGroupId = filterGroup.Id
                });
            }
        }

        private void CreateFilterItemLinks(Footnote footnote, IEnumerable<Guid> filterItemIds)
        {
            var filterItems = _filterItemService.FindMany(filterItem => filterItemIds.Contains(filterItem.Id));

            foreach (var filterItem in filterItems)
            {
                footnote.FilterItems.Add(new FilterItemFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterItemId = filterItem.Id
                });
            }
        }

        private void CreateIndicatorsLinks(Footnote footnote, IEnumerable<Guid> indicatorIds)
        {
            var indicators = _indicatorService.FindMany(indicator => indicatorIds.Contains(indicator.Id));

            foreach (var indicator in indicators)
            {
                footnote.Indicators.Add(new IndicatorFootnote
                {
                    FootnoteId = footnote.Id,
                    IndicatorId = indicator.Id
                });
            }
        }

        private void UpdateFilterLinks(Footnote footnote, IReadOnlyCollection<Guid> filterIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Filters.Select(link => link.FilterId), filterIds))
            {
                footnote.Filters = new List<FilterFootnote>();
                CreateFilterLinks(footnote, filterIds);
            }
        }

        private void UpdateFilterGroupLinks(Footnote footnote, IReadOnlyCollection<Guid> filterGroupIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterGroups.Select(link => link.FilterGroupId), filterGroupIds))
            {
                footnote.FilterGroups = new List<FilterGroupFootnote>();
                CreateFilterGroupLinks(footnote, filterGroupIds);
            }
        }

        private void UpdateFilterItemLinks(Footnote footnote, IReadOnlyCollection<Guid> filterItemIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterItems.Select(link => link.FilterItemId), filterItemIds))
            {
                footnote.FilterItems = new List<FilterItemFootnote>();
                CreateFilterItemLinks(footnote, filterItemIds);
            }
        }

        private void UpdateIndicatorLinks(Footnote footnote, IReadOnlyCollection<Guid> indicatorIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Indicators.Select(link => link.IndicatorId), indicatorIds))
            {
                footnote.Indicators = new List<IndicatorFootnote>();
                CreateIndicatorsLinks(footnote, indicatorIds);
            }
        }

        private void UpdateSubjectLinks(Footnote footnote, IReadOnlyCollection<Guid> subjectIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Subjects.Select(link => link.SubjectId), subjectIds))
            {
                footnote.Subjects = new List<SubjectFootnote>();
                CreateSubjectLinks(footnote, subjectIds);
            }
        }

        private static bool SequencesAreEqualIgnoringOrder(IEnumerable<Guid> left, IEnumerable<Guid> right)
        {
            return left.OrderBy(id => id).SequenceEqual(right.OrderBy(id => id));
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
    }
}