import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';
import clamp from 'lodash/clamp';

export const defaultMaxDecimalPlaces = 2;

/**
 * Return a formatted {@param value} in a pretty format
 * i.e. 10,000,000.000.
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
  switch (unit) {
    case 'string':
      return String(value);
    case '£': {
      const formattedNumber = formatNumber(value, decimalPlaces);
      if (formattedNumber.isNegative) {
        return `-£${formattedNumber.value.substring(1)}`;
      }
      return `£${formattedNumber.value}`;
    }
    case '£m': {
      const formattedNumber = formatNumber(value, decimalPlaces);
      if (formattedNumber.isNegative) {
        return `-£${formattedNumber.value.substring(1)}m`;
      }
      return `£${formattedNumber.value}m`;
    }
    case undefined: {
      const formattedNumber = formatNumber(value, decimalPlaces);
      return formattedNumber.value;
    }
    default: {
      const formattedNumber = formatNumber(value, decimalPlaces);
      return `${formattedNumber.value}${unit}`;
    }
  }
}

interface FormattedNumber {
  value: string;
  isNegative: boolean;
}

function formatNumber(
  value: string | number,
  decimalPlaces?: number,
): FormattedNumber {
  let numberValue: number;

  if (typeof value === 'string') {
    numberValue = Number(value);

    if (Number.isNaN(numberValue) || value.trim() === '') {
      return { value, isNegative: false };
    }
  } else {
    numberValue = value;
  }

  let formattedNumber: string;

  if (typeof decimalPlaces === 'undefined' || Math.sign(decimalPlaces) === -1) {
    const minDecimalPlaces = clamp(
      countDecimalPlaces(value) ?? 0,
      0,
      defaultMaxDecimalPlaces,
    );

    formattedNumber = numberValue.toLocaleString('en-GB', {
      maximumFractionDigits: defaultMaxDecimalPlaces,
      minimumFractionDigits: minDecimalPlaces,
    });
  } else {
    formattedNumber = numberValue.toLocaleString('en-GB', {
      maximumFractionDigits: decimalPlaces,
      minimumFractionDigits: decimalPlaces,
    });
  }

  return { value: formattedNumber, isNegative: numberValue < 0 };
}
