/**
 * This is unsafe as we don't do any further parsing
 * of the {@param value} to ensure it is a number,
 * but will be slightly more performant.
 */
export function unsafeCountDecimals(value: string) {
  const decimals = value.split('.')[1];
  return decimals ? decimals.length : 0;
}

/**
 * Count the number of decimal places in a {@param value}.
 */
export default function countDecimals(value: number | string): number {
  if (Number.isNaN(Number(value))) {
    return 0;
  }

  return unsafeCountDecimals(value.toString());
}
