using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public abstract class AbstractTableBuilderSubjectMetaService
    {
        private readonly IFilterItemService _filterItemService;

        protected AbstractTableBuilderSubjectMetaService(IFilterItemService filterItemService)
        {
            _filterItemService = filterItemService;
        }

        protected Dictionary<string, TableBuilderFilterMetaViewModel> GetFilters(IQueryable<Observation> observations)
        {
            return _filterItemService.GetFilterItemsIncludingFilters(observations)
                .GroupBy(item => item.FilterGroup.Filter)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new TableBuilderFilterMetaViewModel
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Options = itemsGroupedByFilter.GroupBy(item => item.FilterGroup).ToDictionary(
                            itemsGroupedByFilterGroup => itemsGroupedByFilterGroup.Key.Label.PascalCase(),
                            itemsGroupedByFilterGroup =>
                                new TableBuilderFilterItemsMetaViewModel
                                {
                                    Label = itemsGroupedByFilterGroup.Key.Label,
                                    Options = itemsGroupedByFilterGroup.Select(item => new LabelValue
                                    {
                                        Label = item.Label,
                                        Value = item.Id.ToString()
                                    })
                                }),
                        TotalValue = GetTotalValue(itemsGroupedByFilter)
                    });
        }

        private string GetTotalValue(IEnumerable<FilterItem> filterItems)
        {
            return _filterItemService.GetTotal(filterItems)?.Id.ToString() ?? string.Empty;
        }
    }
}