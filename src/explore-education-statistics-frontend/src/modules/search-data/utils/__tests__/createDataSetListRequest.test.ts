import { SearchDataPageQuery } from '@frontend/modules/search-data/SearchDataPage';
import createDataSetListRequest, {
  createDataSetSuggestRequest,
  getParamsFromQuery,
} from '../createDataSetListRequest';

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

describe('createDataSetListRequest', () => {
  describe('with no filters', () => {
    test('returns minimal request with default values', () => {
      const testQuery: SearchDataPageQuery = {};

      const result = createDataSetListRequest(testQuery);

      expect(result).toEqual({
        page: 1,
        search: '',
        orderBy: 'published desc',
        filter: 'latestData eq true',
      });
    });
  });

  describe('search term', () => {
    test('includes search when search term is 3 characters', () => {
      const testQuery: SearchDataPageQuery = {
        search: 'abc',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.search).toBe('abc');
    });

    test('includes search when search term is more than 3 characters', () => {
      const testQuery: SearchDataPageQuery = {
        search: 'test search term',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.search).toBe('test search term');
    });

    test('excludes search when search term is less than 3 characters', () => {
      const testQuery: SearchDataPageQuery = {
        search: 'ab',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.search).toBe('');
    });
  });

  describe('filter building', () => {
    test('includes theme filter when themeIds provided', () => {
      const testQuery: SearchDataPageQuery = {
        themeId: ['theme-1', 'theme-2', 'theme-3'],
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe(
        "search.in(themeId, 'theme-1|theme-2|theme-3', '|') and latestData eq true",
      );
    });

    test('includes publication filter when single publicationId provided', () => {
      const testQuery: SearchDataPageQuery = {
        publicationId: 'pub-1',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe(
        "search.in(publicationId, 'pub-1', '|') and latestData eq true",
      );
    });

    test('includes filter when multiple releaseTypes provided', () => {
      const testQuery: SearchDataPageQuery = {
        releaseType: ['AccreditedOfficialStatistics', 'OfficialStatistics'],
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe(
        "search.in(releaseType, 'AccreditedOfficialStatistics|OfficialStatistics', '|') and latestData eq true",
      );
    });

    test('excludes realeaseType filter when invalid releaseType provided', () => {
      const testInvalidQuery: SearchDataPageQuery = {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        releaseType: 'InvalidType' as any,
      };

      const result = createDataSetListRequest(testInvalidQuery);

      expect(result.filter).toBe('latestData eq true');
    });

    test('when latestDataOnly is undefined', () => {
      const testQuery: SearchDataPageQuery = {
        latestDataOnly: undefined,
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe('latestData eq true');
    });

    test('excludes latestdata filter when latestDataOnly is false', () => {
      const testQuery: SearchDataPageQuery = {
        latestDataOnly: 'false',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBeUndefined();
    });

    test('includes api filter when dataSetType is api', () => {
      const testQuery: SearchDataPageQuery = {
        dataSetType: 'api',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe(
        "latestData eq true and api/id ne null and api/id ne ''",
      );
    });

    test('excludes api filter when dataSetType is all', () => {
      const testQuery: SearchDataPageQuery = {
        dataSetType: 'all',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe('latestData eq true');
    });

    test('excludes api filter when dataSetType is undefined', () => {
      const testQuery: SearchDataPageQuery = {
        dataSetType: undefined,
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe('latestData eq true');
    });

    test('combines themeId, publicationId, releaseType and geographicLevel filters', () => {
      const testQuery: SearchDataPageQuery = {
        themeId: ['theme-1', 'theme-2'],
        publicationId: 'pub-1',
        releaseType: ['AccreditedOfficialStatistics', 'OfficialStatistics'],
        geographicLevel: ['NAT', 'REG'],
      };

      const result = createDataSetListRequest(testQuery);

      expect(result.filter).toBe(
        "(search.in(themeId, 'theme-1|theme-2', '|') or search.in(publicationId, 'pub-1', '|')) and search.in(releaseType, 'AccreditedOfficialStatistics|OfficialStatistics', '|') and geographicLevels/any(g: search.in(g, 'NAT|REG', '|')) and latestData eq true",
      );
    });
  });

  describe('full request with all parameters', () => {
    test('returns complete request with all fields', () => {
      const testQuery: SearchDataPageQuery = {
        page: 2,
        search: 'test search',
        sortBy: 'title',
        themeId: 'theme-1',
        publicationId: 'pub-3',
        releaseType: 'AccreditedOfficialStatistics',
        geographicLevel: 'NAT',
        dataSetType: 'api',
        latestDataOnly: 'true',
      };

      const result = createDataSetListRequest(testQuery);

      expect(result).toEqual({
        page: 2,
        search: 'test search',
        orderBy: 'title asc',
        filter:
          "(search.in(themeId, 'theme-1', '|') or search.in(publicationId, 'pub-3', '|')) and search.in(releaseType, 'AccreditedOfficialStatistics', '|') and geographicLevels/any(g: search.in(g, 'NAT', '|')) and latestData eq true and api/id ne null and api/id ne ''",
      });
    });
  });
});

describe('createDataSetSuggestRequest', () => {
  test('returns request with search term', () => {
    const testQuery: SearchDataPageQuery = {};
    const testSearchTerm = 'test search';

    const result = createDataSetSuggestRequest(testQuery, testSearchTerm);

    expect(result).toEqual({
      page: 1,
      search: 'test search',
      orderBy: 'published desc',
      filter: 'latestData eq true',
    });
  });
});

describe('getParamsFromQuery', () => {
  test('returns default values when query is empty', () => {
    const testQuery: SearchDataPageQuery = {};

    const result = getParamsFromQuery(testQuery);

    expect(result).toEqual({
      dataSetType: 'all',
      geographicLevels: undefined,
      latestDataOnly: true,
      page: undefined,
      publicationIds: undefined,
      releaseTypes: undefined,
      search: undefined,
      sortBy: 'newest',
      themeIds: undefined,
    });
  });

  test('converts single themeId to array', () => {
    const testQuery: SearchDataPageQuery = {
      themeId: 'theme-1',
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.themeIds).toEqual(['theme-1']);
  });

  test('keeps themeId array as is', () => {
    const testQuery: SearchDataPageQuery = {
      themeId: ['theme-1', 'theme-2'],
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.themeIds).toEqual(['theme-1', 'theme-2']);
  });

  test('returns latestDataOnly as true by default', () => {
    const testQuery: SearchDataPageQuery = {};

    const result = getParamsFromQuery(testQuery);

    expect(result.latestDataOnly).toBe(true);
  });

  test('returns latestDataOnly as true when not false', () => {
    const testQuery: SearchDataPageQuery = {
      latestDataOnly: 'true',
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.latestDataOnly).toBe(true);
  });

  test('returns latestDataOnly as false when explicitly false', () => {
    const testQuery: SearchDataPageQuery = {
      latestDataOnly: 'false',
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.latestDataOnly).toBe(false);
  });

  test('returns dataSetType as api when specified', () => {
    const testQuery: SearchDataPageQuery = {
      dataSetType: 'api',
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.dataSetType).toBe('api');
  });

  test('returns dataSetType as all by default', () => {
    const testQuery: SearchDataPageQuery = {};

    const result = getParamsFromQuery(testQuery);

    expect(result.dataSetType).toBe('all');
  });

  test('returns newest as default sortBy when undefined', () => {
    const testQuery: SearchDataPageQuery = {
      sortBy: undefined,
    };

    const result = getParamsFromQuery(testQuery);

    expect(result.sortBy).toBe('newest');
  });
});
