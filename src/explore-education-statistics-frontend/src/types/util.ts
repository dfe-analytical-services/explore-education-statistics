/**
 * Enumerates the typical comparison
 * integers of 1, 0 and -1.
 */
export enum Comparable {
  GreaterThan = 1,
  Equal = 0,
  LessThan = -1,
}

/**
 * Extract a type which are the keys from T
 * that have value matching the type U.
 */
export type KeysWithType<T, U> = {
  [K in keyof T]: T[K] extends U ? K : never
}[keyof T];

/**
 * Remove any specified keys from T.
 */
export type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>;

/**
 * Remove any keys from T and replace them with
 * the corresponding keys from U.
 */
export type Overwrite<T, U> = Omit<T, keyof T & keyof U> & U;
