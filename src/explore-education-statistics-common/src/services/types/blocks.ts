import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import { TableDataQuery } from '@common/modules/table-tool/services/tableBuilderService';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';

export type Chart = ChartRendererProps;

export interface Table {
  indicators: string[];
  tableHeaders: TableHeadersConfig;
}

export interface Summary {
  dataKeys: string[];
  dataSummary: string[];
  dataDefinitionTitle: string[];
  dataDefinition: string[];
}

export type BlockType = 'MarkDownBlock' | 'HtmlBlock' | 'DataBlock';

export interface BaseBlock {
  id: string;
  order: number;
  type: BlockType;
}

export interface MarkdownBlock extends BaseBlock {
  type: 'MarkDownBlock';
  body: string;
}

/**
 * Some seeded data may come in the form of HTML blocks,
 * so we need to potentially read and render these blocks.
 * However, we should NOT be using these in a writeable way.
 * @deprecated
 */
export interface HtmlBlock extends BaseBlock {
  type: 'HtmlBlock';
  body: string;
}

export type DataBlockRequest = TableDataQuery;

export type ContentBlock = MarkdownBlock | HtmlBlock;

export interface DataBlock extends BaseBlock {
  type: 'DataBlock';
  name: string;
  heading: string;
  source: string;
  dataBlockRequest: DataBlockRequest;
  charts: Chart[];
  tables: Table[];
  summary?: Summary;
}

export type Block = ContentBlock | DataBlock;
