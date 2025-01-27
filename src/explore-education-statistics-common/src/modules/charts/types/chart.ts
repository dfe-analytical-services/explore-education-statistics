import {
  horizontalBarBlockDefinition,
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import {
  infographicBlockDefinition,
  InfographicChartProps,
} from '@common/modules/charts/components/InfographicBlock';
import {
  lineChartBlockDefinition,
  LineChartProps,
} from '@common/modules/charts/components/LineChartBlock';
import {
  mapBlockDefinition,
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import {
  verticalBarBlockDefinition,
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { NestedPartial } from '@common/types';

export type TitleType = 'default' | 'alternative';

export type ChartType =
  | 'line'
  | 'verticalbar'
  | 'horizontalbar'
  | 'map'
  | 'infographic';

export type ChartSymbol =
  | 'circle'
  | 'cross'
  | 'diamond'
  | 'square'
  | 'star'
  | 'triangle'
  | 'wye'
  | 'none';

export type LineStyle = 'solid' | 'dashed' | 'dotted';

export type Axis = 'x' | 'y';
export type AxisGroupBy = 'timePeriod' | 'locations' | 'filters' | 'indicators';
export type AxisType = 'major' | 'minor';
export type TickConfig = 'default' | 'startEnd' | 'custom' | 'showAll';
export type ReferenceLineStyle = 'dashed' | 'solid' | 'none';
export type LineChartDataLabelPosition = 'above' | 'below';
export type BarChartDataLabelPosition = 'inside' | 'outside';

export interface ReferenceLine {
  endPosition?: string;
  label: string;
  labelWidth?: number;
  otherAxisEnd?: string;
  otherAxisPosition?: number;
  otherAxisStart?: string;
  position: number | string;
  perpendicularLine?: boolean;
  style?: ReferenceLineStyle;
}

export interface Label {
  text: string;
  rotated?: boolean;
  width?: number;
}

export interface AxisConfiguration {
  type: AxisType;
  groupBy?: AxisGroupBy;
  groupByFilter?: string;
  groupByFilterGroups?: boolean;
  sortBy?: string;
  sortAsc?: boolean;
  dataSets: DataSet[];
  referenceLines: ReferenceLine[];
  visible?: boolean;
  unit?: string;
  decimalPlaces?: number;
  showGrid?: boolean;
  label?: Label;
  size?: number;
  min?: number;
  max?: number;
  tickConfig?: TickConfig;
  tickSpacing?: number;
}

export type AxesConfiguration = {
  [key in AxisType]?: AxisConfiguration;
};

export const dataGroupingTypes = {
  EqualIntervals: 'Equal intervals',
  Quantiles: 'Quantiles',
  Custom: 'Custom',
} as const;

export type DataGroupingType = keyof typeof dataGroupingTypes;

export interface CustomDataGroup {
  max: number;
  min: number;
}

export interface DataGroupingConfig {
  type: DataGroupingType;
  numberOfGroups?: number;
  customGroups: CustomDataGroup[];
}

export interface MapDataSetConfig {
  dataSet: DataSet;
  dataGrouping: DataGroupingConfig;
  boundaryLevel?: number;
}

export interface MapConfig {
  dataSetConfigs: MapDataSetConfig[];
}

export interface ChartCapabilities {
  canIncludeNonNumericData: boolean;
  canPositionLegendInline: boolean;
  canSetBarThickness: boolean;
  canSetDataLabelPosition: boolean;
  canShowDataLabels: boolean;
  canShowAllMajorAxisTicks: boolean;
  canSize: boolean;
  canSort: boolean;
  hasGridLines: boolean;
  hasLegend: boolean;
  hasLegendPosition: boolean;
  hasLineStyle: boolean;
  hasReferenceLines: boolean;
  hasSymbols: boolean;
  requiresGeoJson: boolean;
  stackable: boolean;
}

export interface ChartDefinitionOptions {
  stacked?: boolean;
  height: number;
  width?: number;
  barThickness?: number;
  subtitle?: string;
  title?: string;
  titleType: TitleType;
  alt: string;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  dataLabelPosition?: BarChartDataLabelPosition | LineChartDataLabelPosition;
  // Map options
  boundaryLevel?: number;
  dataClassification?: DataGroupingType;
  dataGroups?: number;
}

export interface ChartDefinition {
  type: ChartType;
  name: string;
  capabilities: ChartCapabilities;
  options: {
    defaults?: Partial<ChartDefinitionOptions>;
  };
  legend: {
    defaults?: Partial<LegendConfiguration>;
  };
  axes: {
    [key in AxisType]?: ChartDefinitionAxis;
  };
}

export interface ChartDefinitionAxisCapabilities {
  canRotateLabel: boolean;
}

export interface ChartDefinitionAxis {
  axis?: Axis;
  id: string;
  title: string;
  type: AxisType;
  hide?: boolean;
  capabilities: ChartDefinitionAxisCapabilities;
  defaults?: NestedPartial<AxisConfiguration>;
  constants?: {
    groupBy?: AxisGroupBy;
  };
  referenceLineDefaults?: Partial<ReferenceLine>;
}

export const chartDefinitions: ChartDefinition[] = [
  lineChartBlockDefinition,
  verticalBarBlockDefinition,
  horizontalBarBlockDefinition,
  mapBlockDefinition,
  infographicBlockDefinition,
];

interface BaseChartConfig {
  type: 'horizontalbar' | 'infographic' | 'line' | 'map' | 'verticalbar';
  title?: string;
  subtitle?: string;
  alt: string;
  height: number;
  width?: number;
  includeNonNumericData: boolean;
  titleType?: TitleType;
}

export interface HorizontalBarChartConfig extends BaseChartConfig {
  type: 'horizontalbar';
  barThickness?: number;
  stacked: boolean;
  showDataLabels: boolean;
  dataLabelPosition?: BarChartDataLabelPosition;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  map?: undefined;
}

export interface InfographicConfig extends BaseChartConfig {
  type: 'infographic';
  fileId: string;
  axes?: undefined;
  legend?: undefined;
  map?: undefined;
}

export interface LineChartConfig extends BaseChartConfig {
  type: 'line';
  showDataLabels: boolean;
  dataLabelPosition?: LineChartDataLabelPosition;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  map?: undefined;
}

export interface MapChartConfig extends BaseChartConfig {
  type: 'map';
  boundaryLevel: number;
  dataGroups?: number;
  dataClassification?: DataGroupingType;
  map: MapConfig;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
  };
}

export interface VerticalBarChartConfig extends BaseChartConfig {
  type: 'verticalbar';
  barThickness?: number;
  stacked: boolean;
  showDataLabels: boolean;
  dataLabelPosition?: BarChartDataLabelPosition;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  map?: undefined;
}

/**
 * This is the chart type that will be returned by the API.
 * It differs from {@see RenderableChart} as we need to
 * progressively migrate parts of its old data structure,
 * to our newer one that is being used by {@see ChartRendererProps}.
 */
export type ChartConfig =
  | InfographicConfig
  | LineChartConfig
  | MapChartConfig
  | HorizontalBarChartConfig
  | VerticalBarChartConfig;

export type DraftChartConfig = Partial<ChartConfig>;

/**
 * FullChart, a {@see ChartConfig} alongside related all data and meta.
 */
export interface FullChart {
  data: TableDataResult[];
  meta: FullTableMeta;
  chartConfig: ChartConfig;
}

export type DraftFullChart<
  T extends { chartConfig: { type: string } } = FullChart,
> = Omit<T, 'chartConfig'> & {
  chartConfig: DraftChartConfig;
};

export interface StackedBarProps extends FullChart {
  chartConfig: VerticalBarChartConfig | HorizontalBarChartConfig;
}

/**
 * RenderableChart,
 * All appropriate props bundled together to fit any chart renderer component.
 * a {@see FullChart} & some additional helpers and {@see WithChartConfigType} hoisted(copied)
 */
export type RenderableChart =
  | HorizontalBarProps
  | LineChartProps
  | VerticalBarProps
  | InfographicChartProps
  | Omit<MapBlockProps, 'id'>;
