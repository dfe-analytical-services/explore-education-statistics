import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';
import round from 'lodash/round';

export interface RoundNearestOptions {
  /**
   * Explicitly round the value up or down to its
   * next nearest multiple. For example:
   * - 2.4 rounded up to nearest multiple of 5 returns 5
   * - 2.4 rounded down to nearest multiple of 5 returns 0
   */
  direction?: 'up' | 'down';
  /**
   * Control the precision of the rounded value.
   * We automatically apply some additional
   * rounding to prevent inexact floating point
   * numbers being returned.
   */
  precision?: number;
}

/**
 * Round a {@param value} to the nearest {@param multiple}.
 * Additional {@param options} can be provided to
 * control things like the rounding direction.
 */
export default function roundNearest(
  value: number,
  multiple: number,
  options?: RoundNearestOptions,
): number {
  let roundingFunction: (x: number) => number;

  switch (options?.direction) {
    case 'up':
      roundingFunction = Math.ceil;
      break;
    case 'down':
      roundingFunction = Math.floor;
      break;
    default:
      roundingFunction = Math.round;
      break;
  }

  const roundedValue = roundingFunction(value / multiple) * multiple;

  // Re-round using defined `precision` option or the
  // multiple's decimal places to correct potentially
  // inexact floating point numbers.
  return round(
    roundedValue,
    options?.precision ?? countDecimalPlaces(multiple),
  );
}

// Convenience wrappers to round up/down

export function roundDownToNearest(value: number, multiple: number): number {
  return roundNearest(value, multiple, { direction: 'down' });
}

export function roundUpToNearest(value: number, multiple: number): number {
  return roundNearest(value, multiple, { direction: 'up' });
}
