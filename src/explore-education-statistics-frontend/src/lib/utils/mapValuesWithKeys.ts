import { Dictionary } from 'src/types/util';

/**
 * Map an {@param object} to a new object with the same keys,
 * but newly mapped values using a {@param mappingFunction}
 * that takes both the key and value (the entry).
 */
export default function mapValuesWithKeys<T extends Dictionary<any>, TResult>(
  object: T,
  mappingFunction: (entry: [string, T[keyof T]]) => TResult,
): Dictionary<TResult> {
  return Object.entries(object).reduce((acc, entry) => {
    return {
      ...acc,
      [entry[0]]: mappingFunction(entry),
    };
  }, {});
}
