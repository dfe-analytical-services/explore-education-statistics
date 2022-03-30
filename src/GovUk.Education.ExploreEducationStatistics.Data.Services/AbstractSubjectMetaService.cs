#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public abstract class AbstractSubjectMetaService
    {
        protected readonly IFilterItemRepository _filterItemRepository;
        protected static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

        protected AbstractSubjectMetaService(IFilterItemRepository filterItemRepository)
        {
            _filterItemRepository = filterItemRepository;
        }

        protected Dictionary<string, FilterMetaViewModel> BuildFilterHierarchy(IEnumerable<FilterItem> filterItems)
        {
            return filterItems
                .GroupBy(item => item.FilterGroup.Filter, item => item, Filter.IdComparer)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new FilterMetaViewModel
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Name = itemsGroupedByFilter.Key.Name,
                        Options = itemsGroupedByFilter
                            .GroupBy(item => item.FilterGroup, item => item, FilterGroup.IdComparer)
                            .OrderBy(items => items.Key.Label.ToLower() != "total")
                            .ThenBy(items => items.Key.Label, LabelComparer)
                            .ToDictionary(
                                itemsGroupedByFilterGroup => itemsGroupedByFilterGroup.Key.Label.PascalCase(),
                                itemsGroupedByFilterGroup => 
                                    BuildFilterItemsViewModel(itemsGroupedByFilterGroup.Key, itemsGroupedByFilterGroup)
                            ),
                        TotalValue = GetTotalValue(itemsGroupedByFilter)
                    });
        }

        protected static List<IndicatorMetaViewModel> BuildIndicatorViewModels(IEnumerable<Indicator> indicators)
        {
            return indicators
                .OrderBy(indicator => indicator.Label, LabelComparer)
                .Select(indicator => new IndicatorMetaViewModel
                {
                    Label = indicator.Label,
                    Name = indicator.Name,
                    Unit = indicator.Unit,
                    Value = indicator.Id.ToString(),
                    DecimalPlaces = indicator.DecimalPlaces
                })
                .ToList();
        }

        protected static FilterItemsMetaViewModel BuildFilterItemsViewModel(FilterGroup filterGroup,
            IEnumerable<FilterItem> filterItems)
        {
            return new()
            {
                Label = filterGroup.Label,
                Options = filterItems
                    .OrderBy(item => item.Label.ToLower() != "total")
                    .ThenBy(item => item.Label, LabelComparer)
                    .Select(item => new LabelValue(item.Label, item.Id.ToString()))
                    .ToList()
            };
        }

        protected static IEnumerable<LocationAttributeViewModel> DeduplicateLocationViewModels(
            IEnumerable<LocationAttributeViewModel> viewModels)
        {
            var list = viewModels.ToList();

            // TODO EES-2954 SOW8 Review if these cases are still applicable and enhance if necessary

            /*
             The list of Location attributes should in theory already be unique.
             If they are not, there's three possibilities:
              * Duplicates exist where the label-value pairs are distinct but the Level attribute is different
                i.e. where the same Location attribute is reused across multiple Geographic Levels e.g. LA and LAD.
                These need transforming to give them distinct labels.
              * Duplicates where the labels are the same but the values are different.
                These need transforming to give them distinct labels.
              * Duplicates where the values are the same but the labels are different.
                These don't need any action.
            */

            var case1 = list
                .GroupBy(model => (model.Value, model.Label))
                .Where(grouping => grouping.Count() > 1)
                .SelectMany(grouping => grouping)
                .ToList();

            var case2 = list.Except(case1)
                .GroupBy(model => model.Label)
                .Where(grouping => grouping.Count() > 1)
                .SelectMany(grouping => grouping)
                .ToList();

            if (!(case1.Any() || case2.Any()))
            {
                return list;
            }

            return list.Select(value =>
            {
                if (case1.Contains(value))
                {
                    value.Label += $" ({value.Level})";
                }

                if (case2.Contains(value))
                {
                    value.Label += $" ({value.Value})";
                }

                return value;
            });
        }

        private string GetTotalValue(IEnumerable<FilterItem> filterItems)
        {
            return _filterItemRepository.GetTotal(filterItems)?.Id.ToString() ?? string.Empty;
        }
    }
}
