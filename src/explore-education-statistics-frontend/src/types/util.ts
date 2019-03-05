/**
 * Extract a type which are the keys from T
 * that have value matching the type U.
 */
export type KeysWithType<T, U> = {
  [K in keyof T]: T[K] extends U ? K : never
}[keyof T];

export type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>;
