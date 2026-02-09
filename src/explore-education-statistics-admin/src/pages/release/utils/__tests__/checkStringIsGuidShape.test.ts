import checkStringIsGuidShape from '@admin/pages/release/utils/checkStringIsGuidShape';

describe('checkStringIsGuidShape', () => {
  test('returns true for valid lowercase guid shape', () => {
    expect(checkStringIsGuidShape('123e4567-e89b-12d3-a456-426614174000')).toBe(
      true,
    );
  });

  test('returns true for valid uppercase guid shape', () => {
    expect(checkStringIsGuidShape('123E4567-E89B-12D3-A456-426614174000')).toBe(
      true,
    );
  });

  test('returns true for valid mixed-case guid shape', () => {
    expect(checkStringIsGuidShape('123e4567-E89B-12d3-a456-426614174000')).toBe(
      true,
    );
  });

  test('returns false for missing hyphens', () => {
    expect(checkStringIsGuidShape('123e4567e89b12d3a456426614174000')).toBe(
      false,
    );
  });

  test('returns false for wrong segment lengths', () => {
    expect(
      checkStringIsGuidShape('123e4567-e89b-12d3-a4567-426614174000'),
    ).toBe(false);
  });

  test('returns false for non-hex characters', () => {
    expect(checkStringIsGuidShape('123e4567-e89b-12d3-a456-42661417400Z')).toBe(
      false,
    );
  });

  test('returns false for empty string', () => {
    expect(checkStringIsGuidShape('')).toBe(false);
  });

  test('returns false for undefined object', () => {
    expect(checkStringIsGuidShape(undefined)).toBe(false);
  });

  test('returns false for extra prefix or suffix', () => {
    expect(
      checkStringIsGuidShape('x123e4567-e89b-12d3-a456-426614174000'),
    ).toBe(false);
    expect(
      checkStringIsGuidShape('123e4567-e89b-12d3-a456-426614174000x'),
    ).toBe(false);
  });
});
