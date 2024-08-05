import getUnmappedLocationErrors from '@admin/pages/release/data/utils/getUnmappedLocationErrors';
import { MappableLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { Dictionary } from '@common/types';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';

describe('getUnmappedLocationTotalsByLevel', () => {
  const testMappableLocations: Dictionary<MappableLocation[]> = {
    localAuthority: [
      {
        mapping: {
          type: 'ManualNone',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
          sourceKey: 'Location1Key',
          publicId: 'location-1-public-id',
        },
      },
      {
        mapping: {
          type: 'AutoNone',
          source: {
            label: 'Location 2',
            code: 'location-2-code',
          },
          sourceKey: 'Location2Key',
          publicId: 'location-2-public-id',
        },
      },
      {
        candidate: {
          key: 'Location3Key',
          label: 'Location 3',
          code: 'location-3-code',
        },
        mapping: {
          candidateKey: 'Location3Key',
          type: 'AutoNone',
          source: {
            label: 'Location 3',
            code: 'location-3-code',
          },
          sourceKey: 'Location3Key',
          publicId: 'location-3-public-id',
        },
      },
    ],
    region: [
      {
        candidate: {
          key: 'Location4Key',
          label: 'Location 4',
          code: 'location-4-code',
        },
        mapping: {
          candidateKey: 'Location4Key',
          type: 'AutoNone',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
          sourceKey: 'Location4Key',
          publicId: 'location-4-public-id',
        },
      },
      {
        candidate: {
          key: 'Location5Key',
          label: 'Location 5 updated',
          code: 'location-5-code',
        },
        mapping: {
          candidateKey: 'Location5Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
          sourceKey: 'Location5Key',
          publicId: 'location-5-public-id',
        },
      },
    ],
    englishDevolvedArea: [
      {
        candidate: {
          key: 'Location6Key',
          label: 'Location 6 updated',
          code: 'location-6-code',
        },
        mapping: {
          candidateKey: 'Location6Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 6',
            code: 'location-6-code',
          },
          sourceKey: 'Location6Key',
          publicId: 'location-6-public-id',
        },
      },
    ],
  };

  test('it returns the error messages', () => {
    const expected: ErrorSummaryMessage[] = [
      {
        id: 'mappable-localAuthority',
        message: 'There are 2 unmapped local authorities',
      },
      { id: 'mappable-region', message: 'There is 1 unmapped region' },
    ];

    expect(getUnmappedLocationErrors(testMappableLocations)).toEqual(expected);
  });
});
