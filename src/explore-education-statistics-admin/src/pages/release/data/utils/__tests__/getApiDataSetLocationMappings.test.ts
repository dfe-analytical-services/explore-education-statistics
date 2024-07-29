import getApiDataSetLocationMappings, {
  AutoMappedLocation,
  NewLocationCandidate,
  UnmappedAndManuallyMappedLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import testLocationsMapping from '@admin/pages/release/data/__data__/testLocationsMapping';

describe('getApiDataSetLocationMappings', () => {
  test('returns the correct unmapped and manually mapped locations grouped by level', () => {
    const { unmappedAndManuallyMappedLocations } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expectedLocalAuthority: UnmappedAndManuallyMappedLocation[] = [
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
          label: 'Location 3 updated',
          code: 'location-3-code',
        },
        mapping: {
          candidateKey: 'Location3Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 3',
            code: 'location-3-code',
          },
        },
      },
      {
        mapping: {
          type: 'ManualNone',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
        },
      },
    ];

    const expectedRegion: UnmappedAndManuallyMappedLocation[] = [
      {
        candidate: { label: 'Location 9 updated', code: 'location-9-code' },
        mapping: {
          candidateKey: 'Location9Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 9',
            code: 'location-9-code',
          },
        },
      },
      {
        candidate: { label: 'Location 10 updated', code: 'location-10-code' },
        mapping: {
          candidateKey: 'Location10Key',
          type: 'ManualMapped',
          source: {
            label: 'Location 10',
            code: 'location-10-code',
          },
        },
      },
    ];

    expect(unmappedAndManuallyMappedLocations.localAuthority).toEqual(
      expectedLocalAuthority,
    );
    expect(unmappedAndManuallyMappedLocations.region).toEqual(expectedRegion);
    expect(
      unmappedAndManuallyMappedLocations.englishDevolvedArea,
    ).toBeUndefined();
  });

  test('returns the correct new locations grouped by level', () => {
    const { newLocationCandidates } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expectedLocalAuthority: NewLocationCandidate[] = [
      {
        candidate: {
          label: 'Location 6',
          code: 'location-6-code',
        },
      },
      {
        candidate: {
          label: 'Location 7',
          code: 'location-7-code',
        },
      },
    ];

    const expectedRegion: NewLocationCandidate[] = [
      {
        candidate: {
          label: 'Location 11',
          code: 'location-11-code',
        },
      },
    ];

    expect(newLocationCandidates.localAuthority).toEqual(
      expectedLocalAuthority,
    );
    expect(newLocationCandidates.region).toEqual(expectedRegion);
    expect(newLocationCandidates.englishDevolvedArea).toBeUndefined();
  });

  test('returns the correct auto mapped locations as a flat list', () => {
    const { autoMappedLocations } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expected: AutoMappedLocation[] = [
      {
        candidate: { label: 'Location 1', code: 'location-1-code' },
        mapping: {
          candidateKey: 'Location1Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
        },
      },
      {
        candidate: { label: 'Location 5', code: 'location-5-code' },
        mapping: {
          candidateKey: 'Location5Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
        },
      },
      {
        candidate: { label: 'Location 8', code: 'location-8-code' },
        mapping: {
          candidateKey: 'Location8Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 8',
            code: 'location-8-code',
          },
        },
      },
      {
        candidate: { label: 'Location 12', code: 'location-12-code' },
        mapping: {
          candidateKey: 'Location12Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 12',
            code: 'location-12-code',
          },
        },
      },
      {
        candidate: { label: 'Location 13', code: 'location-13-code' },
        mapping: {
          candidateKey: 'Location13Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 13',
            code: 'location-13-code',
          },
        },
      },
      {
        candidate: { label: 'Location 14', code: 'location-14-code' },
        mapping: {
          candidateKey: 'Location14Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 14',
            code: 'location-14-code',
          },
        },
      },
      {
        candidate: { label: 'Location 15', code: 'location-15-code' },
        mapping: {
          candidateKey: 'Location15Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 15',
            code: 'location-15-code',
          },
        },
      },
      {
        candidate: { label: 'Location 16', code: 'location-16-code' },
        mapping: {
          candidateKey: 'Location16Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 16',
            code: 'location-16-code',
          },
        },
      },
      {
        candidate: { label: 'Location 17', code: 'location-17-code' },
        mapping: {
          candidateKey: 'Location17Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 17',
            code: 'location-17-code',
          },
        },
      },
      {
        candidate: { label: 'Location 18', code: 'location-18-code' },
        mapping: {
          candidateKey: 'Location18Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 18',
            code: 'location-18-code',
          },
        },
      },
      {
        candidate: { label: 'Location 19', code: 'location-19-code' },
        mapping: {
          candidateKey: 'Location19Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 19',
            code: 'location-19-code',
          },
        },
      },
      {
        candidate: { label: 'Location 20', code: 'location-20-code' },
        mapping: {
          candidateKey: 'Location20Key',
          type: 'AutoMapped',
          source: {
            label: 'Location 20',
            code: 'location-20-code',
          },
        },
      },
    ];

    expect(autoMappedLocations).toEqual(expected);
  });
});
