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
            id: 'theme-1-topic-1-pub-1',
            slug: 'education-provision-children-under-5',
            title: 'Education provision: children under 5 years of age',
            type: 'NationalAndOfficial',
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
            id: 'theme-2-topic-1-pub-1',
            slug: 'school-places-sufficiency-survey',
            title: 'School places sufficiency survey',
            type: 'AdHoc',
          },
          {
            id: 'theme-2-topic-1-pub-2',
            slug: 'local-authority-school-places-scorecards',
            title: 'Local authority school places scorecards',
            type: 'NationalAndOfficial',
          },
          {
            id: 'theme-2-topic-1-pub-3',
            slug: 'school-capacity',
            title: 'School capacity',
            type: 'NationalAndOfficial',
          },
        ],
      },
      {
        id: 'theme-2-topic-2',
        title: 'Special educational needs (SEN)',
        summary: '',
        publications: [
          {
            id: 'theme-2-topic-2-pub-1',
            legacyPublicationUrl:
              'https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs',
            slug:
              'special-educational-needs-analysis-and-summary-of-data-sources',
            title:
              'Special educational needs: analysis and summary of data sources',
            type: 'Legacy',
          },
          {
            id: 'theme-2-topic-2-pub-2',
            slug: 'education-health-and-care-plans',
            title: 'Education, health and care plans',
            type: 'NationalAndOfficial',
          },
          {
            id: 'theme-2-topic-2-pub-3',
            slug: 'special-educational-needs-in-england',
            title: 'Special educational needs in England',
            type: 'NationalAndOfficial',
          },
        ],
      },
    ],
  },
];

export const testThemeWithAllPublicationTypes: Theme = {
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
          id: 'theme-1-topic-1-pub-1',
          slug: 'ad-hoc-statistics',
          title: 'Ad hoc statistics',
          type: 'AdHoc',
        },
        {
          id: 'theme-1-topic-1-pub-2',
          slug: 'experimental-statistics',
          title: 'Experimental statistics',
          type: 'Experimental',
        },
        {
          id: 'theme-1-topic-1-pub-3',
          legacyPublicationUrl: 'https://legacy.statistics',
          slug: 'legacy-statistics',
          title: 'Legacy satistics',
          type: 'Legacy',
        },
        {
          id: 'theme-1-topic-1-pub-4',
          slug: 'management-information',
          title: 'Management information',
          type: 'ManagementInformation',
        },
        {
          id: 'theme-1-topic-1-pub-5',
          slug: 'national-statistics',
          title: 'National satistics',
          type: 'NationalAndOfficial',
        },
      ],
    },
  ],
};
