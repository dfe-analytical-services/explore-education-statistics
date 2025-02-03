import reorder from '@common/utils/reorder';

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

  // Adapted from https://github.com/atlassian/react-beautiful-dnd/blob/master/stories/src/multi-drag/utils.js
  const insertAtIndex: number = (() => {
    const destinationIndexOffset: number = selectedIndices.reduce(
      (previous: number, current: number): number => {
        if (current === selectedIndices[0]) {
          return previous;
        }

        if (current >= destinationIndex) {
          return previous;
        }

        // the selected item is before the destination index
        // we need to account for this when inserting into the new location
        return previous + 1;
      },
      0,
    );

    const result: number = destinationIndex - destinationIndexOffset;
    return result;
  })();

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

  newList.splice(insertAtIndex, 0, ...selectedList);
  return newList;
}
