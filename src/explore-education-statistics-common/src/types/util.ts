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

export type Pair<Key, Value> = [Key, Value];

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
 * Make all properties (including nested objects) in T optional.
 */
export type NestedPartial<T> = {
  // eslint-disable-next-line @typescript-eslint/ban-types
  [P in keyof T]?: T[P] extends object | undefined ? NestedPartial<T[P]> : T[P];
};

/**
 * Make readonly properties writable.
 */
export type Writeable<T> = {
  -readonly [P in keyof T]: T[P];
};
