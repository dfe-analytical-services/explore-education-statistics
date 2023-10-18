import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/test-data';
import { PublicationSummary } from '@common/services/publicationService';

const testPublicationSummary: PublicationSummary = {
  id: 'Mock Publication Id',
  slug: 'Mock Publication Slug',
  title: 'Mock Publication Title',
  owner: false,
  contact: mockContact,
};

export default testPublicationSummary;
