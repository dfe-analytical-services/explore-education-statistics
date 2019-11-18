using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class FootnoteController : ControllerBase
    {
        private readonly IFilterService _filterService;
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly IReleaseMetaService _releaseMetaService;
        private readonly IMapper _mapper;

        public FootnoteController(IFilterService filterService,
            IFootnoteService footnoteService,
            IIndicatorGroupService indicatorGroupService,
            IReleaseMetaService releaseMetaService,
            IMapper mapper)
        {
            _filterService = filterService;
            _footnoteService = footnoteService;
            _indicatorGroupService = indicatorGroupService;
            _releaseMetaService = releaseMetaService;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult<FootnoteViewModel> CreateFootnote(CreateFootnoteViewModel footnote)
        {
            var created = _footnoteService.CreateFootnote(footnote.Content,
                footnote.Filters,
                footnote.FilterGroups,
                footnote.FilterItems,
                footnote.Indicators,
                footnote.Subjects);

            return GatherAndBuildFootnoteViewModel(created);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteFootnote(long id)
        {
            return CheckFootnoteExists(id, () =>
            {
                _footnoteService.DeleteFootnote(id);
                return new NoContentResult();
            });
        }

        [HttpGet("release/{releaseId}")]
        public ActionResult<FootnotesViewModel> GetFootnotes(Guid releaseId)
        {
            var footnotes = _footnoteService.GetFootnotes(releaseId).Select(GatherAndBuildFootnoteViewModel);
            var subjects = _releaseMetaService.GetSubjects(releaseId).ToDictionary(subject => subject.Id, subject =>
                new FootnotesSubjectMetaViewModel
                {
                    Filters = GetFilters(subject.Id),
                    Indicators = GetIndicators(subject.Id),
                    SubjectId = subject.Id,
                    SubjectName = subject.Label
                });

            return new FootnotesViewModel
            {
                Footnotes = footnotes,
                Meta = subjects
            };
        }

        [HttpPut("{id}")]
        public ActionResult<FootnoteViewModel> UpdateFootnote(long id, UpdateFootnoteViewModel footnote)
        {
            var updated = _footnoteService.UpdateFootnote(id,
                footnote.Content,
                footnote.Filters,
                footnote.FilterGroups,
                footnote.FilterItems,
                footnote.Indicators,
                footnote.Subjects);

            return GatherAndBuildFootnoteViewModel(updated);
        }

        private ActionResult CheckFootnoteExists(long id, Func<ActionResult> andThen)
        {
            var footnote = _footnoteService.GetFootnote(id);
            return footnote == null ? NotFound() : andThen.Invoke();
        }

        private Dictionary<long, FootnotesIndicatorsMetaViewModel> GetIndicators(long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Id,
                group => new FootnotesIndicatorsMetaViewModel
                {
                    Label = group.Label,
                    Options = group.Indicators.ToDictionary(
                        indicator => indicator.Id,
                        indicator => _mapper.Map<IndicatorMetaViewModel>(indicator))
                }
            );
        }

        private Dictionary<long, FootnotesFilterMetaViewModel> GetFilters(long subjectId)
        {
            return _filterService.GetFiltersIncludingItems(subjectId)
                .ToDictionary(
                    filter => filter.Id,
                    filter => new FootnotesFilterMetaViewModel
                    {
                        Hint = filter.Hint,
                        Legend = filter.Label,
                        Options = filter.FilterGroups.ToDictionary(
                            filterGroup => filterGroup.Id,
                            filterGroup => BuildFilterItemsMetaViewModel(filterGroup, filterGroup.FilterItems))
                    });
        }

        private static FootnotesFilterItemsMetaViewModel BuildFilterItemsMetaViewModel(FilterGroup filterGroup,
            IEnumerable<FilterItem> filterItems)
        {
            return new FootnotesFilterItemsMetaViewModel
            {
                Label = filterGroup.Label,
                Options = filterItems.ToDictionary(
                    item => item.Id,
                    item => new LabelValue
                    {
                        Label = item.Label,
                        Value = item.Id.ToString()
                    })
            };
        }

        private static FootnoteViewModel GatherAndBuildFootnoteViewModel(Footnote footnote)
        {
            var filterItemsGroupByFilterGroup = footnote.FilterItems.GroupBy(
                    filterItemFootnote => filterItemFootnote.FilterItem.FilterGroup,
                    filterItemFootnote => filterItemFootnote.FilterItemId)
                .ToList();

            var filterItemsGroupByFilter = footnote.FilterItems.GroupBy(
                    filterItemFootnote => filterItemFootnote.FilterItem.FilterGroup.Filter,
                    filterItemFootnote => filterItemFootnote.FilterItemId)
                .ToList();

            var filterGroupsGroupByFilter = footnote.FilterGroups.GroupBy(
                    filterGroupFootnote => filterGroupFootnote.FilterGroup.Filter,
                    filterGroupFootnote => filterGroupFootnote.FilterGroupId)
                .ToList();

            var filterItemsByFilterGroup = filterItemsGroupByFilterGroup
                .ToDictionary(grouping => grouping.Key.Id, grouping => grouping.ToList());

            var filterGroupsByFilter = filterGroupsGroupByFilter
                .ToDictionary(grouping => grouping.Key.Id, grouping => grouping.ToList());

            // There can be more than one indicator belonging to the same subject in each group so we add Distinct here. 
            var indicatorGroupsBySubject = footnote.Indicators
                .GroupBy(indicatorFootnote => indicatorFootnote.Indicator.IndicatorGroup.SubjectId,
                    indicatorFootnote => indicatorFootnote.Indicator.IndicatorGroupId)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Distinct().ToList());

            var indicatorsByIndicatorGroup = footnote.Indicators
                .GroupBy(indicatorFootnote => indicatorFootnote.Indicator.IndicatorGroupId,
                    indicatorFootnote => indicatorFootnote.IndicatorId)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            var filtersBySubject = footnote.Filters.GroupBy(
                    filterFootnote => filterFootnote.Filter.SubjectId,
                    filterFootnote => filterFootnote.FilterId)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            // Index filter groups related to the filter items 
            AppendFilterGroups(filterItemsGroupByFilterGroup.Select(group => group.Key), filterGroupsByFilter);

            // Index filters related to the filter groups
            AppendFilters(filterGroupsGroupByFilter.Select(group => group.Key), filtersBySubject);

            // Index filters related to the filter items 
            AppendFilters(filterItemsGroupByFilter.Select(group => group.Key), filtersBySubject);

            var selectedSubjects = footnote.Subjects.Select(subjectFootnote => subjectFootnote.SubjectId).ToList();
            var selectedFilters = footnote.Filters.Select(filterFootnote => filterFootnote.FilterId);
            var selectedFilterGroups = footnote.FilterGroups.Select(groupFootnote => groupFootnote.FilterGroupId);

            var subjectIds = selectedSubjects
                .Concat(filtersBySubject.Keys)
                .Concat(indicatorGroupsBySubject.Keys)
                .Distinct();

            return BuildFootnoteViewModel(footnote,
                subjectIds,
                filtersBySubject,
                filterGroupsByFilter,
                filterItemsByFilterGroup,
                indicatorGroupsBySubject,
                indicatorsByIndicatorGroup,
                selectedSubjects,
                selectedFilters,
                selectedFilterGroups);
        }

        private static void AppendFilterGroups(IEnumerable<FilterGroup> filterGroups,
            IDictionary<long, List<long>> filterGroupsByFilter)
        {
            foreach (var filterGroup in filterGroups)
            {
                if (!filterGroupsByFilter.TryGetValue(filterGroup.FilterId, out var idList))
                {
                    idList = new List<long>();
                    filterGroupsByFilter.Add(filterGroup.FilterId, idList);
                }

                if (!idList.Contains(filterGroup.Id))
                {
                    idList.Add(filterGroup.Id);
                }
            }
        }

        private static void AppendFilters(IEnumerable<Filter> filters,
            IDictionary<long, List<long>> filtersBySubject)
        {
            foreach (var filter in filters)
            {
                if (!filtersBySubject.TryGetValue(filter.SubjectId, out var idList))
                {
                    idList = new List<long>();
                    filtersBySubject.Add(filter.SubjectId, idList);
                }

                if (!idList.Contains(filter.Id))
                {
                    idList.Add(filter.Id);
                }
            }
        }

        private static FootnoteViewModel BuildFootnoteViewModel(Footnote footnote,
            IEnumerable<long> subjectIds,
            IReadOnlyDictionary<long, List<long>> filtersBySubject,
            IReadOnlyDictionary<long, List<long>> filterGroupsByFilter,
            IReadOnlyDictionary<long, List<long>> filterItemsByFilterGroup,
            IReadOnlyDictionary<long, List<long>> indicatorGroupsBySubject,
            IReadOnlyDictionary<long, List<long>> indicatorsByIndicatorGroup,
            IEnumerable<long> selectedSubjects,
            IEnumerable<long> selectedFilters,
            IEnumerable<long> selectedFilterGroups)
        {
            return new FootnoteViewModel
            {
                Id = footnote.Id,
                Content = footnote.Content,
                Subjects = subjectIds.ToDictionary(
                    subjectId => subjectId,
                    subjectId => new FootnoteSubjectViewModel
                    {
                        Filters = filtersBySubject.GetValueOrDefault(subjectId)?.ToDictionary(
                                      filterId => filterId,
                                      filterId => new FootnoteFilterViewModel
                                      {
                                          FilterGroups = filterGroupsByFilter.GetValueOrDefault(filterId)?.ToDictionary(
                                                             filterGroupId => filterGroupId,
                                                             filterGroupId => new FootnoteFilterGroupViewModel
                                                             {
                                                                 FilterItems =
                                                                     filterItemsByFilterGroup.GetValueOrDefault(
                                                                         filterGroupId) ?? new List<long>(),
                                                                 Selected = selectedFilterGroups.Contains(filterGroupId)
                                                             }) ?? new Dictionary<long, FootnoteFilterGroupViewModel>(),
                                          Selected = selectedFilters.Contains(filterId)
                                      }) ?? new Dictionary<long, FootnoteFilterViewModel>(),
                        IndicatorGroups = indicatorGroupsBySubject.GetValueOrDefault(subjectId)?.ToDictionary(
                                              indicatorGroupId => indicatorGroupId,
                                              indicatorGroupId => new FootnoteIndicatorGroupViewModel
                                              {
                                                  Indicators =
                                                      indicatorsByIndicatorGroup.GetValueOrDefault(indicatorGroupId) ??
                                                      new List<long>()
                                              }) ?? new Dictionary<long, FootnoteIndicatorGroupViewModel>(),
                        Selected = selectedSubjects.Contains(subjectId)
                    })
            };
        }
    }
}