import { MethodologyContent } from 'src/services/methodologyContentService';
import { MethodologyVersion } from 'src/services/methodologyService';

const testMethodology: MethodologyVersion = {
  id: 'm1',
  amendment: false,
  methodologyId: 'm-1',
  title: 'Test methodology',
  slug: 'test-methodology',
  status: 'Draft',
  otherPublications: [
    {
      id: 'op1',
      title: 'Other publication title 1',
      contact: {
        teamName: 'mock team name 2',
        teamEmail: 'mock team email 2',
        contactName: 'mock contact name 2',
      },
    },
    {
      id: 'op2',
      title: 'Other publication title 2',
      contact: {
        teamName: 'mock team name 3',
        teamEmail: 'mock team email 3',
        contactName: 'mock contact name 3',
      },
    },
  ],
  owningPublication: {
    id: 'p1',
    title: 'Publication title',
    contact: {
      teamName: 'mock team name 1',
      teamEmail: 'mock team email 1',
      contactName: 'mock contact name 1',
    },
  },
  published: '',
};

export const testMethodologyAmendment = {
  ...testMethodology,
  amendment: true,
};

export const testMethodologyContent: MethodologyContent = {
  id: 'mc-1',
  title: 'The content',
  slug: 'content-1',
  status: 'Draft',
  content: [],
  annexes: [],
  notes: [],
};

export default testMethodology;
