import { PublicationSummaryPreview } from '@common/services/publicationService';
import { IdTitlePair } from '@admin/services/types/common';
import { UserWithRoles } from '@admin/services/types/userWithRoles';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import { GlobalRole } from '@admin/services/types/GlobalRole';

export const testStandardUser: UserWithRoles = {
  id: 'user-1-id',
  name: 'Florian Schneider',
  email: 'test@test.com',
  globalRole: GlobalRole.StandardUser,
  userPublicationRoles: [
    {
      id: 'pr-id-1',
      publication: 'Publication 1',
      role: PublicationRole.Approver,
    },
    {
      id: 'pr-id-2',
      publication: 'Publication 2',
      role: PublicationRole.Drafter,
    },
  ],
  userPreReleaseRoles: [
    {
      id: 'rr-id-1',
      publication: 'Publication 1',
      release: 'Release 2',
    },
    {
      id: 'rr-id-2',
      publication: 'Publication 2',
      release: 'Release 2',
    },
  ],
};

export const testBauUser: UserWithRoles = {
  id: 'bau-user',
  name: 'Timothy Smith',
  email: 'timothy@test.com',
  globalRole: GlobalRole.BauUser,
  userPublicationRoles: [],
  userPreReleaseRoles: [],
};

export const testPublicationSummaries: PublicationSummaryPreview[] = [
  {
    id: 'publication-1-id',
    slug: 'publication-1-slug',
    latestReleaseSlug: 'latest-release-slug-1',
    title: 'Publication 1',
    owner: false,
    contact: {
      teamName: 'Mock Contact Team Name',
      teamEmail: 'Mock Contact Team Email',
      contactName: 'Mock Contact Name',
    },
  },
  {
    id: 'publication-2-id',
    slug: 'publication-2-slug',
    latestReleaseSlug: 'latest-release-slug-2',
    title: 'Publication 2',
    owner: false,
    contact: {
      teamName: 'Mock Contact Team Name',
      teamEmail: 'Mock Contact Team Email',
      contactName: 'Mock Contact Name',
    },
  },
  {
    id: 'publication-3-id',
    slug: 'publication-3-slug',
    latestReleaseSlug: 'latest-release-slug-3',
    title: 'Publication 3',
    owner: false,
    contact: {
      teamName: 'Mock Contact Team Name',
      teamEmail: 'Mock Contact Team Email',
      contactName: 'Mock Contact Name',
    },
  },
];

export const testReleases: IdTitlePair[] = [
  {
    id: 'release-1-id',
    title: 'Release 1',
  },
  {
    id: 'release-2-id',
    title: 'Release 2',
  },
  {
    id: 'release-3-id',
    title: 'Release 3',
  },
];
