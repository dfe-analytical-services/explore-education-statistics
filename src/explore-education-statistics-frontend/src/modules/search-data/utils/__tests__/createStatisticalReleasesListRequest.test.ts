import { SearchDataPageQuery } from '@frontend/modules/search-data/SearchDataPage';
import createStatisticalReleasesListRequest, {
  createStatisticalReleasesSuggestRequest,
  getParamsFromQuery,
} from '../createStatisticalReleasesListRequest';

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
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

describe('createStatisticalReleasesListRequest', () => {
  test('returns minimal request with default values when no filters', () => {
    const testQuery: SearchDataPageQuery = {};

    const result = createStatisticalReleasesListRequest(testQuery);

    expect(result).toEqual({
      page: 1,
      search: '',
      orderBy: 'published desc',
    });
  });

  test('includes organisation filter when single organisationId provided', () => {
    const testQuery: SearchDataPageQuery = {
      organisationId: 'organisation-1',
    };

    const result = createStatisticalReleasesListRequest(testQuery);

    expect(result.filter).toBe(
      "publishingOrganisationIds/any(g: search.in(g, 'organisation-1', '|'))",
    );
  });

  test('includes organisation filter when multiple organisationIds provided', () => {
    const testQuery: SearchDataPageQuery = {
      organisationId: ['organisation-1', 'organisation-2'],
    };

    const result = createStatisticalReleasesListRequest(testQuery);

    expect(result.filter).toBe(
      "publishingOrganisationIds/any(g: search.in(g, 'organisation-1|organisation-2', '|'))",
    );
  });

  test('combines organisationId, releaseType and themeId filters', () => {
    const testQuery: SearchDataPageQuery = {
      organisationId: 'organisation-1',
      releaseType: 'AccreditedOfficialStatistics',
      themeId: ['theme-1', 'theme-2'],
    };

    const result = createStatisticalReleasesListRequest(testQuery);

    expect(result.filter).toBe(
      "publishingOrganisationIds/any(g: search.in(g, 'organisation-1', '|')) and search.in(releaseType, 'AccreditedOfficialStatistics', '|') and search.in(themeId, 'theme-1|theme-2', '|')",
    );
  });
});

describe('createStatisticalReleasesSuggestRequest', () => {
  test('returns request with search term and organisation filter', () => {
    const testQuery: SearchDataPageQuery = {
      organisationId: 'organisation-1',
    };

    const result = createStatisticalReleasesSuggestRequest(
      testQuery,
      'test search',
    );

    expect(result).toEqual({
      page: 1,
      search: 'test search',
      orderBy: 'published desc',
      filter:
        "publishingOrganisationIds/any(g: search.in(g, 'organisation-1', '|'))",
    });
  });
});

describe('getParamsFromQuery', () => {
  test('returns undefined organisationIds when query is empty', () => {
    const result = getParamsFromQuery({});

    expect(result.organisationIds).toBeUndefined();
  });

  test('converts single organisationId to array', () => {
    const result = getParamsFromQuery({ organisationId: 'organisation-1' });

    expect(result.organisationIds).toEqual(['organisation-1']);
  });

  test('keeps organisationId array as is', () => {
    const result = getParamsFromQuery({
      organisationId: ['organisation-1', 'organisation-2'],
    });

    expect(result.organisationIds).toEqual([
      'organisation-1',
      'organisation-2',
    ]);
  });
});
