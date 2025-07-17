import { SubjectMetaFilter } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';

export type OptionLabelsMap = Dictionary<string>;

/**
 * Maps all filters and filter options by their id with their label strings
 * @param filters subject meta filters
 * @returns dictionary of filter and filter option labels, keyed by their id
 */
export default function getFilterHierarchyLabelsMap(
  filters: SubjectMetaFilter[],
): OptionLabelsMap {
  const map: OptionLabelsMap = {};

  filters.forEach(filter => {
    map[filter.id] = filter.legend;
    Object.values(filter.options).forEach(filterOptions => {
      map[filterOptions.id] = filterOptions.label;
      filterOptions.options.forEach(filterGroupOption => {
        map[filterGroupOption.value] = filterGroupOption.label;
      });
    });
  });

  return map;
}

export function isOptionTotal(map: OptionLabelsMap, optionId: string): boolean {
  return map[optionId]?.toLocaleLowerCase() === 'total';
}
