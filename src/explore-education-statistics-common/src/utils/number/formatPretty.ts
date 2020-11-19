/**
 * Return a formatted {@param value} in a pretty format
 * i.e. 10,000,000.000.
 *
 * {@param unit} can be used to add a unit to the
 * formatted value. We will try and handle different
 * units to get a result that looks the best.
 *
 * {@param decimalPlaces} can be optionally used
 * determine the number of decimal places that
 * the formatted number can have.
 *
 * This also accepts strings and preserves their
 * decimal places, even if there are trailing zeros.
 */
export default function formatPretty(
  value: string | number,
  unit?: string,
  decimalPlaces?: number,
): string {
  let formattedValue;

  if (typeof value === 'string') {
    const numberValue = Number(value);

    if (Number.isNaN(numberValue)) {
      return value;
    }

    formattedValue = numberValue.toLocaleString('en-GB', {
      maximumFractionDigits: decimalPlaces,
      minimumFractionDigits: decimalPlaces,
    });
  } else {
    formattedValue = value.toLocaleString('en-GB', {
      maximumFractionDigits: decimalPlaces,
      minimumFractionDigits: decimalPlaces,
    });
  }

  if (unit) {
    switch (unit) {
      case '£':
        return `£${formattedValue}`;
      case '£m':
        return `£${formattedValue}m`;
      default:
        return `${formattedValue}${unit}`;
    }
  }

  return formattedValue;
}
