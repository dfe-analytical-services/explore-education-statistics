import {
  ContentBlock,
  DataBlock,
  EmbedBlock,
} from '@common/services/types/blocks';
import { FileInfo } from '@common/services/types/file';
import { ReleaseType } from '@common/services/types/releaseType';
import {
  MethodologySummary,
  ExternalMethodology,
} from '@common/services/types/methodology';
import { PaginatedList } from '@common/services/types/pagination';
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
  contact: Contact;
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
  supersededById?: string;
  isSuperseded?: boolean;
  supersededBy?: PublicationSupersededBy;
}

export interface PublicationSummary {
  id: string;
  slug: string;
  title: string;
}

export interface PublicationListSummary {
  id: string;
  published: Date;
  rank: number;
  slug: string;
  summary?: string;
  theme: string;
  title: string;
  type: ReleaseType;
}

export interface Contact {
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

export interface KeyStatisticBase {
  type: KeyStatisticType;
  id: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  order: number;
  created: string;
  updated?: string;
}

export const KeyStatisticTypes = {
  DataBlock: 'KeyStatisticDataBlock',
  Text: 'KeyStatisticText',
} as const;

export type KeyStatisticType = typeof KeyStatisticTypes[keyof typeof KeyStatisticTypes];

export interface KeyStatisticDataBlock extends KeyStatisticBase {
  type: 'KeyStatisticDataBlock';
  dataBlockId: string;
}

export interface KeyStatisticText extends KeyStatisticBase {
  type: 'KeyStatisticText';
  title: string;
  statistic: string;
}

export type KeyStatistic = KeyStatisticDataBlock | KeyStatisticText;

export interface ContentSection<BlockType> {
  id: string;
  order: number;
  heading: string;
  caption?: string;
  content: BlockType[];
}

export const publicationSortOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type PublicationSortOption = typeof publicationSortOptions[number];

export type PublicationSortParam = 'published' | 'title' | 'relevance';

export type PublicationOrderParam = 'asc' | 'desc';

export const publicationFilters = ['releaseType', 'search', 'themeId'] as const;

export type PublicationFilter = typeof publicationFilters[number];

export interface PublicationListRequest {
  order?: PublicationOrderParam;
  page?: number;
  pageSize?: number;
  releaseType?: ReleaseType;
  search?: string;
  sort?: PublicationSortParam;
  themeId?: string;
}

export interface Release<
  ContentBlockType extends ContentBlock = ContentBlock,
  DataBlockType extends DataBlock = DataBlock,
  EmbedBlockType extends EmbedBlock = EmbedBlock
> {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  published: string;
  slug: string;
  summarySection: ContentSection<ContentBlockType>;
  keyStatistics: KeyStatistic[];
  keyStatisticsSecondarySection: ContentSection<DataBlockType>;
  headlinesSection: ContentSection<ContentBlockType>;
  relatedDashboardsSection?: ContentSection<ContentBlockType>; // optional because older releases may not have this section
  publication: Publication;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  relatedInformation: BasicLink[];
  type: ReleaseType;
  updates: ReleaseNote[];
  content: ContentSection<ContentBlockType | DataBlockType | EmbedBlockType>[];
  downloadFiles: FileInfo[];
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
}

export interface PublicationReleaseSummary extends ReleaseSummary {
  publication: PublicationSummary;
}

export interface PreReleaseAccessListSummary extends ReleaseSummary {
  publication: PublicationSummary;
  preReleaseAccessList: string;
}

export interface PublicationTreeSummary {
  id: string;
  title: string;
  slug: string;
  legacyPublicationUrl?: string;
  isSuperseded: boolean;
  supersededBy?: PublicationSupersededBy;
}

export interface PublicationSupersededBy {
  id: string;
  title: string;
  slug: string;
}
export interface Topic {
  id: string;
  title: string;
  summary: string;
  publications: PublicationTreeSummary[];
}

export interface Theme {
  id: string;
  title: string;
  summary: string;
  topics: Topic[];
}

interface PublicationTreeOptions {
  publicationFilter?: 'DataTables' | 'DataCatalogue' | 'FastTrack';
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
  getPublicationTree({
    publicationFilter,
  }: PublicationTreeOptions = {}): Promise<Theme[]> {
    return contentApi.get('/publication-tree', {
      params: { publicationFilter },
    });
  },
  listPublications(
    params: PublicationListRequest,
  ): Promise<PaginatedList<PublicationListSummary>> {
    return contentApi.get(`/publications`, {
      params,
    });
  },
};
