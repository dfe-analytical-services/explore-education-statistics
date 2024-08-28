import testFiltersMapping from '@admin/pages/release/data/__data__/testFiltersMapping';
import { MappableFilterOption } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import getUnmappedFilterErrors from '@admin/pages/release/data/utils/getUnmappedFilterErrors';
import { FiltersMapping } from '@admin/services/apiDataSetVersionService';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import { Dictionary } from '@common/types';

describe('getUnmappedFilterErrors', () => {
  const testMappableFilters: Dictionary<MappableFilterOption[]> = {
    Filter1Key: [
      {
        mapping: {
          type: 'AutoNone',
          source: {
            label: 'Filter 1 Option 2',
          },
          sourceKey: 'Filter1Option2Key',
          publicId: 'filter-1-option-2-public-id',
        },
      },
      {
        mapping: {
          type: 'AutoNone',
          source: {
            label: 'Filter 1 Option 3',
          },
          sourceKey: 'Filter1Option3Key',
          publicId: 'filter-1-option-3-public-id',
        },
      },
      {
        candidate: {
          key: 'Filter1Option4UpdatedKey',
          label: 'Filter 1 Option 4 updated',
        },
        mapping: {
          candidateKey: 'Filter1Option4UpdatedKey',
          type: 'ManualMapped',
          source: {
            label: 'Filter 1 Option 4',
          },
          sourceKey: 'Filter1Option4Key',
          publicId: 'filter-1-option-4-public-id',
        },
      },
    ],
    Filter2Key: [
      {
        candidate: {
          key: 'Filter2Option1UpdatedKey',
          label: 'Filter 2 Option 1 updated',
        },
        mapping: {
          candidateKey: 'Filter2Option1UpdatedKey',
          type: 'ManualMapped',
          source: {
            label: 'Filter 2 Option 1',
          },
          sourceKey: 'Filter2Option1Key',
          publicId: 'filter-2-option-1-public-id',
        },
      },
      {
        mapping: {
          type: 'ManualNone',
          source: {
            label: 'Filter 2 Option 2',
          },
          sourceKey: 'Filter2Option2Key',
          publicId: 'filter-2-option-2-public-id',
        },
      },
      {
        mapping: {
          type: 'AutoNone',
          source: {
            label: 'Filter 2 Option 3',
          },
          sourceKey: 'Filter2Option3Key',
          publicId: 'filter-2-option-3-public-id',
        },
      },
    ],
  };

  test('returns the error messages', () => {
    const expected: ErrorSummaryMessage[] = [
      {
        id: 'mappable-table-filter-1-key',
        message: 'There are 2 unmapped Filter 1 filter options',
      },
      {
        id: 'mappable-table-filter-2-key',
        message: 'There is 1 unmapped Filter 2 filter option',
      },
    ];

    expect(
      getUnmappedFilterErrors(testMappableFilters, testFiltersMapping),
    ).toEqual(expected);
  });

  test('does not return error messages for unmapped filter columns', () => {
    const expected: ErrorSummaryMessage[] = [
      {
        id: 'mappable-table-filter-1-key',
        message: 'There are 2 unmapped Filter 1 filter options',
      },
      {
        id: 'mappable-table-filter-2-key',
        message: 'There is 1 unmapped Filter 2 filter option',
      },
    ];

    expect(
      getUnmappedFilterErrors(
        {
          ...testMappableFilters,
          UnMappedFilterKey: [
            {
              mapping: {
                type: 'AutoNone',
                source: {
                  label: 'Unmapped filter option',
                },
                sourceKey: 'UnMappedFilterOptionKey',
                publicId: 'unmapped-filter-option-public-id',
              },
            },
          ],
        },
        testFiltersMapping,
      ),
    ).toEqual(expected);
  });

  test('returns errors with URL safe IDs', () => {
    const testUnsafeMappableFilters: Dictionary<MappableFilterOption[]> = {
      'Filter 1': [
        {
          mapping: {
            type: 'AutoNone',
            source: {
              label: 'Filter 1 Option 1',
            },
            sourceKey: 'Filter1Option1Key',
            publicId: 'filter-1-option-1-public-id',
          },
        },
      ],
      'filter_2_%+,': [
        {
          mapping: {
            type: 'AutoNone',
            source: {
              label: 'Filter 2 Option 1',
            },
            sourceKey: 'Filter2Option1Key',
            publicId: 'filter-2-option-1-public-id',
          },
        },
      ],
    };

    const testUnsafeMappings: FiltersMapping = {
      candidates: {
        'Filter 1': {
          label: 'Filter 1',
          options: {
            Filter1Option1Key: {
              label: 'Filter 1 Option 1',
            },
          },
        },
        'filter_2_%+,': {
          label: 'Filter 2',
          options: {
            Filter2Option1Key: {
              label: 'Filter 2 Option 1',
            },
          },
        },
      },
      mappings: {
        'Filter 1': {
          type: 'AutoMapped',
          source: {
            label: 'Filter 1',
          },
          publicId: 'filter-1-public-id',
          optionMappings: {
            Filter1Option1Key: {
              type: 'AutoNone',
              source: {
                label: 'Filter 1 Option 1',
              },
              publicId: 'filter-1-option-1-public-id',
            },
          },
        },
        'filter_2_%+,': {
          type: 'AutoMapped',
          source: {
            label: 'Filter 2',
          },
          publicId: 'filter-2-public-id',
          optionMappings: {
            Filter2Option1Key: {
              type: 'AutoNone',
              source: {
                label: 'Filter 2 Option 1',
              },
              publicId: 'filter-2-option-1-public-id',
            },
          },
        },
      },
    };

    expect(
      getUnmappedFilterErrors(testUnsafeMappableFilters, testUnsafeMappings),
    ).toEqual<ErrorSummaryMessage[]>([
      {
        id: 'mappable-table-filter-1',
        message: 'There is 1 unmapped Filter 1 filter option',
      },
      {
        id: 'mappable-table-filter-2',
        message: 'There is 1 unmapped Filter 2 filter option',
      },
    ]);
  });
});
