import isGuid from '@common/utils/string/isGuid';

describe('isGuid', () => {
  test('returns true when given a v4 uuid', () => {
    expect(isGuid('6fc52297-e357-465b-9e91-6d7b05d0f805')).toBe(true);
  });

  test('returns false when given an invalid v4 uuid', () => {
    expect(isGuid('')).toBe(false);
    expect(isGuid('not-a-guid')).toBe(false);

    // Slightly change the string
    expect(isGuid('6fc52297-e357-465b-9e91_6d7b05d0f805')).toBe(false);
    expect(isGuid('6fc52297_e357_465b_9e91_6d7b05d0f805')).toBe(false);
  });
});
