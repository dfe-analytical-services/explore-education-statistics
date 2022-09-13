import { ContentBlock, DataBlock } from '@common/services/types/blocks';
import { FileInfo } from '@common/services/types/file';
import { ReleaseType } from '@common/services/types/releaseType';
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
  releases: {
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
  supersededById?: string;
  isSuperseded?: boolean;
}

export interface PublicationSummary {
  id: string;
  slug: string;
  title: string;
}

// TODO EES-3517 a guess at the new type for the find stats page.
export interface PublicationSummaryWithRelease {
  id: string;
  legacyPublicationUrl?: string;
  latestRelease?: {
    id: string;
    published: string;
    type: ReleaseType;
  };
  slug: string;
  summary?: string;
  theme: {
    title: string;
  };
  title: string;
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
  relatedDashboardsSection?: ContentSection<ContentBlockType>; // optional because older releases may not have this section
  publication: PublicationType;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  relatedInformation: BasicLink[];
  type: ReleaseType;
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
  type: ReleaseType;
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
