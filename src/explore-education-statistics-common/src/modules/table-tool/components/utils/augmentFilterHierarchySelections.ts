import { SubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import mapValues from 'lodash/mapValues';
import { FiltersFormValues } from '../FiltersForm';
import { OptionLabelsMap } from './getFilterHierarchyLabelsMap';

/**
 * Augments filterHierarchySelections to dictionarys where the key is the selected option and the value is all related options
 * @param filterHierarchySelections submitted selected filter options
 * @param filterHierarchies the hierarchies to sift through to find appropriate parent and child related ids to add to return value
 * @param optionLabelsMap to help locate total options
 */
export default function augmentFilterHierarchySelections(
  filterHierarchySelections: FiltersFormValues['filterHierarchies'],
  filterHierarchies: SubjectMeta['filterHierarchies'],
  optionLabelsMap: OptionLabelsMap,
): Dictionary<Dictionary<string[]>> {
  if (!filterHierarchies) {
    return mapValues(filterHierarchySelections, hierarchySelections =>
      Object.fromEntries(
        hierarchySelections.map(fhSelection => [fhSelection, [fhSelection]]),
      ),
    );
  }

  // flatten and merge *all* FilterHierarchyTier["hierarchies"] together,
  // giving us a map of *all* parent option to child options relationships across all hierarchies and levels
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

  const augmentedFilterHierarchySelections = mapValues(
    filterHierarchySelections,
    selectedOptionIds => {
      return Object.fromEntries(
        selectedOptionIds.map(optionId => {
          return [
            optionId,
            getOptionChildTotalsRecursively(
              getOptionParentsRecursively([optionId], optionParentRelationsMap),
              optionChildrenRelationsMap,
              optionLabelsMap,
            ),
          ];
        }),
      );
    },
  );

  return augmentedFilterHierarchySelections;
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
