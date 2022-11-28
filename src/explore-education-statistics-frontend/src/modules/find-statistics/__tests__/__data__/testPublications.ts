import { PublicationListSummary } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testPublications: PublicationListSummary[] = [
  {
    id: '1',
    published: new Date('2021-06-08T00:00:00'),
    rank: 0,
    type: 'AdHocStatistics',
    slug: 'publication-1-slug',
    summary: 'Publication 1 summary',
    theme: 'Theme 1',
    title: 'Publication 1',
  },
  {
    id: '2',
    published: new Date('2022-01-01T00:00:00'),
    rank: 0,
    type: 'ExperimentalStatistics',
    slug: 'publication-2-slug',
    summary: 'Publication 2 summary',
    theme: 'Theme 2',
    title: 'Publication 2',
  },

  {
    id: '3',
    published: new Date('2021-08-08T00:00:00'),
    rank: 0,
    type: 'NationalStatistics',
    slug: 'publication-3-slug',
    summary: 'Publication 3 summary',
    theme: 'Theme 3',
    title: 'Publication 3',
  },
];
