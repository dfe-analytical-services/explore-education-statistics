import mergeReplacementFootnoteFilters, {
  MergedFootnoteFilterReplacement,
} from '@admin/pages/release/data/components/utils/mergeReplacementFootnoteFilters';
import { Dictionary } from '@common/types';

describe('mergeReplacementFootnoteFilters', () => {
  test('returns empty object if nothing to merge', () => {
    const result = mergeReplacementFootnoteFilters({
      id: 'footnote-1',
      content: 'Footnote 1',
      filters: [],
      filterItems: [],
      filterGroups: [],
      indicatorGroups: {},
      valid: true,
    });

    expect(result).toEqual({});
  });

  test('returns filters grouped by id', () => {
    const result = mergeReplacementFootnoteFilters({
      id: 'footnote-1',
      content: 'Footnote 1',
      filters: [
        {
          id: 'filter-2',
          label: 'Filter 2',
          valid: true,
        },
        {
          id: 'filter-3',
          label: 'Filter 3',
          valid: false,
        },
        {
          id: 'filter-1',
          label: 'Filter 1',
          valid: false,
        },
      ],
      filterGroups: [],
      filterItems: [],
      indicatorGroups: {},
      valid: true,
    });

    expect(result).toEqual<Dictionary<MergedFootnoteFilterReplacement>>({
      'filter-1': {
        id: 'filter-1',
        label: 'Filter 1',
        isAllSelected: true,
        valid: false,
        groups: {},
      },
      'filter-2': {
        id: 'filter-2',
        label: 'Filter 2',
        isAllSelected: true,
        valid: true,
        groups: {},
      },
      'filter-3': {
        id: 'filter-3',
        label: 'Filter 3',
        isAllSelected: true,
        valid: false,
        groups: {},
      },
    });
  });

  test('returns filter groups grouped by filter', () => {
    const result = mergeReplacementFootnoteFilters({
      id: 'footnote-1',
      content: 'Footnote 1',
      filters: [],
      filterGroups: [
        {
          id: 'group-1',
          label: 'Group 1',
          valid: false,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
        },
        {
          id: 'group-3',
          label: 'Group 3',
          valid: true,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
        },
        {
          id: 'group-2',
          label: 'Group 2',
          valid: true,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
        },
        {
          id: 'group-4',
          label: 'Group 4',
          valid: false,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
        },
      ],
      filterItems: [],
      indicatorGroups: {},
      valid: true,
    });

    expect(result).toEqual<Dictionary<MergedFootnoteFilterReplacement>>({
      'filter-1': {
        id: 'filter-1',
        label: 'Filter 1',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-1': {
            id: 'group-1',
            label: 'Group 1',
            valid: false,
            isAllSelected: true,
            filters: [],
          },
          'group-2': {
            id: 'group-2',
            label: 'Group 2',
            valid: true,
            isAllSelected: true,
            filters: [],
          },
        },
      },
      'filter-2': {
        id: 'filter-2',
        label: 'Filter 2',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-3': {
            id: 'group-3',
            label: 'Group 3',
            valid: true,
            isAllSelected: true,
            filters: [],
          },
          'group-4': {
            id: 'group-4',
            label: 'Group 4',
            valid: false,
            isAllSelected: true,
            filters: [],
          },
        },
      },
    });
  });

  test('returns filter items grouped by filter and filter group', () => {
    const result = mergeReplacementFootnoteFilters({
      id: 'footnote-1',
      content: 'Footnote 1',
      filters: [],
      filterGroups: [],
      filterItems: [
        {
          id: 'item-1',
          label: 'Item 1',
          valid: false,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-1',
          filterGroupLabel: 'Group 1',
        },
        {
          id: 'item-2',
          label: 'Item 2',
          valid: false,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-2',
          filterGroupLabel: 'Group 2',
        },
        {
          id: 'item-3',
          label: 'Item 3',
          valid: true,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-1',
          filterGroupLabel: 'Group 1',
        },
        {
          id: 'item-4',
          label: 'Item 4',
          valid: false,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
          filterGroupId: 'group-3',
          filterGroupLabel: 'Group 3',
        },
        {
          id: 'item-5',
          label: 'Item 5',
          valid: false,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
          filterGroupId: 'group-4',
          filterGroupLabel: 'Group 4',
        },
      ],
      indicatorGroups: {},
      valid: true,
    });

    expect(result).toEqual<Dictionary<MergedFootnoteFilterReplacement>>({
      'filter-1': {
        id: 'filter-1',
        label: 'Filter 1',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-1': {
            id: 'group-1',
            label: 'Group 1',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-1',
                label: 'Item 1',
                valid: false,
              },
              {
                id: 'item-3',
                label: 'Item 3',
                valid: true,
              },
            ],
          },
          'group-2': {
            id: 'group-2',
            label: 'Group 2',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-2',
                label: 'Item 2',
                valid: false,
              },
            ],
          },
        },
      },
      'filter-2': {
        id: 'filter-2',
        label: 'Filter 2',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-3': {
            id: 'group-3',
            label: 'Group 3',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-4',
                label: 'Item 4',
                valid: false,
              },
            ],
          },
          'group-4': {
            id: 'group-4',
            label: 'Group 4',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-5',
                label: 'Item 5',
                valid: false,
              },
            ],
          },
        },
      },
    });
  });

  test('returns filter, filter groups and filter items grouped correctly', () => {
    const result = mergeReplacementFootnoteFilters({
      id: 'footnote-1',
      content: 'Footnote 1',
      valid: false,
      filters: [
        {
          id: 'filter-3',
          label: 'Filter 3',
          valid: true,
        },
        {
          id: 'filter-4',
          label: 'Filter 4',
          valid: false,
        },
      ],
      filterGroups: [
        {
          id: 'group-1',
          label: 'Group 1',
          valid: true,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
        },
        {
          id: 'group-4',
          label: 'Group 4',
          valid: false,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
        },
      ],
      filterItems: [
        {
          id: 'item-1',
          label: 'Item 1',
          valid: false,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-2',
          filterGroupLabel: 'Group 2',
        },
        {
          id: 'item-2',
          label: 'Item 2',
          valid: true,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-3',
          filterGroupLabel: 'Group 3',
        },
        {
          id: 'item-3',
          label: 'Item 3',
          valid: false,
          filterId: 'filter-1',
          filterLabel: 'Filter 1',
          filterGroupId: 'group-2',
          filterGroupLabel: 'Group 2',
        },
        {
          id: 'item-4',
          label: 'Item 4',
          valid: false,
          filterId: 'filter-2',
          filterLabel: 'Filter 2',
          filterGroupId: 'group-5',
          filterGroupLabel: 'Group 5',
        },
      ],
      indicatorGroups: {},
    });

    expect(result).toEqual<Dictionary<MergedFootnoteFilterReplacement>>({
      'filter-1': {
        id: 'filter-1',
        label: 'Filter 1',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-1': {
            id: 'group-1',
            label: 'Group 1',
            valid: true,
            isAllSelected: true,
            filters: [],
          },
          'group-2': {
            id: 'group-2',
            label: 'Group 2',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-1',
                label: 'Item 1',
                valid: false,
              },
              {
                id: 'item-3',
                label: 'Item 3',
                valid: false,
              },
            ],
          },
          'group-3': {
            id: 'group-3',
            label: 'Group 3',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-2',
                label: 'Item 2',
                valid: true,
              },
            ],
          },
        },
      },
      'filter-2': {
        id: 'filter-2',
        label: 'Filter 2',
        isAllSelected: false,
        valid: true,
        groups: {
          'group-4': {
            id: 'group-4',
            label: 'Group 4',
            valid: false,
            isAllSelected: true,
            filters: [],
          },
          'group-5': {
            id: 'group-5',
            label: 'Group 5',
            valid: true,
            isAllSelected: false,
            filters: [
              {
                id: 'item-4',
                label: 'Item 4',
                valid: false,
              },
            ],
          },
        },
      },
      'filter-3': {
        id: 'filter-3',
        label: 'Filter 3',
        isAllSelected: true,
        valid: true,
        groups: {},
      },
      'filter-4': {
        id: 'filter-4',
        label: 'Filter 4',
        isAllSelected: true,
        valid: false,
        groups: {},
      },
    });
  });
});
