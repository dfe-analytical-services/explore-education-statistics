import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import {
  AxesConfiguration,
  AxisConfiguration,
  BarChartDataLabelPosition,
  DataGroupingType,
  LineChartDataLabelPosition,
  MapConfig,
  TitleType,
} from '@common/modules/charts/types/chart';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataQuery } from '@common/services/tableBuilderService';

export interface HorizontalBarChart {
  alt: string;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  barThickness?: number;
  dataLabelPosition?: BarChartDataLabelPosition;
  height: number;
  includeNonNumericData?: boolean;
  legend: LegendConfiguration;
  showDataLabels?: boolean;
  stacked?: boolean;
  subtitle?: string;
  title?: string;
  titleType?: TitleType;
  type: 'horizontalbar';
  width?: number;
}

export interface Infographic {
  alt: string;
  axes: AxesConfiguration;
  fileId: string;
  height: number;
  legend: LegendConfiguration;
  subtitle?: string;
  title?: string;
  titleType?: TitleType;
  type: 'infographic';
  width?: number;
}

export interface LineChart {
  alt: string;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  dataLabelPosition?: LineChartDataLabelPosition;
  height: number;
  includeNonNumericData?: boolean;
  legend: LegendConfiguration;
  showDataLabels?: boolean;
  subtitle?: string;
  title?: string;
  titleType?: TitleType;
  type: 'line';
  width?: number;
}

export type MapChart = {
  alt: string;
  axes: {
    major: AxisConfiguration;
  };
  boundaryLevel: number;
  dataClassification?: DataGroupingType;
  dataGroups?: number;
  height: number;
  includeNonNumericData?: boolean;
  legend: LegendConfiguration;
  map: MapConfig;
  position?: { lat: number; lng: number };
  showDataLabels?: boolean;
  subtitle?: string;
  title?: string;
  titleType?: TitleType;
  type: 'map';
  width?: number;
};

export type VerticalBarChart = {
  alt: string;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
  barThickness?: number;
  dataLabelPosition?: BarChartDataLabelPosition;
  height: number;
  includeNonNumericData?: boolean;
  legend: LegendConfiguration;
  showDataLabels?: boolean;
  stacked?: boolean;
  subtitle?: string;
  title?: string;
  titleType?: TitleType;
  type: 'verticalbar';
  width?: number;
};

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

export interface Table {
  indicators: string[];
  tableHeaders: UnmappedTableHeadersConfig;
}

export type BlockType =
  | 'MarkDownBlock'
  | 'HtmlBlock'
  | 'DataBlock'
  | 'EmbedBlockLink';

export interface BaseBlock {
  id: string;
  order: number;
  type: BlockType;
}

/**
 * Some seeded data may come in the form of markdown blocks,
 * so we need to potentially read and render these blocks.
 * However, we should NOT be using these in a writeable way.
 * @deprecated
 */
export interface MarkdownBlock extends BaseBlock {
  type: 'MarkDownBlock';
  body: string;
}

export interface HtmlBlock extends BaseBlock {
  type: 'HtmlBlock';
  body: string;
}

export type ContentBlock = MarkdownBlock | HtmlBlock;

export interface DataBlock extends BaseBlock {
  type: 'DataBlock';
  name: string;
  dataSetName?: string;
  dataSetId: string;
  highlightName?: string;
  highlightDescription?: string;
  heading: string;
  source?: string;
  query: TableDataQuery;
  charts: Chart[];
  table: Table;
  dataBlockParentId: string;
}

export interface EmbedBlock extends BaseBlock {
  title: string;
  type: 'EmbedBlockLink';
  url: string;
}

export type Block = ContentBlock | DataBlock | EmbedBlock;
