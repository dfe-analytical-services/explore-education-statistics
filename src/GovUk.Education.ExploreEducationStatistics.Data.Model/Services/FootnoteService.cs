using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class FootnoteService : AbstractRepository<Footnote, long>, IFootnoteService
    {
        private readonly IFilterService _filterService;
        private readonly IFilterGroupService _filterGroupService;
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorService _indicatorService;
        private readonly ISubjectService _subjectService;

        public FootnoteService(StatisticsDbContext context,
            ILogger<FootnoteService> logger,
            IFilterService filterService,
            IFilterGroupService filterGroupService,
            IFilterItemService filterItemService,
            IIndicatorService indicatorService,
            ISubjectService subjectService) : base(context, logger)
        {
            _filterService = filterService;
            _filterGroupService = filterGroupService;
            _filterItemService = filterItemService;
            _indicatorService = indicatorService;
            _subjectService = subjectService;
        }

        public Footnote CreateFootnote(string content,
            IEnumerable<long> filterIds,
            IEnumerable<long> filterGroupIds,
            IEnumerable<long> filterItemIds,
            IEnumerable<long> indicatorIds,
            long subjectId)
        {
            var footnote = DbSet().Add(new Footnote
            {
                Content = content,
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Subjects = new List<SubjectFootnote>()
            }).Entity;
            
            CreateSubjectLink(footnote, subjectId);
            CreateFilterLinks(footnote, filterIds);
            CreateFilterGroupLinks(footnote, filterGroupIds);
            CreateFilterItemLinks(footnote, filterItemIds);
            CreateIndicatorsLinks(footnote, indicatorIds);

            _context.SaveChanges();
            return footnote;
        }

        public void DeleteFootnote(long id)
        {
            var footnote = Find(id, new List<Expression<Func<Footnote, object>>>
            {
                f => f.Filters, f => f.FilterGroups, f => f.FilterItems, f => f.Indicators, f => f.Subjects
            });
            
            DeleteEntities(footnote.Subjects);
            DeleteEntities(footnote.Filters);
            DeleteEntities(footnote.FilterGroups);
            DeleteEntities(footnote.FilterItems);
            DeleteEntities(footnote.Indicators);

            Remove(id);
            _context.SaveChanges();
        }

        public Footnote GetFootnote(long id)
        {
            return Find(id);
        }

        public Footnote UpdateFootnote(long id,
            string content,
            IEnumerable<long> filterIds,
            IEnumerable<long> filterGroupIds,
            IEnumerable<long> filterItemIds,
            IEnumerable<long> indicatorIds)
        {
            var footnote = Find(id, new List<Expression<Func<Footnote, object>>>
            {
                f => f.Filters, f => f.FilterGroups, f => f.FilterItems, f => f.Indicators
            });

            DbSet().Update(footnote);

            footnote.Content = content;

            UpdateFilterLinks(footnote, filterIds);
            UpdateFilterGroupLinks(footnote, filterGroupIds);
            UpdateFilterItemLinks(footnote, filterItemIds);
            UpdateIndicatorLinks(footnote, indicatorIds);

            _context.SaveChanges();
            return GetFootnote(id);
        }

        public IEnumerable<Footnote> GetFootnotes(long subjectId,
            IQueryable<Observation> observations,
            IEnumerable<long> indicators)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem.Id).Distinct();

            var subjectIdParam = new SqlParameter("subjectId", subjectId);
            var indicatorListParam = CreateIdListType("indicatorList", indicators);
            var filterItemListParam = CreateIdListType("filterItemList", filterItems);

            return _context.Footnote.FromSqlRaw(
                "EXEC dbo.FilteredFootnotes " +
                "@subjectId," +
                "@indicatorList," +
                "@filterItemList",
                subjectIdParam,
                indicatorListParam,
                filterItemListParam);
        }

        private void CreateSubjectLink(Footnote footnote, long subjectId)
        {
            var subject = _subjectService.Find(subjectId, new List<Expression<Func<Subject, object>>>
                          {
                              s => s.Footnotes
                          }) ??
                          throw new ArgumentException("Subject not found", nameof(subjectId));

            footnote.Subjects.Add(new SubjectFootnote
            {
                FootnoteId = footnote.Id,
                SubjectId = subject.Id
            });
        }

        private void CreateFilterLinks(Footnote footnote, IEnumerable<long> filterIds)
        {
            var filters = _filterService.FindMany(filter => filterIds.Contains(filter.Id),
                new List<Expression<Func<Filter, object>>>
                {
                    filter => filter.Footnotes
                });

            var links = footnote.Filters;
            foreach (var filter in filters)
            {
                AddFilterLink(ref links, footnote.Id, filter.Id);
            }
        }

        private void CreateFilterGroupLinks(Footnote footnote, IEnumerable<long> filterGroupIds)
        {
            var filterGroups = _filterGroupService.FindMany(filter => filterGroupIds.Contains(filter.Id),
                new List<Expression<Func<FilterGroup, object>>>
                {
                    filter => filter.Footnotes
                });

            var links = footnote.FilterGroups;
            foreach (var filterGroup in filterGroups)
            {
                AddFilterGroupLink(ref links, footnote.Id, filterGroup.Id);
            }
        }

        private void CreateFilterItemLinks(Footnote footnote, IEnumerable<long> filterItemIds)
        {
            var filterItems = _filterItemService.FindMany(filter => filterItemIds.Contains(filter.Id),
                new List<Expression<Func<FilterItem, object>>>
                {
                    filter => filter.Footnotes
                });

            var links = footnote.FilterItems;
            foreach (var filterItem in filterItems)
            {
                AddFilterItemLink(ref links, footnote.Id, filterItem.Id);
            }
        }

        private void CreateIndicatorsLinks(Footnote footnote, IEnumerable<long> indicatorIds)
        {
            var indicators = _indicatorService.FindMany(filter => indicatorIds.Contains(filter.Id),
                new List<Expression<Func<Indicator, object>>>
                {
                    filter => filter.Footnotes
                });

            var links = footnote.Indicators;
            foreach (var indicator in indicators)
            {
                AddIndicatorLink(ref links, footnote.Id, indicator.Id);
            }
        }

        private void UpdateFilterLinks(Footnote footnote, IEnumerable<long> filterIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Filters.Select(link => link.FilterId), filterIds))
            {
                footnote.Filters = new List<FilterFootnote>();
                CreateFilterLinks(footnote, filterIds);
            }
        }

        private void UpdateFilterGroupLinks(Footnote footnote, IEnumerable<long> filterGroupIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterGroups.Select(link => link.FilterGroupId), filterGroupIds))
            {
                footnote.FilterGroups = new List<FilterGroupFootnote>();
                CreateFilterGroupLinks(footnote, filterGroupIds);
            }
        }

        private void UpdateFilterItemLinks(Footnote footnote, IEnumerable<long> filterItemIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterItems.Select(link => link.FilterItemId), filterItemIds))
            {
                footnote.FilterItems = new List<FilterItemFootnote>();
                CreateFilterItemLinks(footnote, filterItemIds);
            }
        }

        private void UpdateIndicatorLinks(Footnote footnote, IEnumerable<long> indicatorIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Indicators.Select(link => link.IndicatorId), indicatorIds))
            {
                footnote.Indicators = new List<IndicatorFootnote>();
                CreateIndicatorsLinks(footnote, indicatorIds);
            }
        }

        private static void AddIndicatorLink(ref ICollection<IndicatorFootnote> links, long footnoteId, long linkId)
        {
            links.Add(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = linkId
            });
        }

        private static void AddFilterLink(ref ICollection<FilterFootnote> links, long footnoteId, long linkId)
        {
            links.Add(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = linkId
            });
        }

        private static void AddFilterGroupLink(ref ICollection<FilterGroupFootnote> links, long footnoteId, long linkId)
        {
            links.Add(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = linkId
            });
        }

        private static void AddFilterItemLink(ref ICollection<FilterItemFootnote> links, long footnoteId, long linkId)
        {
            links.Add(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = linkId
            });
        }

        private void DeleteEntities<T>(IEnumerable<T> entitiesToDelete)
        {
            foreach (var t in entitiesToDelete)
            {
                _context.Entry(t).State = EntityState.Deleted;
            }
        }

        private static bool SequencesAreEqualIgnoringOrder(IEnumerable<long> left, IEnumerable<long> right)
        {
            return left.OrderBy(id => id).SequenceEqual(right.OrderBy(id => id));
        }
    }
}