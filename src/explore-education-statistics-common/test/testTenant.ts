import { Tenant } from '@common/tenants/types';
import omit from 'lodash/omit';

/**
 * A non-EES tenant fixture for tests that need to check brand text actually
 * comes from the tenant, rather than happening to match EES's hard-coded
 * values. Deliberately leaves out every optional top-level section so tests
 * can check they're hidden when absent.
 *
 * See {@link minimalTestTenant} for a tenant without the optional
 * organisation fields as well, and {@link fullTestTenant} for one with every
 * optional section supplied.
 */
const testTenant: Tenant = {
  key: 'ees',
  service: {
    nameStartSentence: 'Test statistics',
    nameMidSentence: 'test statistics',
    subjectMidSentence: 'testing',
    productionUrl: 'https://test-statistics-service.example.com',
  },
  organisation: {
    name: 'Test Organisation',
    nameWithArticle: 'the Test Organisation',
    acronym: 'TO',
    url: 'https://test-organisation.example.com',
    description: 'Test Organisation is responsible for testing things.',
    publicEnquiries: {
      question: 'Have a general question about testing?',
      description: 'If you have a general enquiry about testing:',
      telephoneCallToAction: 'Call the test enquiries line:',
      telephone: 'Phone: 01234 567890',
      openingHours: 'Monday to Friday from 9am to 5pm',
    },
  },
  contactEmail: 'test.contact@example.com',
};

/**
 * {@link testTenant} without any optional fields at all, including the
 * organisation's acronym and public enquiries details.
 */
export const minimalTestTenant: Tenant = {
  ...testTenant,
  organisation: omit(testTenant.organisation, ['acronym', 'publicEnquiries']),
};

/**
 * {@link testTenant} with every optional section supplied, using non-EES
 * values.
 */
export const fullTestTenant: Tenant = {
  ...testTenant,
  relatedServices: [
    {
      title: 'Test Related Service',
      url: 'https://related-service.example.com',
      description: 'Find out more about testing.',
    },
  ],
  userResearchBanner: {
    title: 'Shape the future of the test statistics service',
    linkText: 'Tell us what you think',
    url: 'https://test-organisation.example.com/research',
  },
  pressOffice: {
    telephone: 'Telephone: 09876 543210',
  },
  headOfProfession: {
    title: 'Test Head of Profession for Statistics',
    email: 'test.hop@example.com',
  },
  freedomOfInformationUrl: 'https://test-organisation.example.com/foi',
  officialStatisticsStandardsUrl:
    'https://test-organisation.example.com/standards',
};

export default testTenant;
