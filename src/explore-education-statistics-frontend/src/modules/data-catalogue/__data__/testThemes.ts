import { Theme } from '@common/services/publicationService';

// eslint-disable-next-line import/prefer-default-export
export const testThemes: Theme[] = [
  {
    id: 'theme-1',
    title: 'Theme title 1',
    summary: '',
    topics: [
      {
        id: 'theme-1-topic-1',
        title: 'Topic 1',
        summary: '',
        publications: [
          {
            id: 'publication-1',
            title: 'Publication title 1',
            slug: 'publication-slug-1',
            isSuperseded: false,
          },
        ],
      },
    ],
  },
  {
    id: 'theme-2',
    title: 'Theme title 2',
    summary: '',
    topics: [
      {
        id: 'theme-2-topic-1',
        title: 'Topic 2',
        summary: '',
        publications: [
          {
            id: 'publication-2',
            title: 'Publication title 2',
            slug: 'publication-slug-2',
            isSuperseded: false,
          },
          {
            id: 'publication-3',
            title: 'Publication title 3',
            slug: 'publication-slug-3',
            isSuperseded: false,
          },
        ],
      },
    ],
  },
];
