using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces.IReleaseService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class FootnoteController : ControllerBase
    {
        private readonly IFilterRepository _filterRepository;
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly IReleaseService _releaseService;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

        public FootnoteController(IFilterRepository filterRepository,
            IFootnoteService footnoteService,
            IIndicatorGroupRepository indicatorGroupRepository,
            IReleaseService releaseService,
            IReleaseDataFileRepository releaseDataFileRepository)
        {
            _filterRepository = filterRepository;
            _footnoteService = footnoteService;
            _indicatorGroupRepository = indicatorGroupRepository;
            _releaseService = releaseService;
            _releaseDataFileRepository = releaseDataFileRepository;
        }

        [HttpPost("releases/{releaseId:guid}/footnotes")]
        public async Task<ActionResult<FootnoteViewModel>> CreateFootnote(Guid releaseId,
            FootnoteCreateRequest footnote)
        {
            return await _footnoteService
                .CreateFootnote(
                    releaseId,
                    footnote.Content,
                    footnote.Filters,
                    footnote.FilterGroups,
                    footnote.FilterItems,
                    footnote.Indicators,
                    footnote.Subjects)
                .OnSuccess(GatherAndBuildFootnoteViewModel)
                .HandleFailuresOrOk();
        }

        [HttpDelete("releases/{releaseId:guid}/footnotes/{footnoteId:guid}")]
        public async Task<ActionResult> DeleteFootnote(Guid releaseId,
            Guid footnoteId)
        {
            return await _footnoteService
                .DeleteFootnote(releaseId: releaseId,
                    footnoteId: footnoteId)
                .HandleFailuresOrNoContent();
        }

        [HttpGet("releases/{releaseId:guid}/footnotes/{footnoteId:guid}")]
        public async Task<ActionResult<FootnoteViewModel>> GetFootnote(Guid releaseId,
            Guid footnoteId)
        {
            return await _footnoteService
                .GetFootnote(releaseId: releaseId,
                    footnoteId: footnoteId)
                .OnSuccess(GatherAndBuildFootnoteViewModel)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/footnotes")]
        public async Task<ActionResult<IEnumerable<FootnoteViewModel>>> GetFootnotes(Guid releaseId)
        {
            return await _footnoteService
                .GetFootnotes(releaseId)
                .OnSuccess(footnotes => footnotes.Select(GatherAndBuildFootnoteViewModel))
                .HandleFailuresOrOk();
        }

        [HttpPut("releases/{releaseId:guid}/footnotes/{footnoteId:guid}")]
        public async Task<ActionResult<FootnoteViewModel>> UpdateFootnote(Guid releaseId,
            Guid footnoteId,
            FootnoteUpdateRequest footnote)
        {
            return await _footnoteService
                .UpdateFootnote(
                    releaseId: releaseId,
                    footnoteId: footnoteId,
                    footnote.Content,
                    filterIds: footnote.Filters,
                    filterGroupIds: footnote.FilterGroups,
                    filterItemIds: footnote.FilterItems,
                    indicatorIds: footnote.Indicators,
                    subjectIds: footnote.Subjects
                )
                .OnSuccess(GatherAndBuildFootnoteViewModel)
                .HandleFailuresOrOk();
        }

        [HttpGet("releases/{releaseId:guid}/footnotes-meta")]
        public async Task<ActionResult<FootnotesMetaViewModel>> GetFootnotesMeta(Guid releaseId)
        {
            return await _releaseService.ListSubjects(releaseId)
                .OnSuccess(async subjects =>
                {
                    var subjectMetaViewModels = await subjects
                        .ToAsyncEnumerable()
                        .SelectAwait(async subject =>
                            new FootnotesSubjectMetaViewModel
                            {
                                Filters = await GetFilters(subject.Id),
                                Indicators = await GetIndicators(subject.Id),
                                SubjectId = subject.Id,
                                SubjectName = (await _releaseDataFileRepository.GetBySubject(releaseId, subject.Id)).Name,
                            }
                        )
                        .ToListAsync();

                    return new FootnotesMetaViewModel
                    {
                        Subjects = subjectMetaViewModels
                            .ToDictionary(
                                result => result.SubjectId,
                                result => result)
                    };
                })
                .HandleFailuresOrOk();
        }

        [HttpPatch("releases/{releaseId:guid}/footnotes")]
        public async Task<ActionResult<Unit>> UpdateFootnotes(
            Guid releaseId,
            FootnotesUpdateRequest request)
        {
            return await _footnoteService.UpdateFootnotes(releaseId, request)
                .HandleFailuresOrOk();
        }

        private async Task<Dictionary<Guid, FootnotesIndicatorsMetaViewModel>> GetIndicators(Guid subjectId)
        {
            return (await _indicatorGroupRepository.GetIndicatorGroups(subjectId))
                .OrderBy(group => group.Label, LabelComparer)
                .ToDictionary(
                    group => group.Id,
                    group => new FootnotesIndicatorsMetaViewModel
                    {
                        Label = group.Label,
                        Options = group.Indicators
                            .OrderBy(indicator => indicator.Label, LabelComparer)
                            .Select(indicator => new IndicatorMetaViewModel(indicator))
                            .ToList()
                    }
                );
        }

        private async Task<Dictionary<Guid, FootnotesFilterMetaViewModel>> GetFilters(Guid subjectId)
        {
            return (await _filterRepository.GetFiltersIncludingItems(subjectId))
                .ToDictionary(
                    filter => filter.Id,
                    filter => new FootnotesFilterMetaViewModel
                    {
                        Hint = filter.Hint,
                        Legend = filter.Label,
                        Options = filter.FilterGroups
                            .OrderBy(items => items.Label, LabelComparer)
                            .ToDictionary(
                            filterGroup => filterGroup.Id,
                            filterGroup => BuildFilterItemsMetaViewModel(filterGroup, filterGroup.FilterItems))
                    });
        }

        private static FootnotesFilterGroupsMetaViewModel BuildFilterItemsMetaViewModel(FilterGroup filterGroup,
            IEnumerable<FilterItem> filterItems)
        {
            return new FootnotesFilterGroupsMetaViewModel
            {
                Label = filterGroup.Label,
                Options = filterItems
                    .OrderBy(item => item.Label, LabelComparer)
                    .Select(item => new LabelValue(item.Label, item.Id.ToString()))
                    .ToList()
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
            IDictionary<Guid, List<Guid>> filterGroupsByFilter)
        {
            foreach (var filterGroup in filterGroups)
            {
                if (!filterGroupsByFilter.TryGetValue(filterGroup.FilterId, out var idList))
                {
                    idList = new List<Guid>();
                    filterGroupsByFilter.Add(filterGroup.FilterId, idList);
                }

                if (!idList.Contains(filterGroup.Id))
                {
                    idList.Add(filterGroup.Id);
                }
            }
        }

        private static void AppendFilters(IEnumerable<Filter> filters,
            IDictionary<Guid, List<Guid>> filtersBySubject)
        {
            foreach (var filter in filters)
            {
                if (!filtersBySubject.TryGetValue(filter.SubjectId, out var idList))
                {
                    idList = new List<Guid>();
                    filtersBySubject.Add(filter.SubjectId, idList);
                }

                if (!idList.Contains(filter.Id))
                {
                    idList.Add(filter.Id);
                }
            }
        }

        private static FootnoteViewModel BuildFootnoteViewModel(Footnote footnote,
            IEnumerable<Guid> subjectIds,
            IReadOnlyDictionary<Guid, List<Guid>> filtersBySubject,
            IReadOnlyDictionary<Guid, List<Guid>> filterGroupsByFilter,
            IReadOnlyDictionary<Guid, List<Guid>> filterItemsByFilterGroup,
            IReadOnlyDictionary<Guid, List<Guid>> indicatorGroupsBySubject,
            IReadOnlyDictionary<Guid, List<Guid>> indicatorsByIndicatorGroup,
            IEnumerable<Guid> selectedSubjects,
            IEnumerable<Guid> selectedFilters,
            IEnumerable<Guid> selectedFilterGroups)
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
                                                                     filterItemsByFilterGroup
                                                                         .GetValueOrDefault(filterGroupId)
                                                                         ?.Select(filterItemId =>
                                                                             filterItemId.ToString()) ??
                                                                     new List<string>(),
                                                                 Selected = selectedFilterGroups.Contains(filterGroupId)
                                                             }) ?? new Dictionary<Guid, FootnoteFilterGroupViewModel>(),
                                          Selected = selectedFilters.Contains(filterId)
                                      }) ?? new Dictionary<Guid, FootnoteFilterViewModel>(),
                        IndicatorGroups = indicatorGroupsBySubject.GetValueOrDefault(subjectId)?.ToDictionary(
                                              indicatorGroupId => indicatorGroupId,
                                              indicatorGroupId => new FootnoteIndicatorGroupViewModel
                                              {
                                                  Indicators =
                                                      indicatorsByIndicatorGroup.GetValueOrDefault(indicatorGroupId)
                                                          ?.Select(indicatorId => indicatorId.ToString()) ??
                                                      new List<string>()
                                              }) ?? new Dictionary<Guid, FootnoteIndicatorGroupViewModel>(),
                        Selected = selectedSubjects.Contains(subjectId)
                    })
            };
        }
    }
}
