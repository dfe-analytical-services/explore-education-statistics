import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import { LegendPosition } from '@common/modules/charts/types/legend';

export const colours: string[] = [
  '#12436D',
  '#F46A25',
  '#801650',
  '#28A197',
  '#2073BC',
  '#6BACE6',
  '#BFBFBF',
];

export const legendPositions: LegendPosition[] = ['bottom', 'top', 'none'];

export const symbols: ChartSymbol[] = [
  'circle',
  'cross',
  'diamond',
  'square',
  'star',
  'triangle',
  'wye',
  'none',
];

export const lineStyles: LineStyle[] = ['dashed', 'dotted', 'solid'];
