import { PublicationSummaryPreview } from '@common/services/publicationService';
import { IdTitlePair } from '@admin/services/types/common';
import { UserWithRoles } from '@admin/services/types/userWithRoles';
import { Role } from '@admin/services/user-management/globalRolesService';
import { PublicationRole } from '@admin/services/types/PublicationRole';

export const testUser: UserWithRoles = {
  id: 'user-1-id',
  name: 'Florian Schneider',
  email: 'test@test.com',
  role: 'role-guid-1',
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

export const testRoles: Role[] = [
  {
    id: 'role-1-id',
    name: 'Role 1',
    normalizedName: 'Role 1 normalized name',
  },
  {
    id: 'role-2-id',
    name: 'Role 2',
    normalizedName: 'Role 2 normalized name',
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
