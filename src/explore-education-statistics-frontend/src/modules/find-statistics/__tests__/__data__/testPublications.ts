import { PublicationSummaryWithRelease } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testPublications: PublicationSummaryWithRelease[] = [
  {
    id: '1',
    latestRelease: {
      id: 'release-1',
      published: '2021-06-08T00:00:00',
      type: 'AdHocStatistics',
    },
    slug: 'publication-1-slug',
    summary: 'Publication 1 summary',
    theme: {
      title: 'Theme 1',
    },
    title: 'Publication 1',
  },
  {
    id: '2',
    latestRelease: {
      id: 'release-1',
      published: '2022-01-01T00:00:00',
      type: 'ExperimentalStatistics',
    },
    slug: 'publication-2-slug',
    summary: 'Publication 2 summary',
    theme: {
      title: 'Theme 2',
    },
    title: 'Publication 2',
  },
  {
    id: '3',
    legacyPublicationUrl: 'http://test.com',
    slug: 'publication-3-slug',
    title: 'Publication 3',
    theme: {
      title: 'Theme 2',
    },
  },
  {
    id: '4',
    latestRelease: {
      id: 'release-1',
      published: '2021-08-08T00:00:00',
      type: 'NationalStatistics',
    },
    slug: 'publication-4-slug',
    summary: 'Publication 4 summary',
    theme: {
      title: 'Theme 3',
    },
    title: 'Publication 4',
  },
];
