import { horizontalBarBlockDefinition } from '@common/modules/charts/components/HorizontalBarBlock';
import { infographicBlockDefinition } from '@common/modules/charts/components/InfographicBlock';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import { mapBlockDefinition } from '@common/modules/charts/components/MapBlock';
import { verticalBarBlockDefinition } from '@common/modules/charts/components/VerticalBarBlock';
import { DataSet } from '@common/modules/charts/types/dataSet';
import {
  LegendConfiguration,
  LegendLabelColour,
} from '@common/modules/charts/types/legend';
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
  // This property can't be set on new charts since EES-6134.
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

export interface MapCategoricalData {
  colour: string;
  value: string;
}

export interface MapDataSetConfig {
  boundaryLevel?: number;
  categoricalDataConfig?: MapCategoricalData[];
  dataSet: DataSet;
  dataSetKey: string;
  dataGrouping: DataGroupingConfig;
}

export interface MapLegendItem {
  value: string;
  colour: string;
}

export interface MapConfig {
  /**
   * Deprecated as this information is now on the data set config.
   * This is kept as a fallback for pre-existing maps.
   * @deprecated
   */
  categoricalDataConfig?: MapCategoricalData[];
  dataSetConfigs: MapDataSetConfig[];
}

export interface ChartProps {
  data: TableDataResult[];
  meta: FullTableMeta;
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  axes: AxesConfiguration;
  legend?: LegendConfiguration;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
}

export interface StackedBarProps extends ChartProps {
  barThickness?: number;
  dataLabelPosition?: BarChartDataLabelPosition;
  stacked?: boolean;
}

export interface ChartCapabilities {
  canIncludeNonNumericData: boolean;
  canPositionLegendInline: boolean;
  canSetBarThickness: boolean;
  canSetDataLabelColour: boolean;
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
  barThickness?: number;
  subtitle?: string;
  title?: string;
  titleType: TitleType;
  alt: string;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  dataLabelPosition?: BarChartDataLabelPosition | LineChartDataLabelPosition;
  dataLabelColour?: LegendLabelColour;
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

export interface ChartDefinitionAxis {
  axis?: Axis;
  id: string;
  title: string;
  type: AxisType;
  hide?: boolean;
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

interface HorizontalBarChart {
  type: 'horizontalbar';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  barThickness?: number;
  dataLabelPosition?: BarChartDataLabelPosition;
  stacked?: boolean;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

interface Infographic {
  type: 'infographic';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  axes: AxesConfiguration;
  legend?: LegendConfiguration;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  fileId: string;
}

export interface LineChart {
  type: 'line';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  dataLabelPosition?: LineChartDataLabelPosition;
  dataLabelColour?: LegendLabelColour;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

export interface MapChart {
  type: 'map';
  axes: {
    major: AxisConfiguration;
  };
  dataGroups?: number;
  dataClassification?: DataGroupingType;
  legend: LegendConfiguration;
  map?: MapConfig;
  position?: { lat: number; lng: number };
  boundaryLevel: number;
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  subtitle?: string;
}

interface VerticalBarChart {
  type: 'verticalbar';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  barThickness?: number;
  dataLabelPosition?: BarChartDataLabelPosition;
  stacked?: boolean;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

/**
 * This is the chart type that will be returned by the API.
 * It differs from {@see ChartRendererProps} as we need to
 * progressively migrate parts of its old data structure,
 * to our newer one that is being used by {@see ChartRendererProps}.
 */
export type Chart =
  | Infographic
  | LineChart
  | MapChart
  | HorizontalBarChart
  | VerticalBarChart;
