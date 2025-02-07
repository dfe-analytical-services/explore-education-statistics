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
      slug: 'release-slug-1',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'AccreditedOfficialStatistics',
      latestRelease: false,
    },
    {
      id: 'release-id-2',
      title: 'Release title 2',
      yearTitle: '2011',
      coverageTitle: 'Release coverage title 2',
      published: '2011-01-01',
      slug: 'release-slug-2',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'AccreditedOfficialStatistics',
      latestRelease: false,
    },
    {
      id: 'release-id-3',
      title: 'Release title 3',
      yearTitle: '2012',
      coverageTitle: 'Release coverage title 3',
      published: '2012-01-01',
      slug: 'release-slug-3',
      nextReleaseDate: {
        year: '',
        month: '',
        day: '',
      },
      type: 'AccreditedOfficialStatistics',
      latestRelease: true,
    },
  ];

  describe('filter by theme', () => {
    test('selecting a theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        sortBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        themeId: 'selected-theme-id',
        sortBy: 'newest',
      });
    });

    test('removes page, publication and release params when changing theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        sortBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        sortBy: 'newest',
        themeId: 'selected-theme-id',
      });
    });

    test('removes page, publication and release params when changing to all themes', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'all',
        sortBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        sortBy: 'newest',
      });
    });

    test('keeps the latestOnly param when changing theme', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'themeId',
        nextValue: 'selected-theme-id',
        sortBy: 'newest',
        query: {
          latestOnly: 'false',
          page: '4',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          themeId: 'previous-theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latestOnly: 'false',
        sortBy: 'newest',
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
        sortBy: 'newest',
        query: {
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).toHaveBeenCalled();

      expect(result).toEqual({
        publicationId: 'selected-publication-id',
        releaseVersionId: 'release-id-3',
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
        sortBy: 'newest',
        query: {
          publicationId: 'publication-id',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).toHaveBeenCalled();

      expect(result).toEqual({
        publicationId: 'selected-publication-id',
        releaseVersionId: 'release-id-4',
        themeId: 'theme-id',
      });
    });

    test('removes page and release params when changing to all publications', async () => {
      const handleFetchReleases = jest.fn();

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'all',
        sortBy: 'newest',
        query: {
          page: '4',
          releaseVersionId: 'release-id',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(handleFetchReleases).not.toHaveBeenCalled();

      expect(result).toEqual({
        sortBy: 'newest',
        themeId: 'theme-id',
      });
    });

    test('removes latestOnly param when selecting a publication', async () => {
      const handleFetchReleases = jest.fn().mockResolvedValue(testReleases);

      const result = await getUpdatedQueryParams({
        filterType: 'publicationId',
        nextValue: 'selected-publication-id',
        sortBy: 'newest',
        query: {
          latestOnly: 'false',
          themeId: 'theme-id',
        },
        onFetchReleases: handleFetchReleases,
      });

      expect(result).toEqual({
        publicationId: 'selected-publication-id',
        releaseVersionId: 'release-id-3',
        themeId: 'theme-id',
      });
    });
  });

  describe('filter by release', () => {
    test('removes the page and sortBy params and updates the release id when changing release', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseVersionId',
        nextValue: 'selected-release-id',
        sortBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          sortBy: 'newest',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        publicationId: 'publication-id',
        releaseVersionId: 'selected-release-id',
        themeId: 'theme-id',
      });
    });

    test('removes the release param and sets latestOnly to false when changing to all releases from a specific release', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseVersionId',
        nextValue: 'all',
        sortBy: 'newest',
        query: {
          page: '4',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          sortBy: 'newest',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latestOnly: 'false',
        publicationId: 'publication-id',
        sortBy: 'newest',
        themeId: 'theme-id',
      });
    });

    test('sets latestOnly to false when changing to all releases', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseVersionId',
        nextValue: 'all',
        sortBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latestOnly: 'false',
      });
    });

    test('sets latestOnly to true when changing from all to latest releases', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'releaseVersionId',
        nextValue: 'latest',
        sortBy: 'newest',
        query: {},
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latestOnly: 'true',
      });
    });
  });

  describe('filter by search term', () => {
    test('adds the search term to the params and sets sortBy to relevance when search', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'searchTerm',
        nextValue: 'find me',
        sortBy: 'newest',
        query: { sortBy: 'newest' },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        sortBy: 'relevance',
        searchTerm: 'find me',
      });
    });

    test('keeps the latestOnly, theme, publication and release params when search', async () => {
      const result = await getUpdatedQueryParams({
        filterType: 'searchTerm',
        nextValue: 'find me',
        sortBy: 'newest',
        query: {
          latestOnly: 'false',
          sortBy: 'newest',
          publicationId: 'publication-id',
          releaseVersionId: 'release-id',
          themeId: 'theme-id',
        },
        onFetchReleases: () => Promise.resolve([]),
      });
      expect(result).toEqual({
        latestOnly: 'false',
        sortBy: 'relevance',
        publicationId: 'publication-id',
        releaseVersionId: 'release-id',
        searchTerm: 'find me',
        themeId: 'theme-id',
      });
    });
  });
});
