import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import SearchDataPage from '@frontend/modules/search-data/SearchDataPage';
import _publicationService, {
  PublicationListSummary,
} from '@common/services/publicationService';
import _azureDataSetService from '@frontend/services/azureDataSetService';
import _azurePublicationService from '@frontend/services/azurePublicationService';
import { testPublicationTree } from '@frontend/modules/search-data/__tests__/__data__/testPublicationTree';
import { PaginatedList } from '@common/services/types/pagination';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { testDataSetFileSummaries } from '@frontend/modules/data-catalogue/__data__/testDataSets';

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
  odata: jest.fn(),
}));

jest.mock('@common/services/publicationService');
jest.mock('@frontend/services/azureDataSetService');
jest.mock('@frontend/services/azurePublicationService');

const publicationService = jest.mocked(_publicationService);
const azureDataSetService = jest.mocked(_azureDataSetService);
const azurePublicationService = jest.mocked(_azurePublicationService);

const mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const mockRouter = {
  pathname: '/search-data',
  query: {},
  push: jest.fn(),
  replace: jest.fn(),
};

jest.mock('next/router', () => ({
  useRouter: () => mockRouter,
}));

describe('SearchDataPage', () => {
  const testDataSetsResults = {
    results: testDataSetFileSummaries,
    paging: {
      page: 1,
      pageSize: 10,
      totalPages: 1,
      totalResults: 2,
    },
  };

  const testPublicationsResults = {
    results: testPublications,
    paging: {
      page: 1,
      pageSize: 10,
      totalPages: 1,
      totalResults: 2,
    },
  } as PaginatedList<PublicationListSummary>;

  beforeEach(() => {
    mockRouter.pathname = '/search-data';
    mockRouter.query = {};

    azureDataSetService.listDataSets.mockResolvedValue(testDataSetsResults);
    azurePublicationService.listStatisticalReleases.mockResolvedValue(
      testPublicationsResults,
    );
    publicationService.getPublicationTree.mockResolvedValue(
      testPublicationTree,
    );
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('data search page', () => {
    test('renders page with search form and results', async () => {
      render(<SearchDataPage />);

      expect(
        await screen.findByRole('heading', {
          name: 'Explore our education statistics',
        }),
      ).toBeInTheDocument();

      expect(screen.getByRole('combobox')).toBeInTheDocument();

      expect(
        screen.getByRole('heading', { name: '2 results' }),
      ).toBeInTheDocument();

      expect(
        await screen.findByTestId('data-set-file-list'),
      ).toBeInTheDocument();
      expect(screen.getByText('Data set 1')).toBeInTheDocument();
      expect(screen.getByText('Data set 2')).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Reset filters' }),
      ).not.toBeInTheDocument();
    });

    test('renders navigation tabs correctly', async () => {
      render(<SearchDataPage />);

      const nav = await screen.findByRole('navigation', {
        name: 'Search mode',
      });

      const links = within(nav).getAllByRole('link');

      expect(links).toHaveLength(2);
      expect(links[0]).toHaveTextContent('Statistical releases');
      expect(links[1]).toHaveTextContent('Data');
      expect(links[1]).toHaveAttribute('aria-current', 'page');
    });

    test('renders help and related information section', async () => {
      render(<SearchDataPage />);

      expect(
        await screen.findByRole('heading', {
          name: 'Help and related information',
        }),
      ).toBeInTheDocument();

      const helpColumn = screen.getByTestId('related-info-column');

      expect(
        within(helpColumn).getByRole('button', {
          name: /What are statistical releases\?/,
        }),
      ).toBeInTheDocument();

      expect(
        within(helpColumn).getByRole('button', { name: /What is data\?/ }),
      ).toBeInTheDocument();

      expect(
        within(helpColumn).getByRole('link', { name: 'Glossary' }),
      ).toBeInTheDocument();
    });

    test('renders filters section', async () => {
      render(<SearchDataPage />);

      expect(
        await screen.findByRole('heading', { name: 'Filter and sort' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('group', { name: 'Filter by Geographic Level' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('group', { name: 'Show latest or all releases' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('group', { name: 'Type of data' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('group', { name: 'Filter by Release type' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('group', { name: 'Sort by' }),
      ).toBeInTheDocument();
    });

    test('renders skip link to results', async () => {
      render(<SearchDataPage />);

      const skipLink = await screen.findByRole('link', {
        name: 'Skip to search results',
      });

      expect(skipLink).toHaveAttribute('href', '#searchResults');
    });

    test('displays message when no results found', async () => {
      azureDataSetService.listDataSets.mockResolvedValue({
        results: [],
        paging: {
          page: 1,
          pageSize: 10,
          totalPages: 0,
          totalResults: 0,
        },
      });

      render(<SearchDataPage />);

      expect(
        await screen.findByText('No data currently published.'),
      ).toBeInTheDocument();
    });

    test('displays message when no results found and is filtered', async () => {
      azureDataSetService.listDataSets.mockResolvedValue({
        results: [],
        paging: {
          page: 1,
          pageSize: 10,
          totalPages: 0,
          totalResults: 0,
        },
      });

      mockRouter.pathname = '/search-data';
      mockRouter.query = { search: 'test' };

      render(<SearchDataPage />);

      expect(
        await screen.findByText('There are no matching results.'),
      ).toBeInTheDocument();
    });
  });

  describe('filtering', () => {
    test('renders default sorted by text and no filtered string', async () => {
      render(<SearchDataPage />);

      expect(await screen.findByText(/sorted by newest/i)).toBeInTheDocument();

      expect(screen.queryByText('filtered by:')).not.toBeInTheDocument();
    });

    test('renders correct text when search term present', async () => {
      mockRouter.query = { search: 'test', sortBy: 'relevance' };

      render(<SearchDataPage />);

      expect(
        await screen.findByText(/sorted by relevance/i),
      ).toBeInTheDocument();

      expect(screen.getByText(/filtered by:/)).toBeInTheDocument();
      expect(screen.getByText(/Remove filter:/i)).toBeInTheDocument();
      expect(screen.getByText(/Search:/i)).toBeInTheDocument();
    });

    test('renders correct text when other filters present', async () => {
      mockRouter.query = {
        geographicLevel: 'NAT',
        releaseType: 'AdHocStatistics',
        dataSetType: 'api',
      };

      render(<SearchDataPage />);

      expect(await screen.findByText(/filtered by:/)).toBeInTheDocument();
      expect(
        screen.getByText(/Ad hoc statistics, National, API data sets only/),
      ).toBeInTheDocument();
      const resetButtons = screen.getAllByTestId('filter-reset');
      expect(resetButtons).toHaveLength(3);
    });

    test('does not render filter text for data filters on release search page', async () => {
      mockRouter.pathname = '/search-releases';
      mockRouter.query = {
        geographicLevel: 'NAT',
        releaseType: 'AdHocStatistics',
        dataSetType: 'api',
      };

      render(<SearchDataPage />);

      expect(await screen.findByText(/filtered by:/)).toBeInTheDocument();
      expect(
        screen.queryByText(/Ad hoc statistics, National, API data sets only/),
      ).not.toBeInTheDocument();
      const resetButtons = screen.getAllByTestId('filter-reset');
      expect(resetButtons).toHaveLength(1);
    });
  });

  describe('releases search page', () => {
    beforeEach(() => {
      mockRouter.pathname = '/search-releases';
    });

    test('renders page with search form and results', async () => {
      render(<SearchDataPage />);

      expect(
        await screen.findByRole('heading', {
          name: 'Explore our education statistics',
        }),
      ).toBeInTheDocument();

      expect(screen.getByRole('combobox')).toBeInTheDocument();

      expect(await screen.findByTestId('publicationsList')).toBeInTheDocument();
      expect(screen.getByText('Publication 1')).toBeInTheDocument();
      expect(screen.getByText('Publication 2')).toBeInTheDocument();
    });
    test('renders navigation with releases tab active', async () => {
      render(<SearchDataPage />);

      const nav = await screen.findByRole('navigation', {
        name: 'Search mode',
      });

      const links = within(nav).getAllByRole('link');

      expect(links[0]).toHaveAttribute('aria-current', 'page');
      expect(links[1]).not.toHaveAttribute('aria-current');
    });

    test('does not show data-specific filters', async () => {
      render(<SearchDataPage />);

      await screen.findByRole('heading', { name: 'Filter and sort' });

      expect(
        screen.queryByRole('group', { name: 'Filter by Geographic Level' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('group', { name: 'Show latest or all releases' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('group', { name: 'Type of data' }),
      ).not.toBeInTheDocument();
    });
  });
});
