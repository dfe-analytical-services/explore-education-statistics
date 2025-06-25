import isPatchVersion from '../isPatchVersion';

describe('isPatchVersion', () => {
  it('returns false for undefined', () => {
    expect(isPatchVersion(undefined)).toBe(false);
  });

  it('returns false for empty string', () => {
    expect(isPatchVersion('')).toBe(false);
  });

  it('returns false for version with less than 3 parts', () => {
    expect(isPatchVersion('1.0')).toBe(false);
    expect(isPatchVersion('1')).toBe(false);
  });

  it('returns false for version with more than 3 parts', () => {
    expect(isPatchVersion('1.0.1.2')).toBe(false);
  });

  it('returns false for patch version 0', () => {
    expect(isPatchVersion('1.2.0')).toBe(false);
  });

  it('returns true for patch version > 0', () => {
    expect(isPatchVersion('1.2.1')).toBe(true);
    expect(isPatchVersion('0.0.5')).toBe(true);
  });

  it('returns false for non-numeric patch version', () => {
    expect(isPatchVersion('1.2.a')).toBe(false);
  });

  it('returns false for negative patch version', () => {
    expect(isPatchVersion('1.2.-1')).toBe(false);
  });
});
