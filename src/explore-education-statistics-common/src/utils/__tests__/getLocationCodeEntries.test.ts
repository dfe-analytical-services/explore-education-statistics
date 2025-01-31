import getLocationCodeEntries, {
  LocationCodeEntry,
} from '@common/utils/getLocationCodeEntries';

describe('getLocationCodeEntries', () => {
  test('returns correct entries when `code` and `oldCode` are present', () => {
    expect(
      getLocationCodeEntries({
        code: 'test-code',
        oldCode: 'test-old-code',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'code', label: 'Code', value: 'test-code' },
      { key: 'oldCode', label: 'Old code', value: 'test-old-code' },
    ]);
  });

  test('returns correct entries when `code` and `id` are present', () => {
    expect(
      getLocationCodeEntries({
        id: 'test-id',
        code: 'test-code',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'id', label: 'ID', value: 'test-id' },
      { key: 'code', label: 'Code', value: 'test-code' },
    ]);
  });

  test('returns correct entries when `urn` and `laEstab` are present', () => {
    expect(
      getLocationCodeEntries({
        urn: 'test-urn',
        laEstab: 'test-laEstab',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'urn', label: 'URN', value: 'test-urn' },
      { key: 'laEstab', label: 'LAESTAB', value: 'test-laEstab' },
    ]);
  });

  test('returns correct entries when only `ukprn` is present', () => {
    expect(
      getLocationCodeEntries({
        ukprn: 'test-ukprn',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'ukprn', label: 'UKPRN', value: 'test-ukprn' },
    ]);
  });

  test('returns correct entries when only `code` is present', () => {
    expect(
      getLocationCodeEntries({
        code: 'test-code',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'code', label: 'Code', value: 'test-code' },
    ]);
  });

  test('returns correct entries when only `id` is present', () => {
    expect(
      getLocationCodeEntries({
        id: 'test-id',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'id', label: 'ID', value: 'test-id' },
    ]);
  });

  test('returns an empty array when no codes are present', () => {
    expect(getLocationCodeEntries({})).toEqual([]);
  });

  test('return correct entries when all codes are given', () => {
    expect(
      getLocationCodeEntries({
        id: 'test-id',
        code: 'test-code',
        oldCode: 'test-old-code',
        urn: 'test-urn',
        laEstab: 'test-laEstab',
        ukprn: 'test-ukprn',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'id', label: 'ID', value: 'test-id' },
      { key: 'code', label: 'Code', value: 'test-code' },
      { key: 'oldCode', label: 'Old code', value: 'test-old-code' },
      { key: 'urn', label: 'URN', value: 'test-urn' },
      { key: 'laEstab', label: 'LAESTAB', value: 'test-laEstab' },
      { key: 'ukprn', label: 'UKPRN', value: 'test-ukprn' },
    ]);
  });

  test('does not return entries for codes that are empty strings', () => {
    expect(
      getLocationCodeEntries({
        id: '',
        code: 'test-code',
        oldCode: '',
        urn: 'test-urn',
        laEstab: '',
        ukprn: '',
      }),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'code', label: 'Code', value: 'test-code' },
      { key: 'urn', label: 'URN', value: 'test-urn' },
    ]);
  });

  test('return correct entries when `overrideLabels` is provided', () => {
    expect(
      getLocationCodeEntries(
        {
          code: 'test-code',
          urn: 'test-urn',
        },
        {
          code: 'Custom code',
        },
      ),
    ).toEqual<LocationCodeEntry[]>([
      { key: 'code', label: 'Custom code', value: 'test-code' },
      { key: 'urn', label: 'URN', value: 'test-urn' },
    ]);
  });
});
