import getApiDataSetFilterMappings, {
  AutoMappedFilter,
  FilterCandidateWithKey,
  MappableFilter,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import testFiltersMapping, {
  testFiltersMappingUnmappedColumns,
} from '@admin/pages/release/data/__data__/testFiltersMapping';
import {
  FilterCandidate,
  FilterMapping,
} from '@admin/services/apiDataSetVersionService';

describe('getApiDataSetFilterMappings', () => {
  test('returns the correct mappable filter options grouped by filter column', () => {
    const { mappableFilterOptions } =
      getApiDataSetFilterMappings(testFiltersMapping);

    const expectedFilter1: MappableFilter[] = [
      {
        mapping: {
          publicId: 'filter-1-option-2-public-id',
          source: { label: 'Filter 1 Option 2' },
          sourceKey: 'Filter1Option2Key',
          type: 'AutoNone',
        },
      },
      {
        mapping: {
          publicId: 'filter-1-option-3-public-id',
          source: { label: 'Filter 1 Option 3' },
          sourceKey: 'Filter1Option3Key',
          type: 'AutoNone',
        },
      },
      {
        candidate: {
          key: 'Filter1Option4UpdatedKey',
          label: 'Filter 1 Option 4 updated',
        },
        mapping: {
          candidateKey: 'Filter1Option4UpdatedKey',
          publicId: 'filter-1-option-4-public-id',
          source: { label: 'Filter 1 Option 4' },
          sourceKey: 'Filter1Option4Key',
          type: 'ManualMapped',
        },
      },
    ];

    const expectedFilter2: MappableFilter[] = [
      {
        candidate: {
          key: 'Filter2Option1UpdatedKey',
          label: 'Filter 2 Option 1 updated',
        },
        mapping: {
          candidateKey: 'Filter2Option1UpdatedKey',
          publicId: 'filter-2-option-1-public-id',
          source: { label: 'Filter 2 Option 1' },
          sourceKey: 'Filter2Option1Key',
          type: 'ManualMapped',
        },
      },
      {
        mapping: {
          publicId: 'filter-2-option-2-public-id',
          source: { label: 'Filter 2 Option 2' },
          sourceKey: 'Filter2Option2Key',
          type: 'ManualNone',
        },
      },
      {
        mapping: {
          publicId: 'filter-2-option-3-public-id',
          source: { label: 'Filter 2 Option 3' },
          sourceKey: 'Filter2Option3Key',
          type: 'AutoNone',
        },
      },
    ];

    expect(mappableFilterOptions.Filter1Key).toEqual(expectedFilter1);
    expect(mappableFilterOptions.Filter2Key).toEqual(expectedFilter2);
    expect(mappableFilterOptions.Filter3Key).toBeUndefined();
  });

  test('returns the correct new locations grouped by filter column', () => {
    const { newFilterOptions } =
      getApiDataSetFilterMappings(testFiltersMapping);

    const expectedFilter1: FilterCandidateWithKey[] = [
      {
        key: 'Filter1Option2UpdatedKey',
        label: 'Filter 1 Option 2 updated',
      },
    ];

    expect(newFilterOptions.Filter1Key).toEqual(expectedFilter1);
    expect(newFilterOptions.Filter2Key).toBeUndefined();
    expect(newFilterOptions.Filter3Key).toBeUndefined();
  });

  test('returns the correct auto mapped locations grouped by filter column', () => {
    const { autoMappedFilterOptions } =
      getApiDataSetFilterMappings(testFiltersMapping);

    const expectedFilter1: AutoMappedFilter[] = [
      {
        candidate: {
          key: 'Filter1Option1Key',
          label: 'Filter 1 Option 1',
        },
        mapping: {
          candidateKey: 'Filter1Option1Key',
          publicId: 'filter-1-option-1-public-id',
          source: { label: 'Filter 1 Option 1' },
          sourceKey: 'Filter1Option1Key',
          type: 'AutoMapped',
        },
      },
    ];

    const expectedFilter3: AutoMappedFilter[] = [
      {
        candidate: {
          key: 'Filter3Option1Key',
          label: 'Filter 3 Option 1',
        },
        mapping: {
          candidateKey: 'Filter3Option1Key',
          publicId: 'filter-3-option-1-public-id',
          source: {
            label: 'Filter 3 Option 1',
          },
          sourceKey: 'Filter3Option1Key',
          type: 'AutoMapped',
        },
      },
      {
        candidate: {
          key: 'Filter3Option2Key',
          label: 'Filter 3 Option 2',
        },
        mapping: {
          candidateKey: 'Filter3Option2Key',
          publicId: 'filter-3-option-2-public-id',
          source: {
            label: 'Filter 3 Option 2',
          },
          sourceKey: 'Filter3Option2Key',
          type: 'AutoMapped',
        },
      },
      {
        candidate: {
          key: 'Filter3Option3Key',
          label: 'Filter 3 Option 3',
        },
        mapping: {
          candidateKey: 'Filter3Option3Key',
          publicId: 'filter-3-option-3-public-id',
          source: {
            label: 'Filter 3 Option 3',
          },
          sourceKey: 'Filter3Option3Key',
          type: 'AutoMapped',
        },
      },
    ];

    expect(autoMappedFilterOptions.Filter1Key).toEqual(expectedFilter1);
    expect(autoMappedFilterOptions.Filter2Key).toBeUndefined();
    expect(autoMappedFilterOptions.Filter3Key).toEqual(expectedFilter3);
  });

  test('returns the correct mappable filter columns', () => {
    const { mappableFilterColumns } = getApiDataSetFilterMappings(
      testFiltersMappingUnmappedColumns,
    );

    const expectedFilter1: FilterMapping = {
      optionMappings: {
        Filter1Option1Key: {
          publicId: 'filter-1-option-1-public-id',
          source: { label: 'Filter 1 Option 1' },
          type: 'AutoNone',
        },
        Filter1Option2Key: {
          publicId: 'filter-1-option-2-public-id',
          source: { label: 'Filter 1 Option 2' },
          type: 'AutoNone',
        },
        Filter1Option3Key: {
          publicId: 'filter-1-option-3-public-id',
          source: { label: 'Filter 1 Option 3' },
          type: 'AutoNone',
        },
        Filter1Option4Key: {
          publicId: 'filter-1-option-4-public-id',
          source: { label: 'Filter 1 Option 4' },
          type: 'AutoNone',
        },
      },
      publicId: 'filter-1-public-id',
      source: {
        label: 'Filter 1',
      },
      type: 'AutoNone',
    };

    expect(mappableFilterColumns.Filter1Key).toEqual(expectedFilter1);
    expect(mappableFilterColumns.Filter2Key).toBeUndefined();
  });

  test('returns the correct new filter columns', () => {
    const { newFilterColumns } = getApiDataSetFilterMappings(
      testFiltersMappingUnmappedColumns,
    );

    const expectedFilter1: FilterCandidate = {
      label: 'Filter 1',
      options: {
        Filter1Option1Key: {
          label: 'Filter 1 Option 1',
        },
        Filter1Option2Key: {
          label: 'Filter 1 Option 2',
        },
        Filter1Option3Key: {
          label: 'Filter 1 Option 3',
        },
        Filter1Option4Key: {
          label: 'Filter 1 Option 4',
        },
      },
    };

    expect(newFilterColumns.Filter1UpdatedKey).toEqual(expectedFilter1);
    expect(newFilterColumns.Filter1Key).toBeUndefined();
    expect(newFilterColumns.Filter2Key).toBeUndefined();
  });
});
