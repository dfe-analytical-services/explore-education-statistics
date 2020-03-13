import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import MapBlock from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import { PublicationSubjectMeta } from '@common/modules/table-tool/services/tableBuilderService';
import {
  BoundaryLevel,
  DataBlockData,
  DataBlockLocation,
  DataBlockLocationMetadata,
  LabelValueMetadata,
  LabelValueUnitMetadata,
} from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import { ReactNode } from 'react';
import { LegendProps, PositionType } from 'recharts';

export type ChartType =
  | 'line'
  | 'verticalbar'
  | 'horizontalbar'
  | 'map'
  | 'infographic';

export interface ChartDataSet {
  indicator: string;
  filters: string[];
  location?: DataBlockLocation;
  timePeriod?: string;
}

export type ChartSymbol =
  | 'circle'
  | 'cross'
  | 'diamond'
  | 'square'
  | 'star'
  | 'triangle'
  | 'wye';

export type LineStyle = 'solid' | 'dashed' | 'dotted';

export interface LabelConfiguration {
  label: string;
}

export interface DataSetConfiguration extends LabelConfiguration {
  value: string;
  name?: string;
  unit?: string;
  colour?: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

export type AxisGroupBy = 'timePeriod' | 'locations' | 'filters' | 'indicators';
export type AxisType = 'major' | 'minor';
export type LabelPosition = 'axis' | 'graph' | PositionType;

export interface ReferenceLine {
  label: string;
  position: number | string;
}

export interface AxisConfiguration {
  name: string;
  type: AxisType;
  groupBy?: AxisGroupBy;
  sortBy?: string;
  sortAsc?: boolean;
  dataSets: ChartDataSet[];

  referenceLines?: ReferenceLine[];

  visible: boolean;
  title?: string;
  unit?: string;
  showGrid?: boolean;
  labelPosition?: LabelPosition;
  size?: number;

  min?: number;
  max?: number;

  tickConfig?: 'default' | 'startEnd' | 'custom';
  tickSpacing?: number;
}

export type AxesConfiguration = {
  [key in AxisType]?: AxisConfiguration;
};

export interface ChartMetaData {
  filters: PublicationSubjectMeta['filters'];
  indicators: Dictionary<LabelValueUnitMetadata>;
  locations: Dictionary<DataBlockLocationMetadata>;
  boundaryLevels?: BoundaryLevel[];
  timePeriod: Dictionary<LabelValueMetadata>;
}

export interface ChartProps {
  data: DataBlockData;
  meta: ChartMetaData;
  title?: string;
  height: number;
  width?: number;
  labels: Dictionary<DataSetConfiguration>;
  axes: AxesConfiguration;
  legend?: 'none' | 'top' | 'bottom';
  /**
   * Callback to enable us to render legends outside
   * of the chart container.
   * This is a bit of a hack because no such API
   * technically exists in Recharts, however, we can get
   * around this by using the Legend's `content` prop.
   */
  renderLegend?: (props: LegendProps) => ReactNode;
}

export interface StackedBarProps extends ChartProps {
  stacked?: boolean;
}

export interface ChartCapabilities {
  hasAxes: boolean;
  dataSymbols: boolean;
  stackable: boolean;
  lineStyle: boolean;
  gridLines: boolean;
  canSize: boolean;
  fixedAxisGroupBy: boolean;
  hasReferenceLines: boolean;
  hasLegend: boolean;
}

export interface ChartDefinition {
  type: ChartType;
  name: string;
  capabilities: ChartCapabilities;
  options: {
    stacked?: boolean;
    legend?: 'none' | 'top' | 'bottom';
    height: number;
    width?: number;
    title?: string;
  };
  data: {
    type: string;
    title: string;
    entryCount: number | 'multiple';
    targetAxis: string;
  }[];
  axes: Dictionary<{
    id: string;
    title: string;
    type: AxisType;
    defaults?: {
      groupBy?: AxisGroupBy;
    };
    constants?: {
      groupBy?: AxisGroupBy;
    };
  }>;

  requiresGeoJson: boolean;
}

export const chartDefinitions: ChartDefinition[] = [
  LineChartBlock.definition,
  VerticalBarBlock.definition,
  HorizontalBarBlock.definition,
  MapBlock.definition,
];
