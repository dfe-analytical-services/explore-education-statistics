import { ContactDetails, IdTitlePair } from '@admin/services/common/types';

const methodologies: IdTitlePair[] = [
  {
    id: 'methodology-1',
    title: 'A guide to absence statistics',
  },
  {
    id: 'methodology-2',
    title: 'Children missing education',
  },
  {
    id: 'methodology-3',
    title: 'School attendance',
  },
  {
    id: 'methodology-4',
    title: 'School attendance parental responsibility measures',
  },
];

const publicationAndReleaseContacts: ContactDetails[] = [
  {
    id: 'contact-1',
    contactName: 'Alex Miller',
    contactTelNo: '07912 345678',
    teamEmail: 'team1@example.com',
    teamName: 'Team 1',
  },
  {
    id: 'contact-2',
    contactName: 'Mark Pearson',
    contactTelNo: '07123 456789',
    teamEmail: 'team2@example.com',
    teamName: 'Team 2',
  },
];

export default {
  getMethodologies: () => methodologies,
  getPublicationAndReleaseContacts: () => publicationAndReleaseContacts,
};
