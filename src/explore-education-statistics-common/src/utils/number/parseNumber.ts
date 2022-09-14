/**
 * Parse a {@param value} to a number or
 * return undefined. If the value is an
 * empty string, we don't convert this to 0.
 *
 * This is useful for converting NaNs to undefined
 * so that we can chain it with null coalescing
 * for a default value e.g.
 *
 * ```typescript
 * parseNumber(someNumber) ?? 100
 * ```
 */
export default function parseNumber(value: unknown): number | undefined {
  let parsed: unknown;

  if (typeof value === 'string' && value.trim() !== '') {
    parsed = Number(value);
  } else {
    parsed = value;
  }

  if (typeof parsed === 'number') {
    if (Number.isNaN(parsed)) {
      return undefined;
    }

    return parsed;
  }

  return undefined;
}
