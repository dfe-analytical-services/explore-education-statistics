import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import { DataBlockRequest } from '@common/services/dataBlockService';
import { DayMonthYearValues } from '@common/utils/date/dayMonthYear';
import { contentApi } from './api';

export type ReleaseStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
  nextUpdate: string;
  otherReleases: {
    id: string;
    slug: string;
    title: string;
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
  contact: PublicationContact;
  methodology?: {
    id: string;
    slug: string;
    summary: string;
    title: string;
  };
  externalMethodology?: {
    title: string;
    url: string;
  };
}

export interface PublicationContact {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}

export interface PublicationTitle {
  id: string;
  title: string;
}

export interface DataQuery {
  method: string;
  path: string;
  body: string;
}

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

export type ContentBlockType =
  | 'MarkDownBlock'
  | 'InsetTextBlock'
  | 'DataBlock'
  | 'HtmlBlock';

export interface ContentBlock {
  id: string;
  order?: number;
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}

export interface BasicLink {
  id: string;
  description: string;
  url: string;
}

export interface ReleaseNote {
  id: string;
  releaseId: string;
  on: Date;
  reason: string;
}

export enum ReleaseType {
  AdHoc = 'Ad Hoc',
  NationalStatistics = 'National Statistics',
  OfficialStatistics = 'Official Statistics',
}

export interface ContentSection<ContentBlockType> {
  id: string;
  order: number;
  heading: string;
  caption: string;
  content?: ContentBlockType[];
}

export interface AbstractRelease<
  ContentBlockType,
  PublicationType = Publication
> {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  published: string;
  slug: string;
  summarySection: ContentSection<ContentBlockType>;
  keyStatisticsSection: ContentSection<ContentBlockType>;
  keyStatisticsSecondarySection?: ContentSection<ContentBlockType>;
  headlinesSection: ContentSection<ContentBlockType>;
  publication: PublicationType;
  latestRelease: boolean;
  publishScheduled?: string;
  nextReleaseDate: DayMonthYearValues;
  status: ReleaseStatus;
  relatedInformation: BasicLink[];
  type: {
    id: string;
    title: ReleaseType;
  };
  updates: ReleaseNote[];
  content: ContentSection<ContentBlockType>[];
  dataFiles?: {
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
  prerelease?: boolean;
}

export type Release = AbstractRelease<ContentBlock>;

export default {
  getPublicationTitle(publicationSlug: string): Promise<PublicationTitle> {
    return contentApi.get(`content/publication/${publicationSlug}/title`);
  },
  getLatestPublicationRelease(publicationSlug: string): Promise<Release> {
    return contentApi.get(`content/publication/${publicationSlug}/latest`);
  },
  getPublicationRelease(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<Release> {
    return contentApi.get(
      `content/publication/${publicationSlug}/${releaseSlug}`,
    );
  },
};
