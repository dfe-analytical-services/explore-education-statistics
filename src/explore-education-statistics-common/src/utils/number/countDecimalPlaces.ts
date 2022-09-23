const INTEGER_WITH_DECIMAL_PLACES = /^[+-]?\d+\.0+$/;

/**
 * Count the number of decimal places in a {@param value}.
 */
export default function countDecimalPlaces(
  value: number | string,
): number | undefined {
  let parsed = value;

  if (typeof value === 'string') {
    const trimmed = value.trim();

    if (trimmed === '') {
      return undefined;
    }

    // Check that the value isn't an integer with decimal places i.e. '4.00'.
    // We still want to count the decimal places, so we can't cast to a
    // number or the decimal places won't be preserved.
    if (!INTEGER_WITH_DECIMAL_PLACES.test(trimmed)) {
      parsed = Number(value);
    }
  }

  if (typeof parsed === 'number' && !Number.isFinite(parsed)) {
    return undefined;
  }

  const decimalPlaces = parsed.toString().split('.')[1];
  return decimalPlaces ? decimalPlaces.length : 0;
}
