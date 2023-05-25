import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  AxisConfiguration,
  AxisType,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { OmitStrict } from '@common/types';

interface ChartAxisConfiguration
  extends OmitStrict<AxisConfiguration, 'dataSets'> {
  dataSets: DataSet[];
}

type ChartAxesConfiguration = {
  [key in AxisType]?: ChartAxisConfiguration;
};

/**
 * This is the chart type that will be returned by the API.
 * It differs from {@see ChartRendererProps} as we need to
 * progressively migrate parts of its old data structure,
 * to our newer one that is being used by {@see ChartRendererProps}.
 */
export type Chart = OmitStrict<ChartRendererProps, 'data' | 'meta' | 'axes'> & {
  axes: ChartAxesConfiguration;
};

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
  highlightName?: string;
  highlightDescription?: string;
  heading: string;
  source: string;
  query: TableDataQuery;
  charts: Chart[];
  table: Table;
}

export interface EmbedBlock extends BaseBlock {
  title: string;
  type: 'EmbedBlockLink';
  url: string;
}

export type Block = ContentBlock | DataBlock | EmbedBlock;
