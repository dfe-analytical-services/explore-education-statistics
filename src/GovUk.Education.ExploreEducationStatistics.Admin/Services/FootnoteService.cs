using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FootnoteService : AbstractRepository<Footnote, Guid>, IFootnoteService
    {
        private readonly IFilterService _filterService;
        private readonly IFilterGroupService _filterGroupService;
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorService _indicatorService;
        private readonly ISubjectService _subjectService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;

        public FootnoteService(StatisticsDbContext context,
            ILogger<FootnoteService> logger,
            IFilterService filterService,
            IFilterGroupService filterGroupService,
            IFilterItemService filterItemService,
            IIndicatorService indicatorService,
            ISubjectService subjectService, 
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper, 
            IUserService userService, 
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper) : base(context, logger)
        {
            _filterService = filterService;
            _filterGroupService = filterGroupService;
            _filterItemService = filterItemService;
            _indicatorService = indicatorService;
            _subjectService = subjectService;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
        }

        public async Task<Either<ActionResult, Footnote>> CreateFootnote(
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return await CheckCanUpdateReleases(
                    filterIds,
                    filterGroupIds,
                    filterItemIds,
                    indicatorIds,
                    subjectIds)
                .OnSuccess(releaseIds =>
                {
                    var footnote = DbSet().Add(new Footnote
                    {
                        Content = content,
                        Filters = new List<FilterFootnote>(),
                        FilterGroups = new List<FilterGroupFootnote>(),
                        FilterItems = new List<FilterItemFootnote>(),
                        Indicators = new List<IndicatorFootnote>(),
                        Subjects = new List<SubjectFootnote>(),
                        // TODO BAU-384 - this can be removed when this work goes out
                        ReleaseId = releaseIds.First()
                    }).Entity;

                    CreateSubjectLinks(footnote, subjectIds);
                    CreateFilterLinks(footnote, filterIds);
                    CreateFilterGroupLinks(footnote, filterGroupIds);
                    CreateFilterItemLinks(footnote, filterItemIds);
                    CreateIndicatorsLinks(footnote, indicatorIds);

                    _context.SaveChanges();
                    return footnote;
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteFootnote(Guid id)
        {
            return await _statisticsPersistenceHelper
                .CheckEntityExists<Footnote>(id, HydrateFootnote)
                .OnSuccess(CheckCanUpdateReleases)
                .OnSuccess(footnote =>
                {
                    DeleteEntities(footnote.Subjects);
                    DeleteEntities(footnote.Filters);
                    DeleteEntities(footnote.FilterGroups);
                    DeleteEntities(footnote.FilterItems);
                    DeleteEntities(footnote.Indicators);

                    Remove(id);
                    _context.SaveChanges();
                    return true;
                });
        }

        public Task<Either<ActionResult, Footnote>> UpdateFootnote(Guid id,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return _statisticsPersistenceHelper
                .CheckEntityExists<Footnote>(id, HydrateFootnote)
                .OnSuccess(CheckCanUpdateReleases)
                .OnSuccess(footnote =>
                {
                    DbSet().Update(footnote);

                    footnote.Content = content;

                    UpdateFilterLinks(footnote, filterIds.ToList());
                    UpdateFilterGroupLinks(footnote, filterGroupIds.ToList());
                    UpdateFilterItemLinks(footnote, filterItemIds.ToList());
                    UpdateIndicatorLinks(footnote, indicatorIds.ToList());
                    UpdateSubjectLinks(footnote, subjectIds.ToList());

                    _context.SaveChanges();
                    return GetFootnote(id);
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Footnote>>> GetFootnotesAsync(Guid releaseId)
        {
            var subjectIds = await _context
                .ReleaseSubject
                .Where(r => r.ReleaseId == releaseId)
                .Select(r => r.SubjectId)
                .ToListAsync();
            
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => DbSet()
                    .Where(footnote =>
                        (!footnote.Subjects.Any() ||
                         footnote.Subjects.Any(subjectFootnote => subjectIds.Contains(subjectFootnote.SubjectId)))
                        && (!footnote.Filters.Any() || footnote.Filters.Any(filterFootnote =>
                                subjectIds.Contains(filterFootnote.Filter.SubjectId)))
                        && (!footnote.FilterGroups.Any() || footnote.FilterGroups.Any(filterGroupFootnote =>
                                subjectIds.Contains(filterGroupFootnote.FilterGroup.Filter.SubjectId)))
                        && (!footnote.FilterItems.Any() || footnote.FilterItems.Any(filterItemFootnote =>
                                subjectIds.Contains(filterItemFootnote.FilterItem.FilterGroup.Filter.SubjectId)))
                        && (!footnote.Indicators.Any() || footnote.Indicators.Any(indicatorFootnote =>
                                subjectIds.Contains(indicatorFootnote.Indicator.IndicatorGroup.SubjectId)))
                        )
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
                    .Include(footnote => footnote.Subjects)
                    .AsEnumerable()
                );
        }

        private void CreateSubjectLinks(Footnote footnote, IEnumerable<Guid> subjectIds)
        {
            var subjects = _subjectService.FindMany(subject => subjectIds.Contains(subject.Id));

            var links = footnote.Subjects;
            foreach (var subject in subjects)
            {
                AddSubjectLink(ref links, footnote.Id, subject.Id);
            }
        }

        private void CreateFilterLinks(Footnote footnote, IEnumerable<Guid> filterIds)
        {
            var filters = _filterService.FindMany(filter => filterIds.Contains(filter.Id));

            var links = footnote.Filters;
            foreach (var filter in filters)
            {
                AddFilterLink(ref links, footnote.Id, filter.Id);
            }
        }

        private void CreateFilterGroupLinks(Footnote footnote, IEnumerable<Guid> filterGroupIds)
        {
            var filterGroups = _filterGroupService.FindMany(filterGroup => filterGroupIds.Contains(filterGroup.Id),
                new List<Expression<Func<FilterGroup, object>>>
                {
                    filterGroup => filterGroup.Filter
                });

            var links = footnote.FilterGroups;
            foreach (var filterGroup in filterGroups)
            {
                AddFilterGroupLink(ref links, footnote.Id, filterGroup.Id);
            }
        }

        private void CreateFilterItemLinks(Footnote footnote, IEnumerable<Guid> filterItemIds)
        {
            var filterItems = _filterItemService.FindMany(filterItem => filterItemIds.Contains(filterItem.Id),
                new List<Expression<Func<FilterItem, object>>>
                {
                    filterItem => filterItem.FilterGroup.Filter
                });

            var links = footnote.FilterItems;
            foreach (var filterItem in filterItems)
            {
                AddFilterItemLink(ref links, footnote.Id, filterItem.Id);
            }
        }

        private void CreateIndicatorsLinks(Footnote footnote, IEnumerable<Guid> indicatorIds)
        {
            var indicators = _indicatorService.FindMany(indicator => indicatorIds.Contains(indicator.Id),
                new List<Expression<Func<Indicator, object>>>
                {
                    indicator => indicator.IndicatorGroup
                });

            var links = footnote.Indicators;
            foreach (var indicator in indicators)
            {
                AddIndicatorLink(ref links, footnote.Id, indicator.Id);
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

        private static void AddIndicatorLink(ref ICollection<IndicatorFootnote> links, Guid footnoteId, Guid linkId)
        {
            links.Add(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = linkId
            });
        }

        private static void AddFilterLink(ref ICollection<FilterFootnote> links, Guid footnoteId, Guid linkId)
        {
            links.Add(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = linkId
            });
        }

        private static void AddFilterGroupLink(ref ICollection<FilterGroupFootnote> links, Guid footnoteId, Guid linkId)
        {
            links.Add(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = linkId
            });
        }

        private static void AddFilterItemLink(ref ICollection<FilterItemFootnote> links, Guid footnoteId, Guid linkId)
        {
            links.Add(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = linkId
            });
        }

        private static void AddSubjectLink(ref ICollection<SubjectFootnote> links, Guid footnoteId, Guid subjectId)
        {
            links.Add(new SubjectFootnote
            {
                FootnoteId = footnoteId,
                SubjectId = subjectId
            });
        }

        private void DeleteEntities<T>(IEnumerable<T> entitiesToDelete)
        {
            foreach (var t in entitiesToDelete)
            {
                _context.Entry(t).State = EntityState.Deleted;
            }
        }

        private static bool SequencesAreEqualIgnoringOrder(IEnumerable<Guid> left, IEnumerable<Guid> right)
        {
            return left.OrderBy(id => id).SequenceEqual(right.OrderBy(id => id));
        }

        private List<Guid> GetSubjectIdsFromFootnoteLinks(
            IReadOnlyCollection<Guid>? filterIds,
            IReadOnlyCollection<Guid>? filterGroupIds,
            IReadOnlyCollection<Guid>? filterItemIds,
            IReadOnlyCollection<Guid>? indicatorIds,
            IReadOnlyCollection<Guid>? subjectIds)
        {
            if (filterIds != null && filterIds.Any())
            {
                return _context
                    .Filter
                    .Where(f => f.Id == filterIds.First())
                    .Select(f => f.SubjectId)
                    .ToList();
            }

            if (filterGroupIds != null && filterGroupIds.Any())
            {
                return _context
                    .FilterGroup
                    .Include(f => f.Filter)
                    .Where(f => f.Id == filterGroupIds.First())
                    .Select(f => f.Filter.SubjectId)
                    .ToList();
            }
            
            if (filterItemIds != null && filterItemIds.Any())
            {
                return _context
                    .FilterItem
                    .Include(f => f.FilterGroup)
                    .ThenInclude(f => f.Filter)
                    .Where(f => f.Id == filterItemIds.First())
                    .Select(f => f.FilterGroup.Filter.SubjectId)
                    .ToList();
            }
            
            if (indicatorIds != null && indicatorIds.Any())
            {
                return _context
                    .Indicator
                    .Include(i => i.IndicatorGroup)
                    .Where(i => i.Id == indicatorIds.First())
                    .Select(i => i.IndicatorGroup.SubjectId)
                    .ToList();
            }
            
            return subjectIds?.ToList();
        }

        private Task<Either<ActionResult, Footnote>> CheckCanUpdateReleases(Footnote footnote)
        {
            return CheckCanUpdateReleases(
                    footnote.Filters?.Select(f => f.FilterId).ToList(),
                    footnote.FilterGroups?.Select(f => f.FilterGroupId).ToList(),
                    footnote.FilterItems?.Select(f => f.FilterItemId).ToList(),
                    footnote.Indicators?.Select(i => i.IndicatorId).ToList(),
                    footnote.Subjects?.Select(s => s.SubjectId).ToList()
                )
                .OnSuccess(_ => footnote);
        }

        // TODO BAU-384 - the returned list of release ids can be removed when this work goes out
        private async Task<Either<ActionResult, List<Guid>>> CheckCanUpdateReleases(
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds)
        {
            var linkedSubjectIds = GetSubjectIdsFromFootnoteLinks(
                filterIds?.ToList(),
                filterGroupIds?.ToList(),
                filterItemIds?.ToList(),
                indicatorIds?.ToList(),
                subjectIds?.ToList()
            );

            var releaseIds = await _context
                .ReleaseSubject
                .Where(r => linkedSubjectIds.Contains(r.SubjectId))
                .Select(r => r.ReleaseId)
                .ToListAsync();

            foreach (var releaseId in releaseIds)
            {
                var canUpdate = await GetContentReleaseById(releaseId)
                    .OnSuccess(_userService.CheckCanUpdateRelease);

                if (canUpdate.IsLeft)
                {
                    return canUpdate.OnSuccess(_ => new List<Guid>());
                }
            }

            return releaseIds;
        }
        
        private Task<Either<ActionResult, Release>> GetContentReleaseById(Guid releaseId)
        {
            return _contentPersistenceHelper.CheckEntityExists<Release>(releaseId);
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

        private Either<ActionResult, Footnote> GetFootnote(Guid id)
        {
            return DbSet().Where(footnote => footnote.Id == id)
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
                .Include(footnote => footnote.Subjects)
                .SingleOrDefault();
        }
    }
}