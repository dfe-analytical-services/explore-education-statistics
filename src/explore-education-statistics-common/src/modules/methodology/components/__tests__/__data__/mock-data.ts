import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/mock-data';
import { MethodologyPublication } from '@common/services/methodologyService';
import { PublicationSummary } from '@common/services/publicationService';

export const mockPublicationSummary: PublicationSummary = {
  id: 'Mock Publication Id',
  slug: 'Mock Publication Slug',
  title: 'Mock Publication Title',
  owner: false,
  contact: mockContact,
};

export const mockMethodologyPublication: MethodologyPublication = {
  id: 'Mock Publication Id',
  title: 'Mock Publication Title',
  contact: mockContact,
};
