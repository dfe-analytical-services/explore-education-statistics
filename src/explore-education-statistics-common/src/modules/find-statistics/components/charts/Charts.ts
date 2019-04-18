export const colours: string[] = [
  '#4763a5',
  '#f5a450',
  '#005ea5',
  '#800080',
  '#C0C0C0',
];

export const symbols: (
  | 'circle'
  | 'cross'
  | 'square'
  | 'star'
  | 'triangle')[] = ['circle', 'square', 'triangle', 'cross', 'star'];

export function parseCondensedTimePeriodRange(
  range: string,
  separator: string = '/',
) {
  return [range.substring(0, 4), range.substring(4, 6)].join(separator);
}
