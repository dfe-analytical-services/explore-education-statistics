import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';

export const testContact: PublicationContactDetails = {
  contactName: 'John Smith',
  contactTelNo: '0777777777',
  id: 'contact-id-1',
  teamEmail: 'john.smith@test.com',
  teamName: 'Team Smith',
};

export const testPublication: MyPublication = {
  id: 'publication-1',
  title: 'Publication 1',
  contact: testContact,
  releases: [],
  legacyReleases: [],
  methodologies: [],
  themeId: 'theme-1',
  topicId: 'theme-1-topic-2',
  permissions: {
    canAdoptMethodologies: true,
    canCreateReleases: true,
    canUpdatePublication: true,
    canUpdatePublicationTitle: true,
    canUpdatePublicationSupersededBy: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
  },
};
