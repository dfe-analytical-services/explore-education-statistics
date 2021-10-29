import { ContentBlock, DataBlock } from '@common/services/types/blocks';
import { FileInfo } from '@common/services/types/file';
import {
  MethodologySummary,
  ExternalMethodology,
} from '@common/services/types/methodology';
import { PartialDate } from '@common/utils/date/partialDate';
import { contentApi } from './api';

export type ReleaseApprovalStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
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
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
}

export interface PublicationSummary {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
}

export interface BasicPublicationContact {
  id?: string;
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName?: string;
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

export interface BasicLink {
  id: string;
  description: string;
  url: string;
}

export interface ReleaseNote {
  id: string;
  on: Date;
  reason: string;
}

// eslint-disable-next-line no-shadow
export enum ReleaseType {
  AdHoc = 'Ad Hoc',
  Experimental = 'Experimental',
  NationalStatistics = 'National Statistics',
  OfficialStatistics = 'Official Statistics',
  TransparencyData = 'Transparency Data',
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
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  relatedInformation: BasicLink[];
  type: {
    id: string;
    title: ReleaseType;
  };
  updates: ReleaseNote[];
  content: ContentSection<ContentBlockType | DataBlockType>[];
  downloadFiles: FileInfo[];
  dataLastPublished: string;
  hasPreReleaseAccessList: boolean;
  hasDataGuidance: boolean;
}

export interface ReleaseSummary {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  published?: string;
  slug: string;
  nextReleaseDate: PartialDate;
  type: {
    id: string;
    title: ReleaseType;
  };
  latestRelease: boolean;
  dataLastPublished: string;
}

export interface PublicationReleaseSummary extends ReleaseSummary {
  publication: PublicationSummary;
}

export interface PreReleaseAccessListSummary extends ReleaseSummary {
  publication: PublicationSummary;
  preReleaseAccessList: string;
}

export default {
  getPublicationTitle(publicationSlug: string): Promise<PublicationTitle> {
    return contentApi.get(`/publications/${publicationSlug}/title`);
  },
  listReleases(publicationSlug: string): Promise<ReleaseSummary[]> {
    return contentApi.get(`/publications/${publicationSlug}/releases`);
  },
  getLatestPublicationRelease(publicationSlug: string): Promise<Release> {
    return contentApi.get(`/publications/${publicationSlug}/releases/latest`);
  },
  getLatestPublicationReleaseSummary(
    publicationSlug: string,
  ): Promise<PublicationReleaseSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/latest/summary`,
    );
  },
  getPublicationRelease(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<Release> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}`,
    );
  },
  getPublicationReleaseSummary(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<PublicationReleaseSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/summary`,
    );
  },
  getLatestPreReleaseAccessList(
    publicationSlug: string,
  ): Promise<PreReleaseAccessListSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/latest/prerelease-access-list`,
    );
  },
  getPreReleaseAccessList(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<PreReleaseAccessListSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/prerelease-access-list`,
    );
  },
};
