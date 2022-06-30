/**
 * Adds commas to large numbers as recommended by the GDS styleguide.
 */
export default function numberWithCommas(value: number): string {
  return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}
