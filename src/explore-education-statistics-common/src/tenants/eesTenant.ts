import { Tenant } from '@common/tenants/types';

// eslint-disable-next-line import/prefer-default-export -- named export matches other tenant files (e.g. egsTenant.ts) and the `index.ts` barrel
export const eesTenant = {
  key: 'ees',
  serviceName: 'Explore education statistics',
  organisation: {
    name: 'Department for Education',
    url: 'https://www.gov.uk/government/organisations/department-for-education',
    publicEnquiries: {
      description:
        'If you have a general enquiry about the Department for Education (DfE) or education:',
      telephone: 'Phone: 0370 000 2288',
      openingHours:
        'Monday to Friday from 9.30am to 5.30pm (closed on bank holidays)',
    },
  },
  contactEmail: 'explore.statistics@education.gov.uk',
  relatedServices: [
    {
      title: 'Statistics at DfE',
      url: 'https://www.gov.uk/government/organisations/department-for-education/about/statistics',
      description:
        'Find out more about latest news, announcements, forthcoming releases and ad hoc publications, as well as related education statistics.',
    },
    {
      title: 'Find and compare school and college profiles service',
      url: 'https://find-compare-schools-colleges.service.gov.uk/',
      description:
        'Search for information about schools and colleges in England, including performance data.',
    },
    {
      title: 'Get information about schools',
      url: 'https://www.get-information-schools.service.gov.uk/',
      description:
        'Search to find and download information about schools, colleges, educational organisations and governors in England.',
    },
    {
      title: 'Financial Benchmarking and Insights Tool',
      url: 'https://financial-benchmarking-and-insights-tool.education.gov.uk/',
      description:
        "Compare your school's expenditure with other schools in England.",
    },
  ],
  userResearchBanner: {
    title: 'Shape the future of Explore education statistics',
    linkText: 'Share your feedback to help us improve this service',
    url: 'https://forms.office.com/e/QVWqCbbxCj',
  },
  pressOffice: {
    description: 'If you have a media enquiry:',
    telephone: 'Telephone: 020 7783 8300',
  },
  headOfProfession: {
    title: 'DfE Head of Profession for Statistics',
    email: 'hop.statistics@education.gov.uk',
  },
  freedomOfInformationUrl:
    'https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-f1453496-7d8a-463f-9f33-1da2ac47ed76/AF-Stage-1e64d4cc-25fb-499a-a8d7-74e98203ac00/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen',
  officialStatisticsStandardsUrl:
    'https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education',
} as const satisfies Tenant;
