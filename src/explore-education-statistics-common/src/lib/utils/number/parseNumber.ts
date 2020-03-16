/**
 * Parse a {@param value} to a number or
 * return the {@param defaultValue}. If the value
 * is an empty string, we don't convert this to 0.
 *
 * This is useful for converting NaNs to
 * some other value or undefined.
 */
export default function parseNumber(
  value: unknown,
  defaultValue?: number,
): number | undefined {
  let parsed: unknown;

  if (typeof value === 'string' && value !== '') {
    parsed = Number(value);
  } else {
    parsed = value;
  }

  if (typeof parsed === 'number') {
    if (Number.isNaN(parsed)) {
      return defaultValue;
    }

    return parsed;
  }

  return defaultValue;
}
