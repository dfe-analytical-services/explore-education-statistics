import createDataSetFileListRequest from '@frontend/modules/data-catalogue/utils/createDataSetFileListRequest';
import { DataSetFileListRequest } from '@frontend/services/dataSetFileService';

describe('createDataSetListRequest', () => {
  test('returns the default request when no query params', () => {
    const result = createDataSetFileListRequest({});
    expect(result).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });

  test('returns the request with query params', () => {
    const result = createDataSetFileListRequest({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      sortBy: 'relevance',
      searchTerm: 'find me',
      themeId: 'theme-id',
    });
    expect(result).toEqual<DataSetFileListRequest>({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      searchTerm: 'find me',
      sort: 'relevance',
      sortDirection: 'Desc',
      themeId: 'theme-id',
    });
  });

  test('returns the correct order and sort params', () => {
    expect(
      createDataSetFileListRequest({ sortBy: 'relevance' }),
    ).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'relevance',
      sortDirection: 'Desc',
    });

    expect(
      createDataSetFileListRequest({ sortBy: 'newest' }),
    ).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });

    expect(
      createDataSetFileListRequest({ sortBy: 'oldest' }),
    ).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Asc',
    });

    expect(
      createDataSetFileListRequest({ sortBy: 'title' }),
    ).toEqual<DataSetFileListRequest>({
      sort: 'title',
      page: 1,
      sortDirection: 'Asc',
    });
  });

  test('returns `natural` sort when the release id is set', () => {
    const result = createDataSetFileListRequest({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      releaseVersionId: 'release-id',
      sortBy: 'newest',
      searchTerm: 'find me',
      themeId: 'theme-id',
    });
    expect(result).toEqual<DataSetFileListRequest>({
      latestOnly: 'true',
      page: 4,
      publicationId: 'publication-id',
      releaseId: 'release-id',
      searchTerm: 'find me',
      sort: 'natural',
      themeId: 'theme-id',
    });
  });

  test('excludes search terms that are too short', () => {
    const result = createDataSetFileListRequest({ searchTerm: 'hi' });
    expect(result).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });

  test('excludes params that are undefined or empty', () => {
    const result = createDataSetFileListRequest({
      publicationId: undefined,
      releaseVersionId: '',
    });
    expect(result).toEqual<DataSetFileListRequest>({
      page: 1,
      sort: 'published',
      sortDirection: 'Desc',
    });
  });
});
