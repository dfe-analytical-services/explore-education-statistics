import { Chart } from '@common/modules/charts/types/chart';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataQuery } from '@common/services/tableBuilderService';

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
