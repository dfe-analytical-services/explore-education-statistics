import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

export type LegendPosition = 'none' | 'top' | 'bottom' | 'line';

export interface LegendConfiguration {
  position?: LegendPosition;
  items: LegendItem[];
}

export interface LegendItemConfiguration {
  label: string;
  colour: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
  lineChartLegendPosition?: LineChartLegendPosition;
}

export interface LegendItem extends LegendItemConfiguration {
  dataSet: DataSet;
}

export type LineChartLegendPosition = 'above' | 'below';
