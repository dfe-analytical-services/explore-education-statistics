import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import {
  LegendInlinePosition,
  LegendLabelColour,
  LegendPosition,
} from '@common/modules/charts/types/legend';

export interface Colour {
  label: string;
  value: string;
}

export const colours: Colour[] = [
  { label: 'Dark blue', value: '#12436D' },
  { label: 'Orange', value: '#F46A25' },
  { label: 'Dark pink', value: '#801650' },
  { label: 'Turquoise', value: '#28A197' },
  { label: 'Medium blue', value: '#2073BC' },
  { label: 'Light blue', value: '#6BACE6' },
  { label: 'Grey', value: '#BFBFBF' },
];

export const mapCategoricalDataColours: string[] = [
  '#12436D',
  '#28A197',
  '#801650',
  '#F46A25',
  '#3D3D3D',
  '#A285D1',
];

export const legendPositions: LegendPosition[] = [
  'inline',
  'bottom',
  'top',
  'none',
];

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

export const legendInlinePositions: LegendInlinePosition[] = [
  'right',
  'above',
  'below',
];

export const legendLabelColours: LegendLabelColour[] = ['black', 'inherit'];

export const lineChartDataLabelPositions = ['above', 'below'];

export const barChartDataLabelPositions = ['inside', 'outside'];

export const axisTickStyle = { fill: '#0b0c0c' };
