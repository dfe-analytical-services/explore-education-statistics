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
import { Organisation } from '@common/services/types/organisation';
import { SortDirection } from '@common/services/types/sort';
import { PartialDate } from '@common/utils/date/partialDate';
import { contentApi } from './api';

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

export interface PublicationSummary {
  id: string;
  slug: string;
  latestReleaseSlug?: string;
  title: string;
  owner: boolean;
  contact: Contact;
}

// TODO EES-6449 - rename to remove 'redesign'
export interface PublicationSummaryRedesign {
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

export type KeyStatisticType =
  (typeof KeyStatisticTypes)[keyof typeof KeyStatisticTypes];

export interface KeyStatisticDataBlock extends KeyStatisticBase {
  type: 'KeyStatisticDataBlock';
  dataBlockParentId: string;
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

export interface ReleaseVersion<
  ContentBlockType extends ContentBlock = ContentBlock,
  DataBlockType extends DataBlock = DataBlock,
  EmbedBlockType extends EmbedBlock = EmbedBlock,
> {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  published: string;
  slug: string;
  summarySection: ContentSection<ContentBlockType>;
  keyStatistics: KeyStatistic[];
  keyStatisticsSecondarySection: ContentSection<DataBlockType>;
  headlinesSection: ContentSection<ContentBlockType>;
  relatedDashboardsSection?: ContentSection<ContentBlockType>; // optional because older releases may not have this section
  publication: Publication;
  publishingOrganisations?: Organisation[];
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

// Used for the redesigned release pages
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
  yearTitle: string;
}

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

export interface Theme {
  id: string;
  title: string;
  summary: string;
  publications: PublicationTreeSummary[];
}

export interface PublicationTreeOptions {
  publicationFilter?: 'DataTables' | 'DataCatalogue' | 'FastTrack';
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
  getPublicationSummaryRedesign(
    publicationSlug: string,
  ): Promise<PublicationSummaryRedesign> {
    // TODO EES-6404 - remove dummy data and reinstate API call
    // return contentApi.get(`/publications/${publicationSlug}`);
    const basePublicationSummary = {
      id: 'publication-summary-1',
      title: 'Pupil attendance in schools',
      slug: 'publication-slug',
      summary:
        'Pupil attendance and absence data including termly national statistics and fortnightly statistics in development derived from DfE’s regular attendance data',
      latestRelease: {
        slug: 'release-slug',
        title: 'Calendar year 2024 - Final',
        id: 'release-version-summary-1',
      },
      nextReleaseDate: { year: 2026, month: 3 },
      theme: {
        id: '323e4567-e89b-12d3-a456-426614174000',
        title: 'Pupils and schools',
        summary:
          'Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics',
      },
      contact: {
        teamName: 'Test team',
        teamEmail: 'test@test.com',
        contactName: 'Joe Bloggs',
        contactTelNo: '01234 567890',
        id: 'contact-id',
      },
    };
    return new Promise(resolve => {
      setTimeout(() => {
        resolve(
          publicationSlug === 'superseded-publication'
            ? {
                ...basePublicationSummary,
                supersededByPublication: {
                  id: '223e4567-e89b-12d3-a456-426614174000',
                  title: 'Superseding publication',
                  slug: 'superseding-publication',
                },
              }
            : basePublicationSummary,
        );
      }, 500);
    });
  },
  getPublicationTitle(publicationSlug: string): Promise<PublicationTitle> {
    return contentApi.get(`/publications/${publicationSlug}/title`);
  },
  listReleases(publicationSlug: string): Promise<ReleaseSummary[]> {
    return contentApi.get(`/publications/${publicationSlug}/releases`);
  },
  getLatestPublicationRelease(
    publicationSlug: string,
  ): Promise<ReleaseVersion> {
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
  ): Promise<ReleaseVersion> {
    return contentApi.get(
      `/publications/${publicationSlug}/releases/${releaseSlug}`,
    );
  },
  // TODO EES-6404 - remove dummy data and reinstate API call
  getReleaseVersionSummary(
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    publicationSlug: string,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    releaseSlug: string,
  ): Promise<ReleaseVersionSummary> {
    return new Promise(resolve => {
      setTimeout(() => {
        resolve({
          id: 'release-version-summary-1',
          slug: 'release-slug',
          type: 'AccreditedOfficialStatistics',
          title: 'Calendar year 2024 - Final',
          yearTitle: '2024',
          coverageTitle: 'Calendar year',
          label: 'Final',
          published: '2025-08-10T09:30:00+01:00',
          lastUpdated: '2025-08-11T14:30:00+01:00',
          publishingOrganisations: [
            {
              id: '5e089801-cf1a-b375-acd3-88e9d8aece66',
              title: 'Department for Education',
              url: 'https://www.gov.uk/government/organisations/department-for-education',
            },
            {
              id: '5e089801-ce1a-e274-9915-e83f3e978699',
              title: 'Skills England',
              url: 'https://www.gov.uk/government/organisations/skills-england',
            },
          ],
          isLatestRelease: true,
          updateCount: 5,
        });
      }, 500);
    });
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
  listSitemapItems(): Promise<PublicationSitemapItem[]> {
    return contentApi.get('/publications/sitemap-items');
  },
};
export default publicationService;
