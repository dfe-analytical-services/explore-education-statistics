import { Chart } from '@common/modules/charts/types/chart';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataQuery } from '@common/services/tableBuilderService';

export interface Table {
  tableHeaders: UnmappedTableHeadersConfig;
}

export type BlockType = 'HtmlBlock' | 'DataBlockVersionLink' | 'EmbedBlockLink';

export interface BaseBlock {
  id: string;
  order?: number;
  type: BlockType;
}

export interface HtmlBlock extends BaseBlock {
  type: 'HtmlBlock';
  body?: string;
}

export type ContentBlock = HtmlBlock;

export interface DataBlock extends BaseBlock {
  type: 'DataBlockVersionLink';
  name: string;
  dataSetName?: string;
  dataSetId?: string;
  highlightName?: string;
  highlightDescription?: string;
  heading: string;
  source?: string;
  query: TableDataQuery;
  charts: Chart[];
  table: Table;
  dataBlockId: string;
}

export interface EmbedBlock extends BaseBlock {
  title: string;
  type: 'EmbedBlockLink';
  url: string;
}

export type Block = ContentBlock | DataBlock | EmbedBlock;
