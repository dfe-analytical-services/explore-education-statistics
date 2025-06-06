import FindStatisticsPage from '@frontend/modules/find-statistics/FindStatisticsPage';
import { Paging } from '@common/services/types/pagination';
import render from '@common-test/render';
import _publicationService from '@frontend/services/azurePublicationService';
import _themeService from '@common/services/themeService';
import {
  testFacetNoResults,
  testFacetResults,
  testFacetResultsSearched,
  testFacetResultsSearchedOneResult,
} from '@frontend/modules/find-statistics/__tests__/__data__/testFacetData';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import mockRouter from 'next-router-mock';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

jest.mock('@frontend/services/azurePublicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@common/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
  odata: jest.fn(),
}));

describe('FindStatisticsPageAzure', () => {
  const testPaging: Paging = {
    page: 1,
    pageSize: 10,
    totalResults: 30,
    totalPages: 3,
  };

  beforeEach(() => {
    mockRouter.setCurrentUrl('/find-statistics');
    themeService.listThemes.mockResolvedValue(testThemeSummaries);
  });

  test('renders correctly with publications', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
      facets: testFacetResults,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      screen.getByRole('heading', { name: 'Find statistics and data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Search and browse statistical summaries and download associated data to help you understand and analyse our range of statistics.',
      ),
    ).toBeInTheDocument();

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Search publications')).toBeInTheDocument();

    const sortSelect = screen.getByLabelText('Sort by');
    const sortOptions = within(sortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toHaveTextContent('Newest');
    expect(sortOptions[0]).toHaveValue('newest');
    expect(sortOptions[0].selected).toBe(true);
    expect(sortOptions[1]).toHaveTextContent('Oldest');
    expect(sortOptions[1]).toHaveValue('oldest');
    expect(sortOptions[1].selected).toBe(false);
    expect(sortOptions[2]).toHaveTextContent('A to Z');
    expect(sortOptions[2]).toHaveValue('title');
    expect(sortOptions[2].selected).toBe(false);

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(5);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 1 (1)');
    expect(themes[1]).toHaveValue('theme-1');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveTextContent('Theme 2 (1)');
    expect(themes[2]).toHaveValue('theme-2');
    expect(themes[2].selected).toBe(false);

    expect(themes[3]).toHaveTextContent('Theme 3 (1)');
    expect(themes[3]).toHaveValue('theme-3');
    expect(themes[3].selected).toBe(false);

    expect(themes[4]).toHaveTextContent('Theme 4 (0)');
    expect(themes[4]).toHaveValue('theme-4');
    expect(themes[4].selected).toBe(false);

    const releaseTypesSelect = screen.getByLabelText('Filter by Release type');
    const releaseTypes = within(releaseTypesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(releaseTypes).toHaveLength(7);

    expect(releaseTypes[0]).toHaveTextContent('All release types');
    expect(releaseTypes[0]).toHaveValue('all');
    expect(releaseTypes[0].selected).toBe(true);

    expect(releaseTypes[1]).toHaveTextContent(
      'Accredited official statistics (1)',
    );
    expect(releaseTypes[1]).toHaveValue('AccreditedOfficialStatistics');
    expect(releaseTypes[1].selected).toBe(false);

    expect(releaseTypes[2]).toHaveTextContent('Experimental statistics (1)');
    expect(releaseTypes[2]).toHaveValue('ExperimentalStatistics');
    expect(releaseTypes[2].selected).toBe(false);

    expect(releaseTypes[3]).toHaveTextContent('Ad hoc statistics (1)');
    expect(releaseTypes[3]).toHaveValue('AdHocStatistics');
    expect(releaseTypes[3].selected).toBe(false);

    expect(releaseTypes[4]).toHaveTextContent('Official statistics (0)');
    expect(releaseTypes[4]).toHaveValue('OfficialStatistics');
    expect(releaseTypes[4].selected).toBe(false);

    expect(releaseTypes[5]).toHaveTextContent(
      'Official statistics in development (0)',
    );
    expect(releaseTypes[5]).toHaveValue('OfficialStatisticsInDevelopment');
    expect(releaseTypes[5].selected).toBe(false);

    expect(releaseTypes[6]).toHaveTextContent('Management information (0)');
    expect(releaseTypes[6]).toHaveValue('ManagementInformation');
    expect(releaseTypes[6].selected).toBe(false);

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(3);

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 1' }),
    ).toBeInTheDocument();
    expect(
      publicationsList.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();
    expect(
      publicationsList.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();

    const pagination = within(
      screen.getByRole('navigation', { name: 'Pagination' }),
    );
    expect(pagination.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
      'href',
      '?page=1',
    );
    expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '?page=2',
    );
    expect(pagination.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
      'href',
      '?page=3',
    );
    expect(pagination.getByRole('link', { name: 'Next page' })).toHaveAttribute(
      'href',
      '?page=2',
    );

    expect(
      screen.queryByText('No data currently published.'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with no publications', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: [],
      paging: {
        ...testPaging,
        totalPages: 0,
        totalResults: 0,
      },
      facets: {},
    });
    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('No data currently published.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination navigation' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when searched and has results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?search=Find+me');
    publicationService.listPublications.mockResolvedValue({
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      facets: testFacetResultsSearched,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search: Find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();

    expect(
      await publicationsList.findByText('find me highlight'),
    ).toBeVisible();

    const searchHighlight = publicationsList.getByTestId('search-highlight');

    expect(
      within(searchHighlight).getByText('find me highlight', { exact: false })
        .nodeName,
    ).toEqual('EM');

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('There are no matching results.'),
    ).not.toBeInTheDocument();

    const sortSelect = screen.getByLabelText('Sort by');
    const sortOptions = within(sortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(sortOptions).toHaveLength(4);
    expect(sortOptions[3]).toHaveTextContent('Relevance');
    expect(sortOptions[3]).toHaveValue('relevance');
    expect(sortOptions[3].selected).toBe(false);

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(5);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 2 (1)');
    expect(themes[1]).toHaveValue('theme-2');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveTextContent('Theme 3 (1)');
    expect(themes[2]).toHaveValue('theme-3');
    expect(themes[2].selected).toBe(false);

    expect(themes[3]).toHaveTextContent('Theme 1 (0)');
    expect(themes[3]).toHaveValue('theme-1');
    expect(themes[3].selected).toBe(false);

    expect(themes[4]).toHaveTextContent('Theme 4 (0)');
    expect(themes[4]).toHaveValue('theme-4');
    expect(themes[4].selected).toBe(false);
  });

  test('renders correctly when searched and has no results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?search=Cannot+find+me');
    publicationService.listPublications.mockResolvedValue({
      results: [],
      paging: { ...testPaging, totalPages: 0, totalResults: 0 },
      facets: testFacetNoResults,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('0 results, 0 pages, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Search: Cannot find me',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by theme and has results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?themeId=theme-2');
    publicationService.listPublications.mockResolvedValue({
      results: [
        { ...testPublications[1], highlightContent: undefined },
        testPublications[2],
      ],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      facets: testFacetResultsSearched,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('There are no matching results.'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by theme and has no results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?themeId=theme-2');
    publicationService.listPublications.mockResolvedValue({
      results: [],
      paging: { ...testPaging, totalPages: 0, totalResults: 0 },
      facets: testFacetNoResults,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(
      screen.getByText('0 results, 0 pages, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by release type and has results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?releaseType=AdHocStatistics');
    publicationService.listPublications.mockResolvedValue({
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      facets: testFacetResultsSearched,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('There are no matching results.'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by release type and has no results', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=ExperimentalStatistics',
    );
    publicationService.listPublications.mockResolvedValue({
      results: [],
      paging: { ...testPaging, totalPages: 0, totalResults: 0 },
      facets: testFacetNoResults,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Experimental statistics',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly with all filters and search', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=AdHocStatistics&search=find+me&themeId=theme-1',
    );
    publicationService.listPublications.mockResolvedValue({
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      facets: testFacetResultsSearched,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search: find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 1' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();

    expect(
      publicationsList.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();
  });

  test('renders the desktop filters', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
      facets: testFacetResults,
    });

    render(<FindStatisticsPage useAzureSearch />);

    expect(screen.getByLabelText('Filter by Theme')).toBeInTheDocument();
    expect(screen.getByLabelText('Filter by Release type')).toBeInTheDocument();

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test('renders the mobile filters', async () => {
    mockIsMedia = true;
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
      facets: testFacetResults,
    });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    await user.click(screen.getByRole('button', { name: 'Filter results' }));

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByLabelText('Filter by Theme')).toBeInTheDocument();
    expect(modal.getByLabelText('Filter by Release type')).toBeInTheDocument();
    expect(
      modal.getByRole('button', { name: 'Back to results' }),
    ).toBeInTheDocument();

    mockIsMedia = false;
  });

  test('adding filters', async () => {
    mockRouter.setCurrentUrl('/find-statistics');

    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        facets: testFacetResultsSearched,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        facets: testFacetResultsSearchedOneResult,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);

    // Add release type filter
    await user.selectOptions(screen.getByLabelText('Filter by Release type'), [
      'AdHocStatistics',
    ]);

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { releaseType: 'AdHocStatistics' },
    });

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(2);

    // Add theme filter
    await user.selectOptions(screen.getByLabelText('Filter by Theme'), [
      'theme-1',
    ]);

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: {
        releaseType: 'AdHocStatistics',
        themeId: 'theme-1',
      },
    });

    expect(
      await screen.findByText('1 result, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);
  });

  test('removing filters', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=AccreditedOfficialStatistics&themeId=theme-2',
    );

    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        facets: testFacetResultsSearchedOneResult,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        facets: testFacetResultsSearched,
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('1 result, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Accredited official statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);

    const releaseTypesSelect = screen.getByLabelText('Filter by Release type');
    const releaseTypes = within(releaseTypesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(releaseTypes[0]).toHaveTextContent('All release types');
    expect(releaseTypes[0].selected).toBe(false);

    expect(releaseTypes[1]).toHaveTextContent(
      'Accredited official statistics (1)',
    );
    expect(releaseTypes[1].selected).toBe(true);

    expect(releaseTypes[2]).toHaveTextContent('Official statistics (0)');
    expect(releaseTypes[2].selected).toBe(false);

    // remove release type filter
    await user.click(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Accredited official statistics',
      }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { themeId: 'theme-2' },
    });

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: Release type Accredited official statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(2);

    const updatedReleaseTypeSelect = screen.getByLabelText(
      'Filter by Release type',
    );
    const updatedReleaseTypes = within(updatedReleaseTypeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(updatedReleaseTypes[0]).toHaveTextContent('All release types');
    expect(updatedReleaseTypes[0].selected).toBe(true);

    expect(updatedReleaseTypes[1]).toHaveTextContent(
      'Accredited official statistics (1)',
    );
    expect(updatedReleaseTypes[1].selected).toBe(false);

    expect(updatedReleaseTypes[2]).toHaveTextContent(
      'Experimental statistics (1)',
    );
    expect(updatedReleaseTypes[2]).toHaveValue('ExperimentalStatistics');
    expect(updatedReleaseTypes[2].selected).toBe(false);

    // Remove theme filter
    await user.click(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
    });

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: Release type Accredited official statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme: Theme 2' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reset filters' }),
    ).not.toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);
  });

  test('searching', async () => {
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        facets: testFacetResultsSearched,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);

    await user.type(screen.getByLabelText('Search publications'), 'Find me');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { search: 'Find me', sortBy: 'newest' },
    });

    expect(await screen.findByText('2 results')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search: Find me' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
  });

  test('removing search', async () => {
    mockRouter.setCurrentUrl('/find-statistics?search=Find+me');

    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
        facets: testFacetResultsSearchedOneResult,
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('1 result, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Search: Find me',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);

    await user.click(
      screen.getByRole('button', {
        name: 'Remove filter: Search: Find me',
      }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { sortBy: 'newest' },
    });

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Remove filter: Search: Find me' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reset filters' }),
    ).not.toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);
  });

  test('sorting', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
      facets: testFacetResults,
    });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(await screen.findByText('30 results')).toBeInTheDocument();

    const sortSelect = screen.getByLabelText('Sort by');
    const sortOptions = within(sortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toHaveTextContent('Newest');
    expect(sortOptions[0]).toHaveValue('newest');
    expect(sortOptions[0].selected).toBe(true);
    expect(sortOptions[1]).toHaveTextContent('Oldest');
    expect(sortOptions[1]).toHaveValue('oldest');
    expect(sortOptions[1].selected).toBe(false);
    expect(sortOptions[2]).toHaveTextContent('A to Z');
    expect(sortOptions[2]).toHaveValue('title');
    expect(sortOptions[2].selected).toBe(false);

    await user.selectOptions(screen.getByLabelText('Sort by'), ['A to Z']);

    await waitFor(() => {
      expect(mockRouter).toMatchObject({
        pathname: '/find-statistics',
        query: { sortBy: 'title' },
      });
    });

    expect(sortOptions[0]).toHaveTextContent('Newest');
    expect(sortOptions[0]).toHaveValue('newest');
    expect(sortOptions[0].selected).toBe(false);
    expect(sortOptions[1]).toHaveTextContent('Oldest');
    expect(sortOptions[1]).toHaveValue('oldest');
    expect(sortOptions[1].selected).toBe(false);
    expect(sortOptions[2]).toHaveTextContent('A to Z');
    expect(sortOptions[2]).toHaveValue('title');
    expect(sortOptions[2].selected).toBe(true);
  });

  test('adds the relevance sort option when applying a search filter', async () => {
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        facets: testFacetResultsSearched,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(await screen.findByText('30 results')).toBeInTheDocument();

    const sortSelect = screen.getByLabelText('Sort by');
    const sortOptions = within(sortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toHaveTextContent('Newest');
    expect(sortOptions[0].selected).toBe(true);
    expect(sortOptions[1]).toHaveTextContent('Oldest');
    expect(sortOptions[1].selected).toBe(false);
    expect(sortOptions[2]).toHaveTextContent('A to Z');
    expect(sortOptions[2].selected).toBe(false);

    await user.type(screen.getByLabelText('Search publications'), 'Find me');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    expect(await screen.findByText('2 results')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search: Find me' }),
    ).toBeInTheDocument();

    const updatedSortSelect = screen.getByLabelText('Sort by');
    const updatedSortOptions = within(updatedSortSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(updatedSortOptions).toHaveLength(4);
    expect(updatedSortOptions[3]).toHaveTextContent('Relevance');
    expect(updatedSortOptions[3]).toHaveValue('relevance');
    expect(updatedSortOptions[3].selected).toBe(false);
  });

  test('Reset filters', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=AdHocStatistics&search=find+me&themeId=theme-1',
    );
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[1], testPublications[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
        facets: testFacetResultsSearched,
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
        facets: testFacetResults,
      });

    const { user } = render(<FindStatisticsPage useAzureSearch />);

    expect(
      await screen.findByText('2 results, page 1 of 1, filtered by:'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search: find me' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme: Theme 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    await user.click(screen.getByRole('button', { name: 'Reset filters' }));

    expect(
      await screen.findByText(
        '30 results, page 1 of 3, showing all publications',
      ),
    ).toBeInTheDocument();

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: {},
    });
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Search: find me' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: Release type: Ad hoc statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme: Theme 1' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reset filters' }),
    ).not.toBeInTheDocument();
  });
});
