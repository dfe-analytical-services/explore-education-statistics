import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import { LegendPosition } from '@common/modules/charts/types/legend';

export const colours: string[] = [
  '#1d70b8',
  '#912b88',
  '#d53880',
  '#f499be',
  '#f47738',
  '#b58840',
  '#85994b',
  '#28a197',
  '#505a5f',
  '#0b0c0c',
  '#d4351c',
  '#ffdd00',
  '#00703c',
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
];

export const lineStyles: LineStyle[] = ['dashed', 'dotted', 'solid'];
