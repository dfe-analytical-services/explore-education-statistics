import { SubjectMeta } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import mapValues from 'lodash/mapValues';
import { FiltersFormValues } from '../FiltersForm';
import { OptionLabelsMap } from './getFilterHierarchyLabelsMap';

/**
 * Amends submitted option id lists to include parent option id(s) and child total option id(s)
 * @param selectedValues submitted selected filter options
 * @param filterHierarchies the hierarchies to sift through to find appropriate parent and child related ids to add to return value
 * @param optionLabelsMap to help locate total options
 * @returns a amended list of selected option ids to include option parent ids and child total ids to help API select appropriate tabel rows to return
 */
export default function getFilterHierarchyRelatedOptionIds(
  selectedValues: FiltersFormValues['filterHierarchies'],
  filterHierarchies: SubjectMeta['filterHierarchies'],
  optionLabelsMap: OptionLabelsMap,
): FiltersFormValues['filterHierarchies'] {
  if (!filterHierarchies) {
    return selectedValues;
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

  const selectedRelatedValues = mapValues(selectedValues, selectedOptionIds => {
    return Array.from(
      new Set( // remove duplicate ids
        selectedOptionIds.flatMap(optionId => {
          return getOptionChildTotalsRecursively(
            getOptionParentsRecursively([optionId], optionParentRelationsMap),
            optionChildrenRelationsMap,
            optionLabelsMap,
          );
        }),
      ),
    );
  });
  return selectedRelatedValues;
}

function getOptionParentsRecursively(
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

function getOptionChildTotalsRecursively(
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
