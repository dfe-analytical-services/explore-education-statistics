import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';
import clamp from 'lodash/clamp';

export const defaultMaxDecimalPlaces = 2;

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
 * the formatted number should have.
 */
export default function formatPretty(
  value: string | number,
  unit?: string,
  decimalPlaces?: number,
): string {
  let numberValue: number;

  if (typeof value === 'string') {
    numberValue = Number(value);

    if (Number.isNaN(numberValue) || value.trim() === '') {
      return value;
    }
  } else {
    numberValue = value;
  }

  let formattedValue: string;

  if (typeof decimalPlaces === 'undefined') {
    const minDecimalPlaces = clamp(
      countDecimalPlaces(value) ?? 0,
      0,
      defaultMaxDecimalPlaces,
    );

    formattedValue = numberValue.toLocaleString('en-GB', {
      maximumFractionDigits: defaultMaxDecimalPlaces,
      minimumFractionDigits: minDecimalPlaces,
    });
  } else {
    formattedValue = numberValue.toLocaleString('en-GB', {
      maximumFractionDigits: decimalPlaces,
      minimumFractionDigits: decimalPlaces,
    });
  }

  if (unit) {
    switch (unit) {
      case '£':
        if (numberValue >= 0) {
          return `£${formattedValue}`;
        }
        return `-£${formattedValue.substring(1)}`;
      case '£m':
        if (numberValue >= 0) {
          return `£${formattedValue}m`;
        }
        return `-£${formattedValue.substring(1)}m`;
      default:
        return `${formattedValue}${unit}`;
    }
  }

  return formattedValue;
}
