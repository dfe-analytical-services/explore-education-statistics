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

export interface ContentSectionRedesign<BlockType> {
  id: string;
  heading?: string;
  content: BlockType[];
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

export interface ReleaseVersionHomeContent<
  HtmlBlockType extends HtmlBlockViewModel = HtmlBlockViewModel,
  DataBlockType extends DataBlockViewModel = DataBlockViewModel,
  EmbedBlockType extends EmbedBlockViewModel = EmbedBlockViewModel,
> {
  content: ContentSectionRedesign<
    HtmlBlockType | DataBlockType | EmbedBlockType
  >[];
  headlinesSection: ContentSectionRedesign<HtmlBlockType>;
  keyStatistics: KeyStatistic[];
  keyStatisticsSecondarySection: ContentSectionRedesign<DataBlockType>;
  summarySection: ContentSectionRedesign<HtmlBlockType>;
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
    return contentApi.get(`/publications/${publicationSlug}`);
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
    // return contentApi.get(
    //   `/publications/${publicationSlug}/releases/${releaseSlug}/content`,
    // );
    return new Promise(resolve => {
      return resolve({
        content: [
          {
            id: '9924eb72-b379-4b03-8711-08de0193c305',
            heading: 'Section 1 heading',
            content: [
              {
                type: 'HtmlBlock',
                body: '<p>Section 1 block 1 text</p>',
                id: '126dac70-871a-43de-5bf3-08de019491ca',
              },
              {
                type: 'DataBlock',
                dataBlockVersion: {
                  dataBlockVersionId: 'ec58d7a2-9831-4e40-94fa-7dc1bd568f7d',
                  dataBlockParentId: 'faa3b3e5-03da-476e-44ff-08de019426f3',
                  charts: [
                    {
                      type: 'infographic',
                      fileId: '5b2051df-7109-43a2-81cc-289652cb6abc',
                      title: 'Chart title',
                      subtitle: 'Chart subtitle',
                      alt: 'Alt text',
                      height: 600,
                      width: 400,
                      includeNonNumericData: false,
                      axes: {},
                      legend: {
                        items: [],
                      },
                    },
                  ],
                  heading: 'Data block heading',
                  name: 'Data block name',
                  query: {
                    subjectId: '7e9c990e-f141-4d36-0edb-08de0193fc7d',
                    locationIds: ['2daf4d3d-c546-4e59-4a50-08de0193fccb'],
                    timePeriod: {
                      startYear: 2025,
                      startCode: 'AY',
                      endYear: 2025,
                      endCode: 'AY',
                    },
                    filters: ['649330c3-3ba1-4edd-88cf-7caedaee04f2'],
                    indicators: ['482b15b7-4e70-4ebf-4a49-08de0193fccb'],
                  },
                  source: 'Source',
                  table: {
                    tableHeaders: {
                      columnGroups: [],
                      columns: [
                        {
                          value: '2025_AY',
                          type: 'TimePeriod',
                        },
                      ],
                      rowGroups: [
                        [
                          {
                            value: '649330c3-3ba1-4edd-88cf-7caedaee04f2',
                            type: 'Filter',
                          },
                        ],
                        [
                          {
                            level: 'localAuthority',
                            value: '2daf4d3d-c546-4e59-4a50-08de0193fccb',
                            type: 'Location',
                          },
                        ],
                      ],
                      rows: [
                        {
                          value: '482b15b7-4e70-4ebf-4a49-08de0193fccb',
                          type: 'Indicator',
                        },
                      ],
                    },
                  },
                },
                id: 'ec58d7a2-9831-4e40-94fa-7dc1bd568f7d',
              },
              {
                type: 'HtmlBlock',
                body: '<p>Section 1 block 3 text</p>',
                id: '13ddc41c-2233-4ace-5bf4-08de019491ca',
              },
            ],
          },
          {
            id: '4454f78d-b825-4ab7-8712-08de0193c305',
            heading: 'Section 2 heading',
            content: [
              {
                type: 'HtmlBlock',
                body: '<p>Section 2 block 1 text</p>',
                id: '5be861a5-1993-4665-5bf6-08de019491ca',
              },
            ],
          },
          {
            id: '09cc35e4-b876-4891-8714-08de0193c305',
            heading: 'Section 3 heading',
            content: [
              {
                type: 'EmbedBlock',
                embedBlock: {
                  embedBlockId: '8752a4d1-3e65-4c9e-8572-470552242d5f',
                  title: 'Embedded dashboard title',
                  url: 'https://department-for-education.shinyapps.io/test-dashboard',
                },
                id: '7463ff35-9ee7-4be8-5bf5-08de019491ca',
              },
            ],
          },
        ],
        headlinesSection: {
          id: '8b7037eb-db5f-41ea-870f-08de0193c305',
          content: [
            {
              type: 'HtmlBlock',
              body: '<p>Headlines section text</p>',
              id: 'f5740fca-a6f6-4139-5bf2-08de019491ca',
            },
          ],
        },
        keyStatistics: [
          {
            type: 'KeyStatisticText',
            statistic: '999',
            title: 'Key statistic 1 title',
            id: '8870b911-8bfb-4675-a0b0-08de0194a756',
            guidanceText: 'Guidance text',
            guidanceTitle: 'Guidance title 1',
            trend: 'Trend',
          },
          {
            type: 'KeyStatisticDataBlock',
            dataBlockVersionId: '247f9bcd-5229-4239-8e13-7dd47eee1e4a',
            dataBlockParentId: 'bb0b761a-daf1-4bf1-44fe-08de019426f3',
            id: '6acf3511-00dd-46d2-a0b2-08de0194a756',
          },
          {
            type: 'KeyStatisticText',
            statistic: '999',
            title: 'Key statistic 2 title',
            id: 'b1fcbe18-4329-43a8-a0b1-08de0194a756',
            guidanceText: 'Guidance text',
            guidanceTitle: 'Guidance title 2',
            trend: 'Trend',
          },
        ],
        keyStatisticsSecondarySection: {
          id: '88e6570e-508a-41e1-870e-08de0193c305',
          content: [
            {
              type: 'DataBlock',
              dataBlockVersion: {
                dataBlockVersionId: '29e4f3ac-6bc6-4fc2-ba8b-b3946f84af29',
                dataBlockParentId: '4494715e-a899-442d-44fd-08de019426f3',
                charts: [
                  {
                    type: 'infographic',
                    fileId: 'e88fa8b8-cb9f-4172-8db1-7923c4dd5de4',
                    title: 'Chart title',
                    subtitle: 'Chart subtitle',
                    alt: 'Alt text',
                    height: 600,
                    width: 400,
                    includeNonNumericData: false,
                    axes: {},
                  },
                ],
                heading: 'Data block heading',
                name: 'Data block name',
                query: {
                  subjectId: '5ff4b0aa-d92b-47d3-0ed7-08de0193fc7d',
                  locationIds: ['f628134d-5235-4615-2438-08dc1c5c7fdf'],
                  timePeriod: {
                    startYear: 2025,
                    startCode: 'AY',
                    endYear: 2025,
                    endCode: 'AY',
                  },
                  filters: ['e5121ea2-08d7-4d9d-ab79-5abd2c8e5cb9'],
                  indicators: ['420ec47b-b784-47b8-42f3-08de0193fccb'],
                },
                source: 'Test source',
                table: {
                  tableHeaders: {
                    columnGroups: [],
                    columns: [
                      {
                        value: '2025_AY',
                        type: 'TimePeriod',
                      },
                    ],
                    rowGroups: [
                      [
                        {
                          value: 'e5121ea2-08d7-4d9d-ab79-5abd2c8e5cb9',
                          type: 'Filter',
                        },
                      ],
                    ],
                    rows: [
                      {
                        value: '420ec47b-b784-47b8-42f3-08de0193fccb',
                        type: 'Indicator',
                      },
                    ],
                  },
                },
              },
              id: '29e4f3ac-6bc6-4fc2-ba8b-b3946f84af29',
            },
          ],
        },
        summarySection: {
          id: '7c71d1c6-84f7-4632-870d-08de0193c305',
          content: [
            {
              type: 'HtmlBlock',
              body: '<p>Summary section text</p>',
              id: '5a55634c-ab2d-4caa-5bf1-08de019491ca',
            },
          ],
        },
      });
    });
  },
  getPublicationReleaseList(
    publicationSlug: string,
    params?: PaginationRequestParams,
  ): Promise<PaginatedList<PublicationReleaseSeriesItem>> {
    return contentApi.get(`/publications/${publicationSlug}/release-entries`, {
      params,
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
  getPublicationMethodologies(
    publicationSlug: string,
  ): Promise<PublicationMethodologiesList[]> {
    return contentApi.get(`/publications/${publicationSlug}/methodologies`);
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
