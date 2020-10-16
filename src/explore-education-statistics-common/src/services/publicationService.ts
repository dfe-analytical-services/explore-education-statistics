import { ContentBlock, DataBlock } from '@common/services/types/blocks';
import { FileInfo } from '@common/services/types/file';
import { PartialDate } from '@common/utils/date/partialDate';
import { contentApi } from './api';

export type ReleaseApprovalStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

export interface Methodology {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

export interface ExternalMethodology {
  title: string;
  url: string;
}

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
  methodology?: Methodology;
  externalMethodology?: ExternalMethodology;
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

export interface PublicationMethodology {
  methodology?: Methodology;
  externalMethodology?: ExternalMethodology;
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

// eslint-disable-next-line no-shadow
export enum ReleaseType {
  AdHoc = 'Ad Hoc',
  NationalStatistics = 'National Statistics',
  OfficialStatistics = 'Official Statistics',
}

export interface ContentSection<BlockType> {
  id: string;
  order: number;
  heading: string;
  caption?: string;
  content: BlockType[];
}

export interface Release<
  ContentBlockType extends ContentBlock = ContentBlock,
  DataBlockType extends DataBlock = DataBlock,
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
  keyStatisticsSection: ContentSection<DataBlockType>;
  keyStatisticsSecondarySection: ContentSection<DataBlockType>;
  headlinesSection: ContentSection<ContentBlockType>;
  publication: PublicationType;
  publicationId: string;
  latestRelease: boolean;
  publishScheduled?: string;
  nextReleaseDate: PartialDate;
  status: ReleaseApprovalStatus;
  relatedInformation: BasicLink[];
  type: {
    id: string;
    title: ReleaseType;
  };
  updates: ReleaseNote[];
  content: ContentSection<ContentBlockType | DataBlockType>[];
  downloadFiles: FileInfo[];
  preReleaseAccessList?: string;
  dataLastPublished: string;
}

export default {
  getPublicationTitle(publicationSlug: string): Promise<PublicationTitle> {
    return contentApi.get(`content/publication/${publicationSlug}/title`);
  },
  getPublicationMethodology(
    publicationSlug: string,
  ): Promise<PublicationMethodology> {
    return contentApi.get(`content/publication/${publicationSlug}/methodology`);
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
