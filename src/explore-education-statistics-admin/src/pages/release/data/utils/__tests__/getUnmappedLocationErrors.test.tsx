import getUnmappedLocationErrors from '@admin/pages/release/data/utils/getUnmappedLocationErrors';
import { UnmappedAndManuallyMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { Dictionary } from '@common/types';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';

describe('getUnmappedLocationTotalsByLevel', () => {
  const testUnmappedAndManuallyMappedLocations: Dictionary<
    UnmappedAndManuallyMappedLocation[]
  > = {
    localAuthority: [
      {
        mapping: {
          type: 'ManualNone',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
        },
      },
      {
        mapping: {
          type: 'AutoNone',
          source: {
            label: 'Location 2',
            code: 'location-2-code',
          },
        },
      },
      {
        candidate: {
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
        },
      },
    ],
    region: [
      {
        candidate: { label: 'Location 4', code: 'location-4-code' },
        mapping: {
          candidateKey: 'Location4Key',
          type: 'AutoNone',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
        },
      },
      {
        candidate: { label: 'Location 5 updated', code: 'location-5-code' },
        mapping: {
          candidateKey: 'Location5Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
        },
      },
    ],
    englishDevolvedArea: [
      {
        candidate: { label: 'Location 6 updated', code: 'location-6-code' },
        mapping: {
          candidateKey: 'Location6Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 6',
            code: 'location-6-code',
          },
        },
      },
    ],
  };

  test('it returns the error messages', () => {
    const expected: ErrorSummaryMessage[] = [
      {
        id: 'unmapped-localAuthority',
        message: 'There are 2 unmapped local authorities',
      },
      { id: 'unmapped-region', message: 'There is 1 unmapped region' },
    ];

    expect(
      getUnmappedLocationErrors(testUnmappedAndManuallyMappedLocations),
    ).toEqual(expected);
  });
});
