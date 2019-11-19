import { Footnote } from '@admin/services/release/edit-release/footnotes/types';

export const dummyFootnotes: Footnote[] = [
  {
    id: '1',
    content: 'State-funded primary schools',
    subjects: {
      '1': {
        selected: false,
        indicatorGroups: {
          '1': {
            selected: false,
            indicators: ['1'],
          },
          '2': {
            selected: true,
            indicators: [],
          },
          '3': {
            selected: false,
            indicators: ['16', '3'],
          },
        },
        filters: {
          '1': {
            selected: false,
            filterGroups: {
              '2': {
                selected: false,
                filterItems: ['25'],
              },
            },
          },
          '2': {
            selected: false,
            filterGroups: {
              '11': {
                selected: false,
                filterItems: ['56', '57', '58'],
              },
            },
          },
          '3': {
            selected: true,
            filterGroups: {},
          },
        },
      },
    },
  },
  {
    id: '2',
    content: 'selectedGroups',
    subjects: {
      '1': {
        selected: false,
        indicatorGroups: {
          '1': {
            selected: true,
            indicators: [],
          },
        },
        filters: {
          '1': {
            selected: false,
            filterGroups: {
              '2': {
                selected: true,
                filterItems: [],
              },
            },
          },
        },
      },
    },
  },
  {
    id: '3',
    content: 'selectedFilter',
    subjects: {
      '1': {
        selected: false,
        indicatorGroups: {
          '1': {
            selected: false,
            indicators: ['1'],
          },
        },
        filters: {
          '1': {
            selected: true,
            filterGroups: {},
          },
        },
      },
    },
  },
  {
    id: '4',
    content: 'selectedSubject',
    subjects: {
      '1': {
        selected: true,
        indicatorGroups: {},
        filters: {},
      },
    },
  },
];
export default dummyFootnotes;
