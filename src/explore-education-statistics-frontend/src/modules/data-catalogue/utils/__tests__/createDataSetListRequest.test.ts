import createDataSetListRequest from '@frontend/modules/data-catalogue/utils/createDataSetListRequest';
import { DataSetListRequest } from '@frontend/services/dataSetService';

describe('createDataSetListRequest', () => {
  test('returns the default request when no query params', () => {
    const result = createDataSetListRequest({});
    expect(result).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });

  test('returns the request with query params', () => {
    const result = createDataSetListRequest({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      releaseId: 'release-id',
      sortBy: 'relevance',
      searchTerm: 'find me',
      themeId: 'theme-id',
    });
    expect(result).toEqual<DataSetListRequest>({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      releaseId: 'release-id',
      searchTerm: 'find me',
      sort: 'relevance',
      sortDirection: 'Desc',
      themeId: 'theme-id',
    });
  });

  test('returns the correct order and sort params', () => {
    expect(
      createDataSetListRequest({ sortBy: 'relevance' }),
    ).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'relevance',
      sortDirection: 'Desc',
    });

    expect(
      createDataSetListRequest({ sortBy: 'newest' }),
    ).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });

    expect(
      createDataSetListRequest({ sortBy: 'oldest' }),
    ).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Asc',
    });

    expect(
      createDataSetListRequest({ sortBy: 'title' }),
    ).toEqual<DataSetListRequest>({
      sort: 'title',
      page: 1,
      sortDirection: 'Asc',
    });
  });

  test('excludes search terms that are too short', () => {
    const result = createDataSetListRequest({ searchTerm: 'hi' });
    expect(result).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });

  test('excludes params that are undefined or empty', () => {
    const result = createDataSetListRequest({
      publicationId: undefined,
      releaseId: '',
    });
    expect(result).toEqual<DataSetListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });
});
