import { Dictionary } from '@common/types/util';

/**
 * Map an {@param object} to a new object with the same keys,
 * but newly mapped values using a {@param mappingFunction}
 * that takes both the key and value (the entry).
 */
export default function mapValuesWithKeys<
  T extends Dictionary<T[keyof T]>,
  TResult,
>(
  object: T,
  mappingFunction: (key: string, value: T[keyof T]) => TResult,
): Dictionary<TResult> {
  const mappedObject: Dictionary<TResult> = {};

  return Object.entries(object).reduce((acc, entry) => {
    // eslint-disable-next-line no-param-reassign
    acc[entry[0]] = mappingFunction(entry[0], entry[1]);
    return acc;
  }, mappedObject);
}
