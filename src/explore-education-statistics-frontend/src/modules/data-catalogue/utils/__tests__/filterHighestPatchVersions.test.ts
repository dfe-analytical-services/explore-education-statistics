import filterHighestPatchVersions from '../filterHighestPatchVersions';

describe('filterHighestPatchVersions', () => {
  test('returns highest patch version for each major.minor group', () => {
    const versions = ['2.0', '2.0.1', '2.0.2', '1.1', '1.1.1', '1.0', '1.0.1'];
    const result = filterHighestPatchVersions(versions);
    expect(result).toEqual(['2.0.2', '1.1.1', '1.0.1']);
  });

  test('returns major.minor version if no patch versions exist', () => {
    const versions = ['2.0', '1.1', '1.0'];
    const result = filterHighestPatchVersions(versions);
    expect(result).toEqual(['2.0', '1.1', '1.0']);
  });

  test('handles mixed patch and non-patch versions', () => {
    const versions = ['2.0', '2.0.1', '1.1', '1.1.2', '1.1.1', '1.0'];
    const result = filterHighestPatchVersions(versions);
    expect(result).toEqual(['2.0.1', '1.1.2', '1.0']);
  });

  test('returns empty array for empty input', () => {
    const result = filterHighestPatchVersions([]);
    expect(result).toEqual([]);
  });

  test('handles single version', () => {
    expect(filterHighestPatchVersions(['2.0'])).toEqual(['2.0']);
    expect(filterHighestPatchVersions(['2.0.3'])).toEqual(['2.0.3']);
  });

  test('handles versions with more than three segments', () => {
    const versions = ['2.0.1.5', '2.0.2', '2.0'];
    const result = filterHighestPatchVersions(versions);
    // Only '2.0.2' is considered a patch version, '2.0.1.5' is ignored
    expect(result).toEqual(['2.0.2']);
  });
});
