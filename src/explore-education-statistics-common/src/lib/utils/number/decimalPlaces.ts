/**
 * Get the number of decimal places for a {@param value}.
 */
export default function decimalPlaces(value: number) {
  const [_, decimals] = value.toString().split('.');
  return decimals ? decimals.length : 0;
}
