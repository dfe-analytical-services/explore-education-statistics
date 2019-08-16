import {
  DataBlockLocation,
  DataBlockRequest,
} from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import { PositionType } from 'recharts';
import { AxesConfiguration } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { contentApi } from './api';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
  nextUpdate: string;
  releases: {
    id: string;
    releaseName: string;
    slug: string;
  }[];
  legacyReleases: {
    id: string;
    description: string;
    url: string;
  }[];
  topic: {
    theme: {
      title: string;
    };
  };
  contact: {
    teamName: string;
    teamEmail: string;
    contactName: string;
    contactTelNo: string;
  };
}

export interface DataQuery {
  method: string;
  path: string;
  body: string;
}

export interface ReferenceLine {
  label: string;
  position: number | string;
}

export interface Axis {
  title: string;
  key?: string[];
  min?: number;
  max?: number;
  size?: number;
}

export type ChartType = 'line' | 'verticalbar' | 'horizontalbar' | 'map' | "infographic";

export interface ChartDataSet {
  indicator: string;
  filters: string[];
  location?: DataBlockLocation;
  timePeriod?: string;
}

export interface OptionalChartDataSet {
  indicator?: string;
  filters?: string[];
  location?: string[];
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

export interface DataSetConfiguration {
  label: string;
  value: string;
  name?: string;
  unit?: string;
  colour?: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

export type AxisGroupBy =
  | 'timePeriods'
  | 'locations'
  | 'filters'
  | 'indicators';

export type AxisType = 'major' | 'minor';

export type LabelPosition = 'axis' | 'graph' | PositionType;

export interface AxisConfiguration {
  name: string;
  type: AxisType;
  groupBy?: AxisGroupBy;
  sortBy?: string;
  sortAsc?: boolean;
  dataSets: ChartDataSet[];
  dataRange?: [number | undefined, number | undefined];

  referenceLines?: ReferenceLine[];

  visible?: boolean;
  title?: string;
  unit?: string;
  showGrid?: boolean;
  labelPosition?: LabelPosition;
  size?: string;

  min?: string;
  max?: string;

  tickConfig?: 'default' | 'startEnd' | 'custom';
  tickSpacing?: string;
}

export interface Chart {
  type: ChartType;
  labels: Dictionary<DataSetConfiguration>;
  axes: AxesConfiguration;

  stacked?: boolean;
  width?: number;
  height?: number;
  showLegend?: boolean;
}

export interface Table {
  indicators: string[];
}

export interface Summary {
  dataKeys: string[];
  dataSummary: string[];
  description: { type: string; body: string };
}

export type ContentBlockType =
  | 'MarkDownBlock'
  | 'InsetTextBlock'
  | 'DataBlock'
  | 'HtmlBlock';

export interface ContentBlock {
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}

export interface AbstractRelease<ContentBlockType> {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  published: string;
  slug: string;
  summary: string;
  publicationId: string;
  publication: Publication;
  updates: {
    id: string;
    releaseId: string;
    on: string;
    reason: string;
  }[];
  content: {
    order: number;
    heading: string;
    caption: string;
    content: ContentBlockType[];
  }[];
  keyStatistics: ContentBlockType;
  dataFiles: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
  downloadFiles: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
}

export type Release = AbstractRelease<ContentBlock>;

export default {
  getPublication(publicationSlug: string): Promise<Release> {
    return contentApi.get(`content/publication/${publicationSlug}`);
  },
  getLatestPublicationRelease(publicationSlug: string): Promise<Release> {
    return contentApi.get(`content/publication/${publicationSlug}/latest`);
  },
  getPublicationRelease(releaseId: string): Promise<Release> {
    return contentApi.get(`content/release/${releaseId}`);
  },
};
