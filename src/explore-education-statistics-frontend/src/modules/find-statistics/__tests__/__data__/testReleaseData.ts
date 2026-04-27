import {
  PublicationSummary,
  ReleaseVersionDataContent,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';

export const testPublicationSummary: PublicationSummary = {
  id: 'publication-summary-1',
  title: 'Pupil attendance in schools',
  slug: 'publication-slug',
  summary:
    'Pupil attendance and absence data including termly national statistics and fortnightly statistics in development derived from DfE’s regular attendance data',
  latestRelease: {
    slug: 'latest-release-slug',
    title: 'Calendar year 2024 - Final',
    id: 'release-version-summary-1',
  },
  nextReleaseDate: { year: 2026, month: 3 },
  theme: {
    id: 'test-theme-id',
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

export const testReleaseVersionSummary: ReleaseVersionSummary = {
  id: 'release-version-summary-1',
  slug: 'release-slug',
  type: 'AccreditedOfficialStatistics',
  title: 'Calendar year 2024 - Final',
  yearTitle: '2024',
  coverageTitle: 'Calendar year',
  label: 'Final',
  published: '2025-08-10T09:30:00+01:00',
  lastUpdated: '2025-08-11T14:30:00+01:00',
  isLatestRelease: true,
  preReleaseAccessList: '<p>Pre-release access list content</p>',
  updateCount: 5,
};

export const testReleaseHomeContent: ReleaseVersionHomeContent = {
  releaseId: 'test-release-id',
  releaseVersionId: 'test-release-version-id',
  content: [
    {
      id: 'test-section-1-id',
      heading: 'Section 1 heading',
      content: [
        {
          type: 'HtmlBlock',
          body: '<p>Section 1 block 1 text</p>',
          id: 'test-section-1-block-1-id',
        },
        {
          type: 'DataBlock',
          dataBlockVersion: {
            dataBlockVersionId: 'test-section-1-block-2-data-block-version-id',
            dataBlockParentId: 'test-section-1-block-2-data-block-parent-id',
            charts: [
              {
                type: 'infographic',
                fileId: 'test-chart-file-id',
                title: 'Chart title',
                subtitle: 'Chart subtitle',
                alt: 'Alt text',
                height: 600,
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
              subjectId: 'test-subject-id',
              locationIds: ['test-location-id'],
              timePeriod: {
                startYear: 2025,
                startCode: 'AY',
                endYear: 2025,
                endCode: 'AY',
              },
              filters: ['test-filter-id'],
              indicators: ['test-indicator-id'],
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
                      value: 'filter-id',
                      type: 'Filter',
                    },
                  ],
                  [
                    {
                      level: 'localAuthority',
                      value: 'location-id',
                      type: 'Location',
                    },
                  ],
                ],
                rows: [
                  {
                    value: 'indicator-id',
                    type: 'Indicator',
                  },
                ],
              },
            },
          },
          id: 'test-section-1-block-2-id',
        },
        {
          type: 'HtmlBlock',
          body: '<p>Section 1 block 3 text</p>',
          id: 'test-section-1-block-3-id',
        },
      ],
    },
    {
      id: 'test-section-2-id',
      heading: 'Section 2 heading',
      content: [
        {
          type: 'HtmlBlock',
          body: '<h3>Section 2 subheading</h3><p>Section 2 block 1 text</p>',
          id: 'test-section-2-block-1-id',
        },
      ],
    },
    {
      id: 'test-section-3-id',
      heading: 'Section 3 heading',
      content: [
        {
          type: 'EmbedBlock',
          embedBlock: {
            embedBlockId: 'test-embed-block-id',
            title: 'Embedded dashboard title',
            url: 'https://department-for-education.shinyapps.io/test-dashboard',
          },
          id: 'test-section-3-block-1-id',
        },
      ],
    },
  ],
  headlinesSection: {
    id: 'test-headlines-section-id',
    content: [
      {
        type: 'HtmlBlock',
        body: '<p>Headlines section text</p>',
        id: 'test-headlines-section-block-1-id',
      },
    ],
  },
  keyStatistics: [
    {
      type: 'KeyStatisticText',
      statistic: '999',
      title: 'Key statistic 1 title',
      id: 'test-key-statistic-1-id',
      guidanceText: 'Guidance text',
      guidanceTitle: 'Guidance title 1',
      trend: 'Trend',
    },
    {
      type: 'KeyStatisticDataBlock',
      dataBlockVersionId: 'test-key-statistic-2-data-block-version-id',
      dataBlockParentId: 'test-key-statistic-2-data-block-parent-id',
      id: 'test-key-statistic-2-id',
    },
    {
      type: 'KeyStatisticText',
      statistic: '999',
      title: 'Key statistic 2 title',
      id: 'test-key-statistic-3-id',
      guidanceText: 'Guidance text',
      guidanceTitle: 'Guidance title 2',
      trend: 'Trend',
    },
  ],
  keyStatisticsSecondarySection: {
    id: 'test-key-statistics-secondary-section-id',
    content: [
      {
        type: 'DataBlock',
        dataBlockVersion: {
          dataBlockVersionId:
            'test-key-statistics-secondary-data-block-version-id',
          dataBlockParentId:
            'test-key-statistics-secondary-data-block-parent-id',
          charts: [
            {
              type: 'infographic',
              fileId: 'test-key-statistics-secondary-chart-file-id',
              title: 'Test chart title',
              subtitle: 'Test chart subtitle',
              alt: 'Alt text',
              height: 600,
              includeNonNumericData: false,
              axes: {},
            },
          ],
          heading: 'Test data block heading',
          name: 'Test data block name',
          query: {
            subjectId: 'test-subject-id',
            locationIds: ['test-location-id'],
            timePeriod: {
              startYear: 2025,
              startCode: 'AY',
              endYear: 2025,
              endCode: 'AY',
            },
            filters: ['test-filter-id'],
            indicators: ['test-indicator-id'],
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
                    value: 'filter-id',
                    type: 'Filter',
                  },
                ],
                [
                  {
                    level: 'localAuthority',
                    value: 'location-id',
                    type: 'Location',
                  },
                ],
              ],
              rows: [
                {
                  value: 'indicator-id',
                  type: 'Indicator',
                },
              ],
            },
          },
        },
        id: 'test-key-statistics-secondary-block-1-id',
      },
    ],
  },
  summarySection: {
    id: 'test-summary-section-id',
    content: [
      {
        type: 'HtmlBlock',
        body: '<p>Summary section text</p>',
        id: 'test-summary-section-block-1-id',
      },
    ],
  },
  warningSection: {
    id: 'test-warning-section-id',
    content: [
      {
        type: 'HtmlBlock',
        body: '<p>Warning section text</p>',
        id: 'test-warning-section-block-1-id',
      },
    ],
  },
};

export const testReleaseDataContent: ReleaseVersionDataContent = {
  releaseId: 'test-release-id',
  releaseVersionId: 'test-release-version-id',
  dataDashboards: '<h3>Data dashboard text</h3>',
  dataGuidance: 'Test data guidance',
  dataSets: [
    {
      dataSetFileId: 'test-dataset-1-datasetfileid',
      fileId: 'test-dataset-1-fileid',
      subjectId: 'test-dataset-1-subjectid',
      meta: {
        filters: ['Characteristic', 'School type'],
        geographicLevels: [
          'Local authority',
          'Local authority district',
          'National',
        ],
        indicators: [
          'Authorised absence rate',
          'Authorised absence rate exact',
        ],
        numDataFileRows: 1000,
        timePeriodRange: {
          start: '2012/13',
          end: '2016/17',
        },
      },
      title: 'Test dataset 1',
      summary: '<p>Test dataset 1 summary</p>',
      isApiEnabled: false,
    },
    {
      dataSetFileId: 'test-dataset-2-datasetfileid',
      fileId: 'test-dataset-2-fileid',
      subjectId: 'test-dataset-2-subjectid',
      meta: {
        filters: ['Characteristic', 'School type'],
        geographicLevels: ['Local authority', 'National', 'Regional'],
        indicators: [
          'Authorised absence rate',
          'Number of authorised absence sessions',
        ],
        numDataFileRows: 2000,
        timePeriodRange: {
          start: '2013/14',
          end: '2016/17',
        },
      },
      title: 'Test dataset 2',
      summary: '<p>Test dataset 2 summary</p>',
      isApiEnabled: false,
    },
  ],
  featuredTables: [
    {
      featuredTableId: 'featured-table-1-id',
      dataBlockId: 'featured-table-1-data-block-id',
      dataBlockParentId: 'featured-table-1-data-block-parent-id',
      title: 'Featured table 1',
      summary: 'Featured table 1 description',
    },
    {
      featuredTableId: 'featured-table-2-id',
      dataBlockId: 'featured-table-2-data-block-id',
      dataBlockParentId: 'featured-table-2-data-block-parent-id',
      title: 'Featured table 2',
      summary: 'Featured table 2 description',
    },
  ],
  supportingFiles: [
    {
      fileId: 'supporting-file-1-file-id',
      extension: 'pdf',
      filename: 'file1.pdf',
      title: 'Supporting file 1',
      summary: 'Supporting file 1 description',
      size: '10 Mb',
    },
    {
      fileId: 'supporting-file-2-file-id',
      extension: 'pdf',
      filename: 'file2.pdf',
      title: 'Supporting file 2',
      summary: 'Supporting file 2 description',
      size: '20 Mb',
    },
  ],
};
