import {
  Publication,
  PublicationSummaryRedesign,
  ReleaseVersion,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';

export const testPublication: Publication = {
  id: 'publication-1',
  title: 'Pupil absence in schools in England',
  slug: 'pupil-absence-in-schools-in-england',
  releaseSeries: [
    {
      isLegacyLink: false,
      description: 'Academic year 2018/19',
      releaseSlug: '2018-19',
    },
    {
      isLegacyLink: true,
      description: 'Academic year 2014/15',
      releaseSlug: '2014-15',
      legacyLinkUrl:
        'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
    },
    {
      isLegacyLink: false,
      description: 'Academic year 2017/18',
      releaseSlug: '2017-18',
    },
    {
      isLegacyLink: true,
      description: 'Academic year 2013/14',
      releaseSlug: '2013-14',
      legacyLinkUrl:
        'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014',
    },
    {
      isLegacyLink: true,
      description: 'Academic year 2012/13',
      releaseSlug: '2012-13',
      legacyLinkUrl:
        'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013',
    },
  ],
  theme: {
    id: 'test-theme',
    title: 'Pupils and schools',
  },
  contact: {
    teamName: 'School absence and exclusions team',
    teamEmail: 'schools.statistics@education.gov.uk',
    contactName: 'Sean Gibson',
    contactTelNo: '0370 000 2288',
  },
  methodologies: [
    {
      id: 'caa8e56f-41d2-4129-a5c3-53b051134bd7',
      slug: 'pupil-absence-in-schools-in-england',
      title: 'Pupil absence statistics: methodology',
    },
  ],
  supersededById: 'superseding-publication-id',
  isSuperseded: false,
};

export const testRelease: ReleaseVersion = {
  latestRelease: true,
  publication: testPublication,
  id: 'release-1',
  title: 'Academic year 2016/17',
  yearTitle: '2016/17',
  coverageTitle: 'Academic year',
  nextReleaseDate: {
    year: '2019',
    month: '3',
    day: '22',
  },
  published: '2018-04-25T09:30:00',
  slug: '2016-17',
  hasDataGuidance: true,
  hasPreReleaseAccessList: true,
  type: 'AccreditedOfficialStatistics',
  updates: [
    {
      id: '18e0d40e-bdf7-4c84-99dd-732e72e9c9a5',
      reason: 'First update',
      on: new Date('2018-03-22T00:00:00'),
    },
    {
      id: '9c0f0139-7f88-4750-afe0-1c85cdf1d047',
      reason: 'Second update',
      on: new Date('2018-04-19T00:00:00'),
    },
  ],
  content: [
    {
      id: '24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94',
      order: 1,
      heading: 'About these statistics',
      content: [
        {
          id: '7eeb1478-ab26-4b70-9128-b976429efa2f',
          order: 0,
          body: '<p>The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:</p><ul><li>primary schools</li><li>secondary schools</li><li>special schools</li></ul><p>They also include information for <a href="/glossary#pupil-referral-unit">pupil referral units</a> and pupils aged 4 years.</p><p>We use the key measures of <a href="/glossary#overall-absence">overall absence</a> and <a href="/glossary#persistent-absence">persistent absence</a> to monitor pupil absence and also include <a href="#contents-sections-heading-4">absence by reason</a> and <a href="#contents-sections-heading-6">pupil characteristics</a>.</p><p>The statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.</p><p>They\'re also used for policy development as key indicators in behaviour and school attendance policy.</p><p>Within this release, absence by reason is broken down in three different ways:</p><ul><li><p>distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences</p></li><li><p>rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions</p></li><li><p>one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason</p></li></ul>',
          type: 'HtmlBlock',
        },
      ],
    },
    {
      id: '8965ef44-5ad7-4ab0-a142-78453d6f40af',
      order: 2,
      heading: 'Pupil absence rates',
      content: [
        {
          id: '2c369594-3bbc-40b4-ad19-196c923f5c7f',
          order: 0,
          body: '<p><strong>Overall absence</strong></p><p>The <a href="/glossary#overall-absence">overall absence</a> rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.</p><p>It increased from 4.6% to 4.7% over this period while the <a href="/glossary#unauthorised-absence">unauthorised absence</a> rate increased from 1.1% to 1.3%.</p><p>The rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.</p><p>The overall and <a href="/glossary#authorised-absence">authorised absence</a> rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14.</p>',
          type: 'HtmlBlock',
        },
        {
          id: '3913a0af-9455-4802-a037-c4cfd4719b18',
          order: 0,
          body: '<p><strong>Unauthorised absence</strong></p><p>The <a href="/glossary#unauthorised-absence">unauthorised absence</a> rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.</p><p>This is due to an increase in absence due to family holidays not agreed by schools.</p>',
          type: 'HtmlBlock',
        },
      ],
    },
    {
      id: '6f493eee-443a-4403-9069-fef82e2f5788',
      order: 3,
      heading: 'Persistent absence',
      content: [
        {
          id: '8a8add13-368c-4067-9210-166bb19a00c1',
          order: 0,
          body: '<p>The <a href="/glossary#persistent-absence">persistent absence</a> rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015/16 but still down from 43.3% in 2011/12.</p><p>It also accounted for almost a third (31.6%) of all <a href="/glossary#authorised-absence">authorised absence</a> and more than half (53.8%) of all <a href="/glossary#unauthorised-absence">unauthorised absence</a>.</p><p>Overall, it\'s increased across primary and secondary schools to 10.8% - up from 10.5% in 2015/16.</p>',
          type: 'HtmlBlock',
        },
        {
          id: '4aa06200-406b-4f5a-bee4-19e3b83eb1d2',
          order: 0,
          body: '<p><strong>Persistent absentees</strong></p><p>The <a href="/glossary#overall-absence">overall absence</a> rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.</p><p><strong>Illness absence rate</strong></p><p>The illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils.</p>',
          type: 'HtmlBlock',
        },
      ],
    },
    {
      id: 'fbf99442-3b72-46bc-836d-8866c552c53d',
      order: 4,
      heading: 'Reasons for absence',
      content: [
        {
          id: '2ef5f84f-e151-425d-8906-2921712f9157',
          order: 0,
          body: '<p><strong>Illness</strong></p><p>This is the main driver behind <a href="/glossary#overall-absence">overall absence</a> and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.</p><p>While the overall absence rate has slightly increased since 2015/16 the illness rate has stayed the same at 2.6%.</p><p>The absence rate due to other unauthorised circumstances has also stayed the same since 2015/16 at 0.7%.</p>',
          type: 'HtmlBlock',
        },
      ],
    },
  ],
  summarySection: {
    id: '4f30b382-ce28-4a3e-801a-ce76004f5eb4',
    order: 1,
    heading: '',
    content: [
      {
        id: 'a0b85d7d-a9bd-48b5-82c6-a119adc74ca2',
        order: 1,
        body: '<p>Read national statistical summaries, view charts and tables and download data files.</p><p>Find out how and why these statistics are collected and published - <a href="/methodology/pupil-absence-in-schools-in-england">Pupil absence statistics: methodology</a>.</p><p>This release was created as example content during the platform\'s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original, release please see <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2016-to-2017">Pupil absence in schools in England: 2016 to 2017</a></p>',
        type: 'HtmlBlock',
      },
    ],
  },
  headlinesSection: {
    id: 'headlines-id',
    order: 1,
    heading: '',
    content: [
      {
        id: 'b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9',
        order: 1,
        body: '<ul><li>pupils missed on average 8.2 school days</li><li>overall and unauthorised absence rates up on 2015/16</li><li>unauthorised absence rise due to higher rates of unauthorised holidays</li><li>10% of pupils persistently absent during 2016/17</li></ul>',
        type: 'HtmlBlock',
      },
    ],
  },
  keyStatistics: [],
  keyStatisticsSecondarySection: {
    id: 'key-stats-secondary-id',
    order: 1,
    heading: '',
    content: [],
  },
  relatedDashboardsSection: {
    id: 'related-dashboards-id',
    order: 0,
    heading: '',
    content: [
      {
        id: 'related-dashboards-content-block-id',
        order: 0,
        body: 'Related dashboards test text',
        type: 'HtmlBlock',
      },
    ],
  },
  downloadFiles: [],
  relatedInformation: [],
};

export const testPublicationSummary: PublicationSummaryRedesign = {
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
};
