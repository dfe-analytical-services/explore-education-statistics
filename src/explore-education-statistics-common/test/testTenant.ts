import { Tenant } from '@common/tenants/types';

/**
 * A non-EES tenant fixture for tests that need to check brand text actually
 * comes from the tenant, rather than happening to match EES's hard-coded
 * values. Deliberately leaves out every optional section so tests can check
 * they're hidden when absent.
 */
const testTenant: Tenant = {
  key: 'ees',
  service: {
    nameStartSentence: 'Test Statistics Service',
    nameMidSentence: 'test statistics service',
  },
  organisation: {
    name: 'Test Organisation',
    nameWithArticle: 'the Test Organisation',
    acronym: 'TO',
    url: 'https://test-organisation.example.com',
    description: 'Test Organisation is responsible for testing things.',
    publicEnquiries: {
      description: 'Have a general question about testing?',
      telephoneCallToAction: 'Call the test enquiries line:',
      telephone: 'Phone: 01234 567890',
      openingHours: 'Monday to Friday from 9am to 5pm',
    },
  },
  contactEmail: 'test.contact@example.com',
};

export default testTenant;
