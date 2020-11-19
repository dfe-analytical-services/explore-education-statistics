/**
 * Count the number of decimal places in a {@param value}.
 */
export default function countDecimals(value: number): number {
  const decimals = value.toString().split('.')[1];
  return decimals ? decimals.length : 0;
}
