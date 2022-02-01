import { Theme } from '@common/services/themeService';

export const testThemes: Theme[] = [
  {
    id: 'theme-1',
    title: 'Early years',
    summary:
      'Including early years foundation stage profile and early years surveys statistics',
    topics: [
      {
        id: 'theme-1-topic-1',
        title: 'Childcare and early years',
        summary: '',
        publications: [
          {
            latestReleaseType: 'NationalStatistics',
            id: 'theme-1-topic-1-pub-1',
            title: 'Education provision: children under 5 years of age',
            slug: 'education-provision-children-under-5',
          },
        ],
      },
    ],
  },
  {
    id: 'theme-2',
    title: 'Pupils and schools',
    summary:
      'Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics',
    topics: [
      {
        id: 'theme-2-topic-1',
        title: 'School capacity',
        summary: '',
        publications: [
          {
            latestReleaseType: 'AdHocStatistics',
            id: 'theme-2-topic-1-pub-1',
            title: 'School places sufficiency survey',
            slug: 'school-places-sufficiency-survey',
          },
          {
            latestReleaseType: 'OfficialStatistics',
            id: 'theme-2-topic-1-pub-2',
            title: 'Local authority school places scorecards',
            slug: 'local-authority-school-places-scorecards',
          },
          {
            latestReleaseType: 'OfficialStatistics',
            id: 'theme-2-topic-1-pub-3',
            title: 'School capacity',
            slug: 'school-capacity',
          },
        ],
      },
      {
        id: 'theme-2-topic-2',
        title: 'Special educational needs (SEN)',
        summary: '',
        publications: [
          {
            legacyPublicationUrl:
              'https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs',
            id: 'theme-2-topic-2-pub-1',
            title:
              'Special educational needs: analysis and summary of data sources',
            slug:
              'special-educational-needs-analysis-and-summary-of-data-sources',
          },
          {
            latestReleaseType: 'NationalStatistics',
            id: 'theme-2-topic-2-pub-2',
            title: 'Education, health and care plans',
            slug: 'education-health-and-care-plans',
          },
          {
            latestReleaseType: 'NationalStatistics',
            id: 'theme-2-topic-2-pub-3',
            title: 'Special educational needs in England',
            slug: 'special-educational-needs-in-england',
          },
        ],
      },
    ],
  },
];

export const testThemeWithAllReleaseTypes: Theme = {
  id: 'theme-1',
  title: 'Test theme',
  summary: '',
  topics: [
    {
      id: 'theme-1-topic-1',
      title: 'Test topic',
      summary: '',
      publications: [
        {
          latestReleaseType: undefined,
          legacyPublicationUrl: 'https://legacy.statistics',
          id: 'theme-1-topic-1-pub-1',
          title: 'Legacy satistics',
          slug: 'legacy-statistics',
        },
        {
          latestReleaseType: 'AdHocStatistics',
          id: 'theme-1-topic-1-pub-2',
          title: 'Ad hoc statistics',
          slug: 'ad-hoc-statistics',
        },
        {
          latestReleaseType: 'ExperimentalStatistics',
          id: 'theme-1-topic-1-pub-3',
          title: 'Experimental statistics',
          slug: 'experimental-statistics',
        },
        {
          latestReleaseType: 'ManagementInformation',
          id: 'theme-1-topic-1-pub-4',
          title: 'Management information',
          slug: 'management-information',
        },
        {
          latestReleaseType: 'NationalStatistics',
          id: 'theme-1-topic-1-pub-5',
          title: 'National satistics',
          slug: 'national-statistics',
        },
        {
          latestReleaseType: 'OfficialStatistics',
          id: 'theme-1-topic-1-pub-6',
          title: 'Official satistics',
          slug: 'official-statistics',
        },
      ],
    },
  ],
};
