import createDataSetListRequest from '@frontend/modules/data-catalogue/utils/createDataSetListRequest';

describe('createDataSetListRequest', () => {
  test('returns the default request when no query params', () => {
    const result = createDataSetListRequest({});
    expect(result).toEqual({ orderBy: 'published', page: 1, sort: 'desc' });
  });

  test('returns the request with query params', () => {
    const result = createDataSetListRequest({
      latest: 'true',
      orderBy: 'relevance',
      page: 4,
      publicationId: 'publication-id',
      releaseId: 'release-id',
      searchTerm: 'find me',
      themeId: 'theme-id',
    });
    expect(result).toEqual({
      latest: 'true',
      orderBy: 'relevance',
      page: 4,
      publicationId: 'publication-id',
      releaseId: 'release-id',
      searchTerm: 'find me',
      sort: 'desc',
      themeId: 'theme-id',
    });
  });

  test('returns the correct order and sort params', () => {
    expect(createDataSetListRequest({ orderBy: 'relevance' })).toEqual({
      orderBy: 'relevance',
      page: 1,
      sort: 'desc',
    });

    expect(createDataSetListRequest({ orderBy: 'newest' })).toEqual({
      orderBy: 'published',
      page: 1,
      sort: 'desc',
    });

    expect(createDataSetListRequest({ orderBy: 'oldest' })).toEqual({
      orderBy: 'published',
      page: 1,
      sort: 'asc',
    });

    expect(createDataSetListRequest({ orderBy: 'title' })).toEqual({
      orderBy: 'title',
      page: 1,
      sort: 'asc',
    });
  });

  test('excludes search terms that are too short', () => {
    const result = createDataSetListRequest({ searchTerm: 'hi' });
    expect(result).toEqual({ orderBy: 'published', page: 1, sort: 'desc' });
  });

  test('excludes params that are undefined or empty', () => {
    const result = createDataSetListRequest({
      publicationId: undefined,
      releaseId: '',
    });
    expect(result).toEqual({ orderBy: 'published', page: 1, sort: 'desc' });
  });
});
