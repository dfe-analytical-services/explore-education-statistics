import { ReleaseSummary } from '@common/services/publicationService';
import getUpdatedQueryParams from '@frontend/modules/data-catalogue/utils/getUpdatedQueryParams';

describe('getUpdatedQueryParams', () => {
  const testReleases: ReleaseSummary[] = [
    {
      id: 'release-id-1',
      title: 'Release title 1',
      yearTitle: '2010',
      coverageTitle: 'Release coverage title 1',
      published: '2010-01-01',
      releaseName: 'Release name 1',
      slug: 'release-slug-1',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'NationalStatistics',
      latestRelease: false,
    },
    {
      id: 'release-id-2',
      title: 'Release title 2',
      yearTitle: '2011',
      coverageTitle: 'Release coverage title 2',
      published: '2011-01-01',
      releaseName: 'Release name 2',
      slug: 'release-slug-2',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'NationalStatistics',
      latestRelease: false,
    },
    {
      id: 'release-id-3',
      title: 'Release title 3',
      yearTitle: '2012',
      coverageTitle: 'Release coverage title 3',
      published: '2012-01-01',
      releaseName: 'Release name 3',
      slug: 'release-slug-3',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'NationalStatistics',
      latestRelease: true,
    },
  ];

  describe('filter by theme', () => {
    test('selecting a theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        orderBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        themeId: 'selected-theme-id',
        orderBy: 'newest',
      });
    });

    test('removes page, publication and release params when changing theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        orderBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        orderBy: 'newest',
        themeId: 'selected-theme-id',
      });
    });

    test('removes page, publication and release params when changing to all themes', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'all',
        orderBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        orderBy: 'newest',
      });
    });

    test('keeps the latest param when changing theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        orderBy: 'newest',
        query: {
          latest: 'false',
          page: '4',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latest: 'false',
        orderBy: 'newest',
        themeId: 'selected-theme-id',
      });
    });
  });

  describe('filter by publication', () => {
    test('fetches releases and selects the latest one when selecting a publication', async () => {
      const handleFetchReleases = jest.fn().mockResolvedValue(testReleases);

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'selected-publication-id',
        orderBy: 'newest',
        query: {
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).toHaveBeenCalled();

      expect(result).toEqual({
        publicationId: 'selected-publication-id',
        orderBy: 'newest',
        releaseId: 'release-id-3',
        themeId: 'theme-id',
      });
    });

    test('fetches releases and selects the latest one when changing publication', async () => {
      const handleFetchReleases = jest
        .fn()
        .mockResolvedValue([{ ...testReleases[2], id: 'release-id-4' }]);

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'selected-publication-id',
        orderBy: 'newest',
        query: {
          publicationId: 'publication-id',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).toHaveBeenCalled();

      expect(result).toEqual({
        publicationId: 'selected-publication-id',
        orderBy: 'newest',
        releaseId: 'release-id-4',
        themeId: 'theme-id',
      });
    });

    test('removes page and release params when changing to all publications', async () => {
      const handleFetchReleases = jest.fn();

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'all',
        orderBy: 'newest',
        query: {
          page: '4',
          releaseId: 'release-id',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).not.toHaveBeenCalled();

      expect(result).toEqual({
        orderBy: 'newest',
        themeId: 'theme-id',
      });
    });

    test('removes latest param when selecting a publication', async () => {
      const handleFetchReleases = jest.fn().mockResolvedValue(testReleases);

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'selected-publication-id',
        orderBy: 'newest',
        query: {
          latest: 'false',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(result).toEqual({
        orderBy: 'newest',
        publicationId: 'selected-publication-id',
        releaseId: 'release-id-3',
        themeId: 'theme-id',
      });
    });
  });

  describe('filter by release', () => {
    test('removes the page param and updates the release id when changing release', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseId',
        nextValue: 'selected-release-id',
        orderBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          orderBy: 'newest',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        publicationId: 'publication-id',
        releaseId: 'selected-release-id',
        orderBy: 'newest',
        themeId: 'theme-id',
      });
    });

    test('removes the release param and sets latest to false when changing to all releases from a specific release', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseId',
        nextValue: 'all',
        orderBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          orderBy: 'newest',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latest: 'false',
        publicationId: 'publication-id',
        orderBy: 'newest',
        themeId: 'theme-id',
      });
    });

    test('sets latest to false when changing to all releases', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseId',
        nextValue: 'all',
        orderBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latest: 'false',
        orderBy: 'newest',
      });
    });
    test('sets latest to true when changing from all to latest releases', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseId',
        nextValue: 'latest',
        orderBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latest: 'true',
        orderBy: 'newest',
      });
    });
  });

  describe('filter by search term', () => {
    test('adds the search term to the params and sets orderBy to relevance when search', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'searchTerm',
        nextValue: 'find me',
        orderBy: 'newest',
        query: { orderBy: 'newest' },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        orderBy: 'relevance',
        searchTerm: 'find me',
      });
    });

    test('keeps the latest, theme, publication and release params when search', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'searchTerm',
        nextValue: 'find me',
        orderBy: 'newest',
        query: {
          latest: 'false',
          orderBy: 'newest',
          publicationId: 'publication-id',
          releaseId: 'release-id',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latest: 'false',
        orderBy: 'relevance',
        publicationId: 'publication-id',
        releaseId: 'release-id',
        searchTerm: 'find me',
        themeId: 'theme-id',
      });
    });
  });
});
