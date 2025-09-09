import ensureTrailingSlash from '../ensureTrailingSlash';

describe('ensureTrailingSlash', () => {
  test('adds a slash if not present', () => {
    expect(ensureTrailingSlash('test-string')).toBe('test-string/');
  });

  test('does not add a slash if already present', () => {
    expect(ensureTrailingSlash('test-string/')).toBe('test-string/');
  });
});
