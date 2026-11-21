import buildPublicationSearchFilter from '@frontend/modules/api/search/buildPublicationSearchFilter';

jest.mock('@azure/search-documents', () => ({
  odata: (strings: TemplateStringsArray, ...values: unknown[]) => {
    let result = '';
    strings.forEach((str, i) => {
      result += str;
      if (i < values.length) {
        result += `'${values[i]}'`;
      }
    });
    return result;
  },
}));

describe('buildPublicationSearchFilter', () => {
  test('returns undefined when no filters provided', () => {
    expect(buildPublicationSearchFilter({})).toBeUndefined();
  });

  test('builds a release type filter', () => {
    expect(
      buildPublicationSearchFilter({ releaseType: 'AdHocStatistics' }),
    ).toBe("releaseType eq 'AdHocStatistics'");
  });

  test('builds a theme filter', () => {
    expect(buildPublicationSearchFilter({ themeId: 'theme-1' })).toBe(
      "themeId eq 'theme-1'",
    );
  });

  test('builds a combined release type and theme filter', () => {
    expect(
      buildPublicationSearchFilter({
        releaseType: 'AdHocStatistics',
        themeId: 'theme-1',
      }),
    ).toBe("releaseType eq 'AdHocStatistics' and themeId eq 'theme-1'");
  });
});
