import getApiDataSetLocationMappings, {
  AutoMappedLocation,
  LocationCandidateWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import testLocationsMapping from '@admin/pages/release/data/__data__/testLocationsMapping';

describe('getApiDataSetLocationMappings', () => {
  test('returns the correct mappable locations grouped by level', () => {
    const { mappableLocations } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expectedLocalAuthority: MappableLocation[] = [
      {
        mapping: {
          publicId: 'location-2-public-id',
          source: {
            label: 'Location 2',
            code: 'location-2-code',
          },
          sourceKey: 'Location2Key',
          type: 'AutoNone',
        },
      },
      {
        candidate: {
          key: 'Location3UpdatedKey',
          code: 'location-3-code-updated',
          label: 'Location 3 updated',
        },
        mapping: {
          candidateKey: 'Location3UpdatedKey',
          publicId: 'location-3-public-id',
          source: {
            label: 'Location 3',
            code: 'location-3-code',
          },
          sourceKey: 'Location3Key',
          type: 'ManualMapped',
        },
      },
      {
        mapping: {
          publicId: 'location-4-public-id',
          source: {
            label: 'Location 4',
            code: 'location-4-code',
          },
          sourceKey: 'Location4Key',
          type: 'ManualNone',
        },
      },
    ];

    const expectedRegion: MappableLocation[] = [
      {
        candidate: {
          key: 'Location9UpdatedKey',
          code: 'location-9-code',
          label: 'Location 9 updated',
        },
        mapping: {
          candidateKey: 'Location9UpdatedKey',
          publicId: 'location-9-public-id',
          source: {
            label: 'Location 9',
            code: 'location-9-code',
          },
          sourceKey: 'Location9Key',
          type: 'ManualMapped',
        },
      },
      {
        candidate: {
          key: 'Location10UpdatedKey',
          code: 'location-10-code',
          label: 'Location 10 updated',
        },
        mapping: {
          candidateKey: 'Location10UpdatedKey',
          publicId: 'location-10-public-id',
          source: {
            label: 'Location 10',
            code: 'location-10-code',
          },
          sourceKey: 'Location10Key',
          type: 'ManualMapped',
        },
      },
      {
        mapping: {
          publicId: 'location-12-public-id',
          source: {
            label: 'Location 12',
            code: 'location-12-code',
          },
          sourceKey: 'Location12Key',
          type: 'ManualNone',
        },
      },
    ];

    expect(mappableLocations.localAuthority).toEqual(expectedLocalAuthority);
    expect(mappableLocations.region).toEqual(expectedRegion);
    expect(mappableLocations.englishDevolvedArea).toBeUndefined();
  });

  test('returns the correct new locations grouped by level', () => {
    const { newLocations } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expectedLocalAuthority: LocationCandidateWithKey[] = [
      {
        key: 'Location6Key',
        code: 'location-6-code',
        label: 'Location 6',
      },
      {
        key: 'Location7Key',
        code: 'location-7-code',
        label: 'Location 7',
      },
    ];

    const expectedRegion: LocationCandidateWithKey[] = [
      {
        key: 'Location11Key',
        code: 'location-11-code',
        label: 'Location 11',
      },
      {
        key: 'Location12Key',
        code: 'location-12-code',
        label: 'Location 12',
      },
    ];

    expect(newLocations.localAuthority).toEqual(expectedLocalAuthority);
    expect(newLocations.region).toEqual(expectedRegion);
    expect(newLocations.englishDevolvedArea).toBeUndefined();
  });

  test('returns the correct auto mapped locations grouped by level', () => {
    const { autoMappedLocations } =
      getApiDataSetLocationMappings(testLocationsMapping);

    const expectedLocalAuthority: AutoMappedLocation[] = [
      {
        candidate: {
          key: 'Location1Key',
          code: 'location-1-code',
          label: 'Location 1',
        },
        mapping: {
          candidateKey: 'Location1Key',
          publicId: 'location-1-public-id',
          source: {
            label: 'Location 1',
            code: 'location-1-code',
          },
          sourceKey: 'Location1Key',
          type: 'AutoMapped',
        },
      },
      {
        candidate: {
          key: 'Location5Key',
          code: 'location-5-code',
          label: 'Location 5',
        },
        mapping: {
          candidateKey: 'Location5Key',
          publicId: 'location-5-public-id',
          source: {
            label: 'Location 5',
            code: 'location-5-code',
          },
          sourceKey: 'Location5Key',
          type: 'AutoMapped',
        },
      },
    ];

    const expectedRegion: AutoMappedLocation[] = [
      {
        candidate: {
          key: 'Location8Key',
          code: 'location-8-code',
          label: 'Location 8',
        },
        mapping: {
          candidateKey: 'Location8Key',
          publicId: 'location-8-public-id',
          source: {
            label: 'Location 8',
            code: 'location-8-code',
          },
          sourceKey: 'Location8Key',
          type: 'AutoMapped',
        },
      },
    ];

    const expectedEnglishDevolvedArea: AutoMappedLocation[] = [
      {
        candidate: {
          key: 'Location13Key',
          code: 'location-13-code',
          label: 'Location 13',
        },
        mapping: {
          candidateKey: 'Location13Key',
          publicId: 'location-13-public-id',
          source: {
            label: 'Location 13',
            code: 'location-13-code',
          },
          sourceKey: 'Location13Key',
          type: 'AutoMapped',
        },
      },
      {
        candidate: {
          key: 'Location14Key',
          code: 'location-14-code',
          label: 'Location 14',
        },
        mapping: {
          candidateKey: 'Location14Key',
          publicId: 'location-14-public-id',
          source: {
            label: 'Location 14',
            code: 'location-14-code',
          },
          sourceKey: 'Location14Key',
          type: 'AutoMapped',
        },
      },
      {
        candidate: {
          key: 'Location15Key',
          code: 'location-15-code',
          label: 'Location 15',
        },
        mapping: {
          candidateKey: 'Location15Key',
          publicId: 'location-15-public-id',
          source: {
            label: 'Location 15',
            code: 'location-15-code',
          },
          sourceKey: 'Location15Key',
          type: 'AutoMapped',
        },
      },
    ];

    expect(autoMappedLocations.localAuthority).toEqual(expectedLocalAuthority);
    expect(autoMappedLocations.region).toEqual(expectedRegion);
    expect(autoMappedLocations.englishDevolvedArea).toEqual(
      expectedEnglishDevolvedArea,
    );
  });
});
