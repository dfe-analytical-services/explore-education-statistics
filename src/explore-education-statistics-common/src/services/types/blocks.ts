import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  AxisConfiguration,
  AxisType,
} from '@common/modules/charts/types/chart';
import {
  DataSetConfiguration,
  DataSetConfigurationOptions,
  DeprecatedDataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { Dictionary, OmitStrict } from '@common/types';

/**
 * We want to phase this out in favour of
 * defining data set configuration alongside
 * the data set itself.
 *
 * The current data structure is unwieldy
 * as we have to unnecessarily stitch labels
 * and their associated data set back together.
 *
 * @deprecated
 */
export type DeprecatedChartLabels = Dictionary<DeprecatedDataSetConfiguration>;

interface ChartAxisConfiguration
  extends OmitStrict<AxisConfiguration, 'dataSets'> {
  dataSets: (OmitStrict<DataSetConfiguration, 'config'> & {
    config?: DataSetConfigurationOptions;
  })[];
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
  labels?: DeprecatedChartLabels;
  axes: ChartAxesConfiguration;
};

export interface Table {
  indicators: string[];
  tableHeaders: UnmappedTableHeadersConfig;
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
