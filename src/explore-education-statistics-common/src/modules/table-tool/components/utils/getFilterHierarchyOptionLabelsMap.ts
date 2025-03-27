import { SubjectMetaFilter } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';

export type OptionLabelsMap = Dictionary<string>;

export default function getFilterHierarchyOptionLabelsMap(
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
