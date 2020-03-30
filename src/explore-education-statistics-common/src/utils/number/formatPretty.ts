/**
 * Return a formatted {@param value} in a pretty format
 * i.e. 10,000,000.000.
 *
 * {@param maxDecimals} can be optionally used
 * determine the number of decimal places that
 * the formatted number can have.
 *
 * This also accepts strings and preserves their
 * decimal places, even if there are trailing zeros.
 */
export default function formatPretty(
  value: string | number,
  unit?: string,
  maxDecimals = 2,
): string {
  let formattedValue;
  if (typeof value === 'string') {
    const numberValue = Number(value);

    if (Number.isNaN(numberValue)) {
      return value;
    }

    const decimals = value.split('.')[1];
    const decimalPlaces = decimals ? decimals.length : 0;

    if (decimalPlaces > 0) {
      formattedValue = numberValue.toLocaleString('en-GB', {
        maximumFractionDigits: maxDecimals,
        minimumFractionDigits:
          decimalPlaces > maxDecimals ? maxDecimals : decimalPlaces,
      });
    } else {
      formattedValue = Number(value).toLocaleString('en-GB', {
        maximumFractionDigits: maxDecimals,
      });
    }
  } else {
    formattedValue = value.toLocaleString('en-GB', {
      maximumFractionDigits: maxDecimals,
    });
  }
  if (unit) {
    return unit === '£'
      ? `${unit}${formattedValue}`
      : `${formattedValue}${unit}`;
  }
  return formattedValue;
}
