import { KeysWithType } from '@common/types';

const collator = new Intl.Collator('en', {
  numeric: true,
  ignorePunctuation: true,
});

export const naturalComparator = collator.compare;

type Key<T> = KeysWithType<T, string | number>;
type CustomKey<T> = (item: T) => string | number;

export type OrderKeys<T> = Key<T> | CustomKey<T> | (Key<T> | CustomKey<T>)[];

export type OrderDirection = 'asc' | 'desc';

type Criteria<T> = (item: T) => string;

/**
 * Naturally order some {@param items} using a set of {@param keys}
 * matched to specific ordering {@param directions}.
 *
 * This should be used in preference to {@see orderBy} for
 * user-facing data as the ordering is usually more natural to humans.
 *
 * Defaults to ordering in ascending order when no direction(s) specified.
 */
export default function naturalOrderBy<T>(
  items: T[],
  keys: OrderKeys<T>,
  directions: OrderDirection | OrderDirection[] = [],
): T[] {
  const keysArray = Array.isArray(keys) ? keys : [keys];
  const directionsArray = Array.isArray(directions) ? directions : [directions];

  const criterias: Criteria<T>[] = keysArray.map(key => {
    return item => {
      const value = typeof key === 'function' ? key(item) : item[key];

      const isComparable =
        typeof value === 'string' || typeof value === 'number';

      // Make incomparable values empty strings
      // so that we don't order them.
      return isComparable ? `${value}` : '';
    };
  });

  return [...(items ?? [])].sort((x, y) => {
    let index = 0;
    const lastIndex = criterias.length - 1;

    while (index <= lastIndex) {
      const criteria = criterias[index];
      const comparison = naturalComparator(criteria(x), criteria(y));

      if (comparison !== 0) {
        const direction = directionsArray[index] ?? 'asc';
        return comparison * (direction === 'asc' ? 1 : -1);
      }

      index += 1;
    }

    return 0;
  });
}
