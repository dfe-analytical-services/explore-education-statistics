import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';

describe('getApiDataSetLocationCodes', () => {
  test('returns the correct codes when code and oldCode are present', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        code: 'test-code',
        oldCode: 'test-old-code',
      }),
    ).toEqual('Code: test-code, Old code: test-old-code');
  });

  test('returns the correct codes when urn and laEstab are present', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        urn: 'test-urn',
        laEstab: 'test-laEstab',
      }),
    ).toEqual('URN: test-urn, LAESTAB: test-laEstab');
  });

  test('returns the correct code when only ukprn is present', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        ukprn: 'test-ukprn',
      }),
    ).toEqual('UKPRN: test-ukprn');
  });

  test('returns the correct code when only code is present', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        code: 'test-code',
      }),
    ).toEqual('test-code');
  });

  test('returns an empty string when no codes are present', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
      }),
    ).toEqual('');
  });

  test('handles when all codes are given', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        code: 'test-code',
        oldCode: 'test-old-code',
        urn: 'test-urn',
        laEstab: 'test-laEstab',
        ukprn: 'test-ukprn',
      }),
    ).toEqual(
      'Code: test-code, Old code: test-old-code, URN: test-urn, LAESTAB: test-laEstab, UKPRN: test-ukprn',
    );
  });

  test('handles empty strings', () => {
    expect(
      getApiDataSetLocationCodes({
        label: 'Test location',
        code: 'test-code',
        oldCode: '',
        urn: 'test-urn',
        laEstab: '',
        ukprn: '',
      }),
    ).toEqual('Code: test-code, URN: test-urn');
  });
});
