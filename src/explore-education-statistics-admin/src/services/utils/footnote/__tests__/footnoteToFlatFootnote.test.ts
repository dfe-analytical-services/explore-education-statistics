import footnoteToFlatFootnote from '@admin/services/utils/footnote/footnoteToFlatFootnote';
import { BaseFootnote } from '@admin/services/footnoteService';

describe('footnoteToFlatFootnote', () => {
  test('only include the subject id if All data is selected for a subject', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'All',
          indicatorGroups: {},
          filters: {},
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: ['subject-id-1'],
      indicators: [],
      indicatorGroups: [],
      filters: [],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('only include the subject id if All data is selected for a subject when indicators/filters have previously been selected', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'All',
          indicatorGroups: {
            'indicator-group-id': {
              selected: false,
              indicators: ['indicator-id'],
            },
          },
          filters: {
            'filter-id': {
              selected: false,
              filterGroups: {
                'filter-group-id': {
                  selected: false,
                  filterItems: ['filter-item-id'],
                },
              },
            },
          },
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: ['subject-id-1'],
      indicators: [],
      indicatorGroups: [],
      filters: [],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('does not include the subject if type NA is selected', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: false,
          selectionType: 'NA',
          indicatorGroups: {
            'indicator-group-id': {
              selected: false,
              indicators: ['indicator-id'],
            },
          },

          filters: {},
        },
        'subject-id-2': {
          selected: true,
          selectionType: 'All',
          indicatorGroups: {},
          filters: {},
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: ['subject-id-2'],
      indicators: [],
      indicatorGroups: [],
      filters: [],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('includes only the filter id if all filters are selected', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'Specific',
          indicatorGroups: {},
          filters: {
            'filter-id': {
              selected: true,
              filterGroups: {
                'filter-group-id': {
                  selected: false,
                  filterItems: ['filter-item-id'],
                },
              },
            },
          },
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: [],
      indicators: [],
      indicatorGroups: [],
      filters: ['filter-id'],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('includes only filter group id if group selected', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'Specific',
          indicatorGroups: {},
          filters: {
            'filter-id': {
              selected: false,
              filterGroups: {
                'filter-group-id': {
                  selected: true,
                  filterItems: ['filter-item-id'],
                },
              },
            },
          },
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: [],
      indicators: [],
      indicatorGroups: [],
      filters: [],
      filterItems: [],
      filterGroups: ['filter-group-id'],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('includes selected filter items', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'Specific',
          indicatorGroups: {},
          filters: {
            'filter-id': {
              selected: false,
              filterGroups: {
                'filter-group-id': {
                  selected: false,
                  filterItems: ['filter-item-id-1', 'filter-item-id-2'],
                },
              },
            },
          },
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: [],
      indicators: [],
      indicatorGroups: [],
      filters: [],
      filterItems: ['filter-item-id-1', 'filter-item-id-2'],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('includes only indicator group id if group selected', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'Specific',
          indicatorGroups: {
            'indicator-group-id': {
              selected: false,
              indicators: ['indicator-id-1', 'indicator-id-2'],
            },
          },
          filters: {},
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: [],
      indicators: ['indicator-id-1', 'indicator-id-2'],
      indicatorGroups: [],
      filters: [],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });

  test('includes selected indicator items', () => {
    const footnote: BaseFootnote = {
      content: '',
      subjects: {
        'subject-id-1': {
          selected: true,
          selectionType: 'Specific',
          indicatorGroups: {
            'indicator-group-id': {
              selected: true,
              indicators: ['indicator-id'],
            },
          },
          filters: {},
        },
      },
    };

    const expectedFlatFootnote = {
      content: '',
      subjects: [],
      indicators: [],
      indicatorGroups: ['indicator-group-id'],
      filters: [],
      filterItems: [],
      filterGroups: [],
    };

    const flatFootnote = footnoteToFlatFootnote(footnote);
    expect(flatFootnote).toEqual(expectedFlatFootnote);
  });
});
