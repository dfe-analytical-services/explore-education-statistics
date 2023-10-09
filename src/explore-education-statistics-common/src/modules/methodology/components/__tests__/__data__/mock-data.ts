import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/mock-data';
import { Methodology } from '@common/services/methodologyService';

const mockMethodology: Methodology = {
  id: 'mockMethodologyId',
  title: 'Mock Methodology Title',
  published: '11th Mocktober, 2020',
  slug: 'Mock Methodology Slug',
  publications: [],
  content: [],
  annexes: [],
  notes: [],
  contact: mockContact,
};

export default mockMethodology;
