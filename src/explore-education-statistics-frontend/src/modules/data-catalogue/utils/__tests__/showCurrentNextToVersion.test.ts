import showCurrentNextToVersion from '../showCurrentNextToVersion';

describe('showCurrentNextToVersion', () => {
  it('returns false when the latest patch is 1, if version is in latestPatchLookup (patch) and the version opened is not the latest patch', () => {
    const latestPatchLookup = { '2.0': 1 };
    expect(showCurrentNextToVersion('2.0', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('2.1', latestPatchLookup)).toBe(true);
  });

  it('returns false when the latest patch is 3, if version is in latestPatchLookup (patch) and the version opened is not the latest patch', () => {
    const latestPatchLookup = { '1.0': 3 };
    expect(showCurrentNextToVersion('1.0.3', latestPatchLookup)).toBe(true);

    expect(showCurrentNextToVersion('1.0.5', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.0.4', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.0.2', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.0.1', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.0', latestPatchLookup)).toBe(false);
  });

  it('returns true when the version supplied has a minor digit, exists in the lookup and false when not', () => {
    const latestPatchLookup = { '1.2': 5 };
    expect(showCurrentNextToVersion('1.2.5', latestPatchLookup)).toBe(true);

    expect(showCurrentNextToVersion('1.2.6', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.2.4', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.2.3', latestPatchLookup)).toBe(false);
    expect(showCurrentNextToVersion('1.2', latestPatchLookup)).toBe(false);
  });

  it('returns true if version is not in latestPatchLookup (non-patch)', () => {
    const latestPatchLookup = { '1.2': 3 };
    expect(showCurrentNextToVersion('1.2', latestPatchLookup)).toBe(false);

    expect(showCurrentNextToVersion('3.0', latestPatchLookup)).toBe(true);
    expect(showCurrentNextToVersion('2.0', latestPatchLookup)).toBe(true);
    expect(showCurrentNextToVersion('1.0', latestPatchLookup)).toBe(true);
  });

  it('returns false if latestPatchLookup is undefined and version is patch', () => {
    expect(showCurrentNextToVersion('1.2.3', undefined)).toBe(false);
  });

  it('returns true if latestPatchLookup is undefined and version is not patch', () => {
    expect(showCurrentNextToVersion('1.2', undefined)).toBe(true);
  });
});
