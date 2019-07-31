import {ContactDetails, IdLabelPair} from "@admin/services/common/types";

const methodologies: IdLabelPair[] = [
  {
    id: 'methodology-1',
    label: 'A guide to absence statistics',
  },
  {
    id: 'methodology-2',
    label: 'Children missing education',
  },
  {
    id: 'methodology-3',
    label: 'School attendance',
  },
  {
    id: 'methodology-4',
    label: 'School attendance parental responsibility measures',
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
