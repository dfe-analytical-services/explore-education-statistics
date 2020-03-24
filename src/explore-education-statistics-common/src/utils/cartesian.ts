/**
 * Form a cartesian product of some {@param lists}.
 * e.g. [a, b], [c, d] => [ac], [ad], [bc], [bd].
 *
 * Note that if one of the lists is empty, this will
 * result in an empty list being returned as this is
 * the equivalent of multiplying by 0.
 */
export default function cartesian<T>(...lists: T[][]): T[][] {
  if (lists.length === 0) {
    return [];
  }

  return lists.reduce<T[][]>(
    (acc, list) => acc.flatMap(row => list.map(item => [...row, item])),
    [[]],
  );
}
