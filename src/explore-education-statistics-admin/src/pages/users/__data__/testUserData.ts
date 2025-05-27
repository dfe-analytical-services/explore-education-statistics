import { PublicationSummary } from '@common/services/publicationService';
import { IdTitlePair } from '@admin/services/types/common';
import { Role, ResourceRoles, User } from '@admin/services/userService';

export const testUser: User = {
  id: 'user-1-id',
  name: 'Florian Schneider',
  email: 'test@test.com',
  role: 'role-guid-1',
  userPublicationRoles: [
    {
      id: 'pr-id-1',
      publication: 'Publication 1',
      role: 'Approver',
      userName: 'Analyst1 User1',
      email: 'analyst1@example.com',
    },
    {
      id: 'pr-id-2',
      publication: 'Publication 2',
      role: 'Owner',
      userName: 'Analyst2 User2',
      email: 'analyst2@example.com',
    },
  ],
  userReleaseRoles: [
    {
      id: 'rr-id-1',
      publication: 'Publication 1',
      release: 'Release 2',
      role: 'Contributor',
    },
    {
      id: 'rr-id-2',
      publication: 'Publication 2',
      release: 'Release 2',
      role: 'Approver',
    },
  ],
};

export const testPublicationSummaries: PublicationSummary[] = [
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

export const testResourceRoles: ResourceRoles = {
  Publication: ['Approver', 'Owner'],
  Release: ['Approver', 'Contributor', 'PrereleaseViewer'],
};

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
