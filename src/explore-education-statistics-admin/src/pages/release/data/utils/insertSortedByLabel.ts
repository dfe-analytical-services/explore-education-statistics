import { naturalComparator } from '@common/utils/array/naturalOrderBy';

/**
 * Insert an item into a list that is already ordered by label, keeping it
 * in natural alphabetical order (e.g. 'Year 2' before 'Year 10').
 *
 * Mutates the list, so can be used directly on Immer drafts. The item is
 * placed after any existing items with an equal label.
 *
 * Mapping lists are sorted with {@link naturalComparator} when converted
 * from the API response (see getApiDataSet*Mappings), so this keeps that
 * order when items move between lists after a mapping update.
 */
export default function insertSortedByLabel<T>(
  items: T[],
  item: T,
  getLabel: (item: T) => string,
): void {
  const label = getLabel(item);
  const index = items.findIndex(
    existing => naturalComparator(getLabel(existing), label) > 0,
  );

  if (index === -1) {
    items.push(item);
  } else {
    items.splice(index, 0, item);
  }
}
