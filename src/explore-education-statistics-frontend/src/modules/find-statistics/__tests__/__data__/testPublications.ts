import { PublicationSummaryWithRelease } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testPublications: PublicationSummaryWithRelease[] = [
  {
    id: '1',
    release: {
      id: 'release-1',
      published: '2021-06-08T00:00:00',
      theme: {
        title: 'Theme 1',
      },
      type: 'AdHocStatistics',
    },
    slug: 'publication-1-slug',
    summary: 'Publication 1 summary',
    title: 'Publication 1',
  },
  {
    id: '2',
    release: {
      id: 'release-1',
      published: '2022-01-01T00:00:00',
      theme: {
        title: 'Theme 2',
      },
      type: 'ExperimentalStatistics',
    },
    slug: 'publication-2-slug',
    summary: 'Publication 2 summary',
    title: 'Publication 2',
  },
  {
    id: '3',
    legacyPublicationUrl: 'http://test.com',
    slug: 'publication-3-slug',
    title: 'Publication 3',
  },
  {
    id: '4',
    release: {
      id: 'release-1',
      published: '2021-08-08T00:00:00',
      theme: {
        title: 'Theme 3',
      },
      type: 'NationalStatistics',
    },
    slug: 'publication-4-slug',
    summary: 'Publication 4 summary',
    title: 'Publication 4',
  },
];
