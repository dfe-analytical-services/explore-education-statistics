/**
 * Test if a {@param value} is contained in some {@param items},
 * casting the value's type to that of an individual item if it is.
 */
export default function isOneOf<TItem>(
  value: unknown,
  items: readonly TItem[],
): value is TItem {
  return items.includes(value as TItem);
}
