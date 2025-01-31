/**
 * This is a typed version of `Object.keys` which returns
 * string literals instead of `string[]`.
 */
export default function typedKeys<T extends object>(object: T): (keyof T)[] {
  return Object.keys(object) as (keyof T)[];
}
