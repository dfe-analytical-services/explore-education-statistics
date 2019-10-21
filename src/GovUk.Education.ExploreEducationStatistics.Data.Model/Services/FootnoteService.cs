using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
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
                Content = content
            }).Entity;

            LinkSubject(footnote, subjectId);
            LinkIndicators(footnote, indicatorIds);
            LinkFilters(footnote, filterIds);
            LinkFilterGroups(footnote, filterGroupIds);
            LinkFilterItems(footnote, filterItemIds);

            _context.SaveChanges();
            return footnote;
        }

        public void DeleteFootnote(long id)
        {
            Remove(id);
            _context.SaveChanges();
        }

        public Footnote GetFootnote(long id)
        {
            return Find(id);
        }

        public Footnote UpdateFootnote(long id, string content)
        {
            var existing = Find(id);
            DbSet().Update(existing);
            existing.Content = content;
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

            return _context.Footnote.AsNoTracking().FromSql(
                "EXEC dbo.FilteredFootnotes " +
                "@subjectId," +
                "@indicatorList," +
                "@filterItemList",
                subjectIdParam,
                indicatorListParam,
                filterItemListParam);
        }

        private Subject LinkSubject(Footnote footnote, long subjectId)
        {
            var subject = _subjectService.Find(subjectId) ??
                          throw new ArgumentException("Subject not found", nameof(subjectId));

            if (subject.Footnotes == null)
            {
                subject.Footnotes = new List<SubjectFootnote>();
            }

            subject.Footnotes.Add(new SubjectFootnote
            {
                FootnoteId = footnote.Id,
                SubjectId = subject.Id
            });

            return subject;
        }

        private IEnumerable<Indicator> LinkIndicators(Footnote footnote, IEnumerable<long> indicatorIds)
        {
            var indicators = _indicatorService.Find(indicatorIds.ToArray());

            foreach (var indicator in indicators)
            {
                if (indicator.Footnotes == null)
                {
                    indicator.Footnotes = new List<IndicatorFootnote>();
                }

                indicator.Footnotes.Add(new IndicatorFootnote
                {
                    FootnoteId = footnote.Id,
                    IndicatorId = indicator.Id
                });
            }

            return indicators;
        }

        private IEnumerable<Filter> LinkFilters(Footnote footnote, IEnumerable<long> filterIds)
        {
            var filters = _filterService.Find(filterIds.ToArray());

            foreach (var filter in filters)
            {
                if (filter.Footnotes == null)
                {
                    filter.Footnotes = new List<FilterFootnote>();
                }

                filter.Footnotes.Add(new FilterFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterId = filter.Id
                });
            }

            return filters;
        }

        private IEnumerable<FilterGroup> LinkFilterGroups(Footnote footnote, IEnumerable<long> filterGroupIds)
        {
            var filterGroups = _filterGroupService.Find(filterGroupIds.ToArray());

            foreach (var filterGroup in filterGroups)
            {
                if (filterGroup.Footnotes == null)
                {
                    filterGroup.Footnotes = new List<FilterGroupFootnote>();
                }

                filterGroup.Footnotes.Add(new FilterGroupFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterGroupId = filterGroup.Id
                });
            }

            return filterGroups;
        }

        private IEnumerable<FilterItem> LinkFilterItems(Footnote footnote, IEnumerable<long> filterItemIds)
        {
            var filterItems = _filterItemService.Find(filterItemIds.ToArray());

            foreach (var filterItem in filterItems)
            {
                if (filterItem.Footnotes == null)
                {
                    filterItem.Footnotes = new List<FilterItemFootnote>();
                }

                filterItem.Footnotes.Add(new FilterItemFootnote
                {
                    FootnoteId = footnote.Id,
                    FilterItemId = filterItem.Id
                });
            }

            return filterItems;
        }
    }
}