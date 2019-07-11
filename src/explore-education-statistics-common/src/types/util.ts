/**
 * Enumerates the typical comparison
 * integers of 1, 0 and -1.
 */
export enum Comparison {
  GreaterThan = 1,
  EqualTo = 0,
  LessThan = -1,
}

export interface Dictionary<T> {
  [key: string]: T;
}

/**
 * Extract a type which are the keys from T
 * that have value matching the type U.
 */
export type KeysWithType<T, U> = {
  [K in keyof T]: T[K] extends U ? K : never;
}[keyof T];

/**
 * Pick key/value pairs from T that
 * have a value matching type U.
 */
export type PickByType<T, U> = Pick<T, KeysWithType<T, U>>;

/**
 * Remove any specified keys from T, that exist on T.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type OmitStrict<T, K extends keyof T> = T extends any
  ? Pick<T, Exclude<keyof T, K>>
  : never;

/**
 * Remove any keys from T and replace them with
 * the corresponding keys from U.
 */
export type Overwrite<T, U> = OmitStrict<T, keyof T & keyof U> & U;

/**
 * Make specified keys from T optional.
 */
export type PartialBy<T, K extends keyof T> = OmitStrict<T, K> &
  Partial<Pick<T, K>>;

/**
 * Construct a type with a set of optional properties K of type T.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type PartialRecord<K extends keyof any, T> = { [P in K]?: T };
