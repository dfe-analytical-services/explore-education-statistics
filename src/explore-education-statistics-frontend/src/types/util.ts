/**
 * Extract a type which are the keys from T
 * that have value matching the type U.
 */
export type KeysWithType<T, U> = {
  [K in keyof T]: T[K] extends U ? K : never
}[keyof T];
