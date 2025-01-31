import { ReleaseSeriesTableEntry } from '@admin/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testReleaseSeries: ReleaseSeriesTableEntry[] = [
  {
    id: 'ees-release-3',
    isLegacyLink: false,
    description: 'EES release 3',
    releaseId: 'release-id',
    releaseSlug: '3',
    isLatest: false,
    isPublished: false,
  },
  {
    id: 'ees-release-2',
    isLegacyLink: false,
    description: 'EES release 2',
    releaseId: 'release-id',
    releaseSlug: '2',
    isLatest: true,
    isPublished: true,
  },
  {
    id: 'ees-release-1',
    isLegacyLink: false,
    description: 'EES release 1',
    releaseId: 'release-id',
    releaseSlug: '1',
    isLatest: false,
    isPublished: true,
  },
  {
    id: 'legacy-release-1',
    isLegacyLink: true,
    description: 'Legacy link 1',
    legacyLinkUrl: 'http://gov.uk/1',
    releaseSlug: 'latest-release-slug',
  },
];
