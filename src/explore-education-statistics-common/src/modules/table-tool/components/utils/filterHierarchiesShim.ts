import {
  FullTableQuery,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from 'lodash';
import getFilterHierarchyLabelsMap, {
  OptionLabelsMap,
} from './getFilterHierarchyLabelsMap';

/**
 * the "tableBuilderService.getTableData" endpoint requires we provide all filters of a given table row to be able to return the correct resulting table
 * this shim augments selected filters with additional filter options that are related to selected filters that appear in filter hierarchies
 * * for each selected filter that appears in a filter hierarchy:
 * * find the parent (& parents parent) of hierarchised selected filters
 * * also, find the children (& childrens children) of hierarchised selected filters that has a label "Total"
 *
 * @param selectedFilters all selected filters.
 * @param subjectMeta full subject meta to be used to determine hierarchised selected filters.
 */
export default function filterHierarchiesShim(
  selectedFilters: FullTableQuery['filters'],
  subjectMeta: SubjectMeta,
): FullTableQuery['filters'] {
  const { filterHierarchies } = subjectMeta;
  if (!filterHierarchies?.length) return selectedFilters;

  const hierarchiedFilterIds = filterHierarchies.flatMap(hierarchy => {
    return [
      ...hierarchy.map(({ filterId }) => filterId),
      hierarchy.at(-1)?.childFilterId,
    ];
  });

  const hierarchiedFilters = Object.values(subjectMeta.filters).filter(filter =>
    hierarchiedFilterIds.includes(filter.id),
  );

  const optionLabelsMap = getFilterHierarchyLabelsMap(hierarchiedFilters);

  // flatten and merge *all* filter hierarchy tiers together, giving us a map
  // of *all* parent option to child options relationships across all hierarchies and levels
  const optionChildrenRelationsMap: Dictionary<string[]> = filterHierarchies
    .flat()
    .reduce((acc, { hierarchy }) => {
      return { ...acc, ...hierarchy };
    }, {});

  const optionParentRelationsMap: Dictionary<string> = Object.fromEntries(
    Object.entries(optionChildrenRelationsMap).flatMap(([parent, children]) =>
      children.map(child => [child, parent]),
    ),
  );

  const selectedRelatedValues = Array.from(
    new Set(
      selectedFilters.flatMap(optionId => {
        return getOptionChildTotalsRecursively(
          getOptionParentsRecursively([optionId], optionParentRelationsMap),
          optionChildrenRelationsMap,
          optionLabelsMap,
        );
      }),
    ),
  );

  return selectedRelatedValues;
}

export function getOptionParentsRecursively(
  optionIds: string[],
  optionParentRelationsMap: Dictionary<string>,
): string[] {
  const optionId = optionIds[0] ?? '';
  const parentOptionId = optionParentRelationsMap[optionId];

  if (!parentOptionId) {
    return optionIds;
  }
  return getOptionParentsRecursively(
    [parentOptionId, ...optionIds],
    optionParentRelationsMap,
  );
}

export function getOptionChildTotalsRecursively(
  optionIds: string[],
  optionChildrenRelationsMap: Dictionary<string[]>,
  optionLabelsMap: OptionLabelsMap,
) {
  const parentOptionId = optionIds.at(-1) ?? '';

  const childTotalId = optionChildrenRelationsMap[parentOptionId]?.find(
    childOptionId =>
      optionLabelsMap[childOptionId]?.toLocaleLowerCase() === 'total',
  );
  if (!childTotalId) {
    return optionIds;
  }
  return getOptionChildTotalsRecursively(
    [...optionIds, childTotalId],
    optionChildrenRelationsMap,
    optionLabelsMap,
  );
}
