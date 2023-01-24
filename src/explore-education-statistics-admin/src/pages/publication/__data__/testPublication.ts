import {
  Contact,
  PublicationWithPermissions,
} from '@admin/services/publicationService';

export const testContact: Contact = {
  contactName: 'John Smith',
  contactTelNo: '0777777777',
  teamEmail: 'john.smith@test.com',
  teamName: 'Team Smith',
};

export const testPublication: PublicationWithPermissions = {
  id: 'publication-1',
  title: 'Publication 1',
  summary: 'Publication 1 summary',
  slug: 'publication-1-slug',
  theme: { id: 'theme-1', title: 'Theme 1' },
  topic: { id: 'theme-1-topic-2', title: 'Theme 1 Topic 2' },
  permissions: {
    canAdoptMethodologies: true,
    canCreateReleases: true,
    canUpdatePublication: true,
    canUpdatePublicationSummary: true,
    canCreateMethodologies: true,
    canManageExternalMethodology: true,
    canManageLegacyReleases: true,
    canUpdateContact: true,
    canUpdateContributorReleaseRole: true,
    canViewReleaseTeamAccess: true,
  },
};
