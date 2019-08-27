using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public abstract class AbstractTableBuilderSubjectMetaService
    {
        private readonly IFilterItemService _filterItemService;
        private static IEqualityComparer<Filter> FilterComparer { get; } = new FilterEqualityComparer();
        private static IEqualityComparer<FilterGroup> FilterGroupComparer { get; } = new FilterGroupEqualityComparer();

        protected AbstractTableBuilderSubjectMetaService(IFilterItemService filterItemService)
        {
            _filterItemService = filterItemService;
        }

        protected Dictionary<string, TableBuilderFilterMetaViewModel> GetFilters(IQueryable<Observation> observations)
        {
            return _filterItemService.GetFilterItemsIncludingFilters(observations)
                .GroupBy(item => item.FilterGroup.Filter, item => item, FilterComparer)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new TableBuilderFilterMetaViewModel
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Options = itemsGroupedByFilter
                            .GroupBy(item => item.FilterGroup, item => item, FilterGroupComparer)
                            .ToDictionary(
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

        private sealed class FilterEqualityComparer : IEqualityComparer<Filter>
        {
            public bool Equals(Filter x, Filter y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Filter obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        private sealed class FilterGroupEqualityComparer : IEqualityComparer<FilterGroup>
        {
            public bool Equals(FilterGroup x, FilterGroup y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(FilterGroup obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}