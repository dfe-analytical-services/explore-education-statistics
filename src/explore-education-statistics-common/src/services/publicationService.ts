import { Chart } from '@common/modules/charts/types/chart';
import {
  ContentBlock,
  DataBlock,
  EmbedBlock,
  Table,
} from '@common/services/types/blocks';
import { FileInfo } from '@common/services/types/file';
import {
  ExternalMethodology,
  InternalMethodologySummary,
  MethodologySummary,
} from '@common/services/types/methodology';
import { Organisation } from '@common/services/types/organisation';
import { ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';
import { PartialDate } from '@common/utils/date/partialDate';
import { contentApi } from './api';
import { TableDataQuery } from './tableBuilderService';
import { PaginatedList, PaginationRequestParams } from './types/pagination';

export type ReleaseApprovalStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  summary?: string;
  releaseSeries: ReleaseSeriesItem[];
  theme: {
    id: string;
    title: string;
  };
  contact: Contact;
  methodologies: MethodologySummary[];
  externalMethodology?: ExternalMethodology;
  supersededById?: string;
  isSuperseded?: boolean;
  supersededBy?: PublicationSupersededBy;
}

export interface ReleaseSeriesItem {
  isLegacyLink: boolean;
  description: string;
  releaseId?: string;
  releaseSlug: string;
  legacyLinkUrl?: string;
}

export interface PublicationSummaryPreview {
  id: string;
  slug: string;
  latestReleaseSlug?: string;
  title: string;
  owner: boolean;
  contact: Contact;
}

export interface PublicationSummary {
  contact: Contact;
  id: string;
  latestRelease: {
    id: string;
    slug: string;
    title: string;
  };
  nextReleaseDate?: PartialDate;
  slug: string;
  summary: string;
  supersededByPublication?: PublicationSupersededBy;
  title: string;
  theme: {
    id: string;
    title: string;
    summary: string;
  };
}

export interface PublicationListSummary {
  id: string;
  published: Date | string;
  rank: number;
  slug: string;
  latestReleaseSlug: string;
  summary?: string;
  highlightContent?: string | null;
  highlightSummary?: string | null;
  highlightTitle?: string | null;
  theme: string;
  title: string;
  type: ReleaseType;
}

export interface Contact {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo?: string;
  id?: string;
}

export interface PublicationTitle {
  publicationId: string;
  title: string;
}

export interface BasicLink {
  id: string;
  description: string;
  url: string;
}

export interface RelatedInformationItem {
  id: string;
  title: string;
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
}

export const KeyStatisticTypes = {
  DataBlock: 'KeyStatisticDataBlock',
  Text: 'KeyStatisticText',
} as const;

export type KeyStatisticType =
  (typeof KeyStatisticTypes)[keyof typeof KeyStatisticTypes];

export interface KeyStatisticDataBlock extends KeyStatisticBase {
  type: 'KeyStatisticDataBlock';
  dataBlockParentId: string;
  dataBlockVersionId?: string;
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
  content: BlockType[];
}

export type PublicationSortParam = 'published' | 'title' | 'relevance';

export interface PublicationListRequest {
  page?: number;
  pageSize?: number;
  releaseType?: ReleaseType;
  search?: string;
  sort?: PublicationSortParam;
  sortDirection?: SortDirection;
  themeId?: string;
}

// This interface is no longer used for the FE release pages but is
// still used elsewhere, e.g. table tool, and in admin.
export interface ReleaseVersion<
  ContentBlockType extends ContentBlock = ContentBlock,
  DataBlockType extends DataBlock = DataBlock,
  EmbedBlockType extends EmbedBlock = EmbedBlock,
> {
  content: ContentSection<ContentBlockType | DataBlockType | EmbedBlockType>[];
  coverageTitle: string;
  downloadFiles: FileInfo[];
  hasDataGuidance: boolean;
  hasPreReleaseAccessList: boolean;
  headlinesSection: ContentSection<ContentBlockType>;
  id: string;
  keyStatistics: KeyStatistic[];
  keyStatisticsSecondarySection: ContentSection<DataBlockType>;
  lastUpdated?: string;
  latestRelease: boolean;
  nextReleaseDate?: PartialDate;
  publication: Publication;
  published: string;
  publishedDisplayDate?: string;
  publishingOrganisations?: Organisation[];
  relatedDashboardsSection?: ContentSection<ContentBlockType>; // optional because older releases may not have this section
  relatedInformation: BasicLink[];
  slug: string;
  summarySection: ContentSection<ContentBlockType>;
  title: string;
  type: ReleaseType;
  updates: ReleaseNote[];
  warningSection: ContentSection<ContentBlockType>;
  yearTitle: string;
}

// Used for the release pages
export interface ReleaseVersionSummary {
  coverageTitle: string;
  id: string;
  isLatestRelease?: boolean;
  label?: string;
  lastUpdated: string;
  published: string;
  publishingOrganisations?: Organisation[];
  slug: string;
  title: string;
  type: ReleaseType;
  updateCount: number;
  preReleaseAccessList: string;
  yearTitle: string;
}

export interface ReleaseVersionHomeSection<BlockType> {
  id: string;
  content: BlockType[];
}

export interface ReleaseVersionHomeContentSection<BlockType>
  extends ReleaseVersionHomeSection<BlockType> {
  heading: string;
}

export interface HtmlBlockViewModel {
  id: string;
  type: 'HtmlBlock';
  body: string;
}

export interface DataBlockViewModel {
  id: string;
  type: 'DataBlock';
  dataBlockVersion: {
    charts: Chart[];
    dataBlockVersionId: string;
    dataBlockParentId: string;
    heading: string;
    highlightDescription?: string;
    highlightName?: string;
    name: string;
    query: TableDataQuery;
    source?: string;
    table: Table;
  };
}

export interface EmbedBlockViewModel {
  id: string;
  type: 'EmbedBlock';
  embedBlock: {
    embedBlockId: string;
    title: string;
    url: string;
  };
}

export type BlockViewModel =
  | HtmlBlockViewModel
  | DataBlockViewModel
  | EmbedBlockViewModel;

export interface ReleaseVersionHomeContent<
  HtmlBlockType extends HtmlBlockViewModel = HtmlBlockViewModel,
  DataBlockType extends DataBlockViewModel = DataBlockViewModel,
  EmbedBlockType extends EmbedBlockViewModel = EmbedBlockViewModel,
> {
  content: ReleaseVersionHomeContentSection<
    HtmlBlockType | DataBlockType | EmbedBlockType
  >[];
  headlinesSection: ReleaseVersionHomeSection<HtmlBlockType>;
  keyStatistics: KeyStatistic[];
  keyStatisticsSecondarySection: ReleaseVersionHomeSection<DataBlockType>;
  releaseId: string;
  releaseVersionId: string;
  summarySection: ReleaseVersionHomeSection<HtmlBlockType>;
  warningSection: ReleaseVersionHomeSection<HtmlBlockType>;
}

export interface ReleaseVersionDataContent {
  releaseId: string;
  releaseVersionId: string;
  dataDashboards?: string;
  dataGuidance?: string;
  dataSets: DataSetItem[];
  featuredTables: FeaturedTableItem[];
  supportingFiles: SupportingFileItem[];
}

export interface DataSetItem {
  dataSetFileId: string;
  fileId: string;
  subjectId: string;
  meta: {
    filters: string[];
    geographicLevels: string[];
    indicators: string[];
    numDataFileRows: number;
    timePeriodRange: {
      start: string;
      end: string;
    };
  };
  title: string;
  summary?: string;
  isApiEnabled: boolean;
  publicApiDataSetId?: string;
}

export interface FeaturedTableItem {
  featuredTableId: string;
  dataBlockId: string;
  dataBlockParentId: string;
  title: string;
  summary: string;
}

export interface SupportingFileItem {
  fileId: string;
  extension: string;
  filename: string;
  title: string;
  summary: string;
  size: string;
}

export interface PublicationLegacyReleaseListItem {
  title: string;
  url: string;
}

export interface PublicationReleaseListItem {
  coverageTitle: string;
  isLatestRelease?: boolean;
  label?: string;
  lastUpdated: string;
  published: string;
  releaseId: string;
  slug: string;
  title: string;
  yearTitle: string;
}

export type PublicationReleaseSeriesItem =
  | PublicationLegacyReleaseListItem
  | PublicationReleaseListItem;

export interface ReleaseSummary {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  published?: string;
  slug: string;
  nextReleaseDate: PartialDate;
  type: ReleaseType;
  latestRelease: boolean;
}

export interface PublicationReleaseSummary extends ReleaseSummary {
  publication: PublicationSummaryPreview;
}

export interface PublicationTreeSummary {
  id: string;
  title: string;
  slug: string;
  legacyPublicationUrl?: string;
  isSuperseded: boolean;
  supersededBy?: PublicationSupersededBy;
}

export interface PublicationMethodologiesList {
  methodologies: InternalMethodologySummary[];
  externalMethodology?: ExternalMethodology;
}

export interface PublicationSupersededBy {
  id: string;
  title: string;
  slug: string;
}

export interface Theme {
  id: string;
  title: string;
  summary: string;
  publications: PublicationTreeSummary[];
}

export interface PublicationTreeOptions {
  filter?: 'DataTables' | 'DataCatalogue' | 'FastTrack';
}

export interface PublicationSitemapItem {
  slug: string;
  lastModified?: string;
  releases: ReleaseSitemapItem[];
}

export interface ReleaseSitemapItem {
  slug: string;
  lastModified?: string;
}

const publicationService = {
  getPublicationSummary(publicationSlug: string): Promise<PublicationSummary> {
    return contentApi.get(`/publications/${publicationSlug}`);
  },
  getPublicationTitle(publicationSlug: string): Promise<PublicationTitle> {
    return contentApi.get(`/publications/${publicationSlug}/title`);
  },
  listReleases(publicationSlug: string): Promise<ReleaseSummary[]> {
    return contentApi.get(`/publications/${publicationSlug}/releases`);
  },
  // Currently used within table tool
  getPublicationRelease(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseVersion> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}`,
    );
  },
  // Currently used within table tool
  getLatestPublicationRelease(
    publicationSlug: string,
  ): Promise<ReleaseVersion> {
    return contentApi.get(`/publications/${publicationSlug}/releases/latest`);
  },
  // Currently used within table tool
  getPublicationReleaseSummary(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<PublicationReleaseSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/summary`,
    );
  },
  // Currently used within table tool
  getLatestPublicationReleaseSummary(
    publicationSlug: string,
  ): Promise<PublicationReleaseSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/latest/summary`,
    );
  },
  getReleaseVersionSummary(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseVersionSummary> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/version-summary`,
    );
  },
  getReleaseVersionHomeContent(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseVersionHomeContent> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/content`,
    );
  },
  getReleaseVersionDataContent(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<ReleaseVersionDataContent> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/data-content`,
    );
  },
  getReleaseVersionRelatedInformation(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<RelatedInformationItem[]> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}/related-information`,
    );
  },
  getPublicationReleaseList(
    publicationSlug: string,
    params?: PaginationRequestParams,
  ): Promise<PaginatedList<PublicationReleaseSeriesItem>> {
    return contentApi.get(`/publications/${publicationSlug}/release-entries`, {
      params,
    });
  },
  getPublicationMethodologies(
    publicationSlug: string,
  ): Promise<PublicationMethodologiesList> {
    return contentApi.get(`/publications/${publicationSlug}/methodologies`);
  },
  getPublicationTree({ filter }: PublicationTreeOptions = {}): Promise<
    Theme[]
  > {
    return contentApi.get('/publications/tree', {
      params: { filter },
    });
  },
  listSitemapItems(): Promise<PublicationSitemapItem[]> {
    return contentApi.get('/publications/sitemap-items');
  },
};
export default publicationService;
