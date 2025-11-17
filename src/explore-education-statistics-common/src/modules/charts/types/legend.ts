import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

export type LegendPosition = 'none' | 'top' | 'bottom' | 'inline';

export interface LegendConfiguration {
  position?: LegendPosition;
  items: LegendItem[];
}

export interface LegendItemConfiguration {
  label: string;
  colour: string;
  labelColour?: LegendLabelColour;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
  inlinePosition?: LegendInlinePosition;
  inlinePositionOffset?: number;
  sequentialCategoryColours?: boolean;
}

export interface LegendItem extends LegendItemConfiguration {
  dataSet: DataSet;
}

export type LegendInlinePosition = 'above' | 'below' | 'right';

export type LegendLabelColour = 'black' | 'inherit';
