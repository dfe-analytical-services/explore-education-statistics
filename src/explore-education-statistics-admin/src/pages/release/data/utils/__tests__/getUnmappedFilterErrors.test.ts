import getUnmappedFilterErrors from '@admin/pages/release/data/utils/getUnmappedFilterErrors';
import { MappableFilterOption } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import { Dictionary } from '@common/types';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import testFiltersMapping from '@admin/pages/release/data/__data__/testFiltersMapping';

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
        id: 'mappable-Filter1Key',
        message: 'There are 2 unmapped Filter 1 filter options',
      },
      {
        id: 'mappable-Filter2Key',
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
        id: 'mappable-Filter1Key',
        message: 'There are 2 unmapped Filter 1 filter options',
      },
      {
        id: 'mappable-Filter2Key',
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
});
