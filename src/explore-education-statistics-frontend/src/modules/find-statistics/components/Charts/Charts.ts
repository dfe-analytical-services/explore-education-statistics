export const colours: string[] = [
  '#b10e1e',
  '#006435',
  '#005ea5',
  '#800080',
  '#C0C0C0',
];

export const symbols: any[] = ['circle', 'square', 'triangle', 'cross', 'star'];

export function parseCondensedYearRange(
  range: string,
  separator: string = '/',
) {
  return [range.substring(0, 4), range.substring(4, 6)].join(separator);
}
