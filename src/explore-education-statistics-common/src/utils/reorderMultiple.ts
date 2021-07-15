import reorder from '@common/utils/reorder';
import max from 'lodash/max';

interface ReorderMultipleOptions<T> {
  list: T[];
  destinationIndex: number;
  selectedIndices: number[];
}

/**
 * Reorder the values in a {@param list} so that
 * the values at the {@param selectedIndices} are
 * moved to a new {@param destinationIndex}.
 *
 * Typically this should be used in sortable lists
 * when trying to reorder multiple elements
 * simultaneously.
 */
export default function reorderMultiple<T>({
  list,
  destinationIndex,
  selectedIndices,
}: ReorderMultipleOptions<T>): T[] {
  if (selectedIndices.length === 0) {
    return list;
  }

  if (selectedIndices.length === 1) {
    return reorder(list, selectedIndices[0], destinationIndex);
  }

  const destinationItem = list[destinationIndex];

  if (!destinationItem) {
    return list;
  }

  const [selectedList, newList] = list.reduce<[T[], T[]]>(
    (acc, item, index) => {
      if (selectedIndices.includes(index)) {
        acc[0].push(item);
      } else {
        acc[1].push(item);
      }

      return acc;
    },
    [[], []],
  );

  const maxIndex = max(selectedIndices) as number;

  let insertIndex = destinationIndex;

  // Last selected item is before the final destination
  // position, so we need to offset this by 1 for maths reasons
  if (maxIndex < insertIndex) {
    insertIndex += 1;
  }

  newList.splice(insertIndex, 0, ...selectedList);

  return newList;
}
