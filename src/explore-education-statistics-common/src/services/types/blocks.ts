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

type HorizontalBarChart = {
  type: 'horizontalbar';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  width?: number;
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
};

type Infographic = {
  type: 'infographic';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  width?: number;
  axes: AxesConfiguration;
  legend?: LegendConfiguration;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  fileId: string;
  getInfographic?: GetInfographic;
};

type LineChart = {
  type: 'line';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  width?: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  map?: MapConfig;
  subtitle?: string;
  dataLabelPosition?: LineChartDataLabelPosition;
  legend: LegendConfiguration;
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
};

type MapChart = {
  type: 'map';
  axes: {
    major: AxisConfiguration;
  };
  dataGroups?: number;
  dataClassification?: DataGroupingType;
  id: string;
  legend: LegendConfiguration;
  map?: MapConfig;
  position?: { lat: number; lng: number };
  boundaryLevel: number;
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  width?: number;
  includeNonNumericData?: boolean;
  showDataLabels?: boolean;
  subtitle?: string;
};

type VerticalBarChart = {
  type: 'verticalbar';
  title?: string;
  titleType?: TitleType;
  alt: string;
  height: number;
  width?: number;
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
