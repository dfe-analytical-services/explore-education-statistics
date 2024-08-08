/**
 * Intersperse a separator item between each item in an array.
 */
export default function intersperse<TItem, TSeparator>(
  items: TItem[],
  separator: (index: number) => TSeparator,
): (TItem | TSeparator)[] {
  return items.reduce<(TItem | TSeparator)[]>((acc, item, index) => {
    if (index === items.length - 1) {
      acc.push(item);
    } else {
      acc.push(item, separator(index));
    }

    return acc;
  }, []);
}
