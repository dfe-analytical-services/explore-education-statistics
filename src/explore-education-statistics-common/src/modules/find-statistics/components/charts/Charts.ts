import { Axis, ReferenceLine } from '@common/services/publicationService';
import { CharacteristicsData } from '@common/services/tableBuilderService';

export const colours: string[] = [
  '#b10e1e',
  '#006435',
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

export interface ChartProps {
  characteristicsData: CharacteristicsData;
  chartDataKeys: string[];
  labels: { [key: string]: string };
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  referenceLines?: ReferenceLine[];
}
