/**
 * Reorder the values in a {@param list} so that the
 * corresponding value at {@param startIndex} replaces
 * the value at {@param endIndex} and shifts the other
 * values around to match this movement.
 *
 * Typically this should be used in sortable lists.
 */
export default function reorder<T>(
  list: T[],
  startIndex: number,
  endIndex: number,
): T[] {
  const newList = [...list];
  const [removed] = newList.splice(startIndex, 1);
  newList.splice(endIndex, 0, removed);

  return newList;
}
