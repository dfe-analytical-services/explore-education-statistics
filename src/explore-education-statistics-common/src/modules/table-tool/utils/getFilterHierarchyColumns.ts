export default function getFilterHierarchyColumns(
  numerator: number,
  denominator: number,
): string {
  if (
    numerator % 1 !== 0 ||
    numerator < 0 ||
    numerator > 3 ||
    denominator % 1 !== 0 ||
    denominator < 2 ||
    denominator > 4 ||
    numerator === denominator
  ) {
    return '';
  }

  const prefix = 'govuk-grid-column';
  const numbers = ['one', 'two', 'three'];
  const chunkSizes = ['half', 'third', 'quarter'];

  return [
    prefix,
    numbers[numerator - 1],
    `${chunkSizes[denominator - 2]}${numerator > 1 ? 's' : ''}`,
  ].join('-');
}
