import { Theme } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testPublicationTree: Theme[] = [
  {
    id: 'theme-id-1',
    summary: 'Theme 1 summary',
    title: 'Theme 1',
    publications: [
      {
        id: 'publication-id-1',
        slug: 'publication-slug-1',
        title: 'Publication Title 1',
        isSuperseded: false,
      },
      {
        id: 'publication-id-2',
        slug: 'publication-slug-2',
        title: 'Publication Title 2',
        isSuperseded: false,
      },
    ],
  },
  {
    id: 'theme-id-2',
    summary: 'Theme 2 summary',
    title: 'Theme 2',
    publications: [
      {
        id: 'publication-id-3',
        slug: 'publication-slug-3',
        title: 'Publication Title 3',
        isSuperseded: false,
      },
      {
        id: 'publication-id-4',
        slug: 'publication-slug-4',
        title: 'Publication Title 4',
        isSuperseded: false,
      },
    ],
  },
  {
    id: 'theme-id-3',
    summary: 'Theme 3 summary',
    title: 'Theme 3',
    publications: [],
  },
];
