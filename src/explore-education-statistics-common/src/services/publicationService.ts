import { MapFeature } from '@common/modules/find-statistics/components/charts/MapBlock';
import { DataBlockRequest } from '@common/services/dataBlockService';
import { FeatureCollection } from 'geojson';
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
  theme: string;
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
  x?: number | string;
  y?: number | string;
}

export interface Axis {
  title: string;
  key?: string;
  min?: number;
  max?: number;
  size?: number;
}

export type ChartType = 'line' | 'verticalbar' | 'horizontalbar' | 'map';

export interface Chart {
  type: ChartType;
  indicators: string[];
  xAxis?: Axis;
  yAxis?: Axis;
  stacked?: boolean;
  geometry?: MapFeature | FeatureCollection;
  referenceLines?: ReferenceLine[];
  width?: number;
  height?: number;
}

export interface Table {
  indicators: string[];
}

export interface Summary {
  dataKeys: string[];
  description: { type: string; body: string };
}

export type ContentBlockType = 'MarkDownBlock' | 'InsetTextBlock' | 'DataBlock';

export interface ContentBlock {
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}

export interface Release {
  id: string;
  title: string;
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
    content: ContentBlock[];
  }[];
  keyStatistics: ContentBlock;
}

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
