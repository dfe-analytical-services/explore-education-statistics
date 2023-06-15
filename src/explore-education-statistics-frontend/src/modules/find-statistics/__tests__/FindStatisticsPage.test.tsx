import FindStatisticsPage from '@frontend/modules/find-statistics/FindStatisticsPage';
import { Paging } from '@common/services/types/pagination';
import render from '@common-test/render';
import _publicationService from '@common/services/publicationService';
import _themeService from '@common/services/themeService';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { screen, waitFor, within } from '@testing-library/react';
import mockRouter from 'next-router-mock';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: false,
    };
  },
}));

jest.mock('@common/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@common/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;

describe('FindStatisticsPage', () => {
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

  test('renders related information links', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
    });

    render(<FindStatisticsPage />);

    const relatedInformationNav = screen.getByRole('navigation', {
      name: 'Related information',
    });

    const relatedInformationLinks = within(relatedInformationNav).getAllByRole(
      'link',
    );

    expect(relatedInformationLinks).toHaveLength(3);
    expect(relatedInformationLinks[0]).toHaveTextContent(
      'Education statistics: data catalogue',
    );
    expect(relatedInformationLinks[1]).toHaveTextContent(
      'Education statistics: methodology',
    );
    expect(relatedInformationLinks[2]).toHaveTextContent(
      'Education statistics: glossary',
    );
  });

  test('renders correctly with publications', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
    });

    render(<FindStatisticsPage />);

    expect(
      screen.getByRole('heading', { name: 'Find statistics and data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Search and browse statistical summaries and download associated data to help you understand and analyse our range of statistics.',
      ),
    ).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: '30 results' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();

    const sortGroup = within(
      screen.getByRole('group', { name: 'Sort results' }),
    );
    const sortOptions = sortGroup.getAllByRole('radio');
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(sortOptions[0]).toBeChecked();
    expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(sortOptions[1]).not.toBeChecked();
    expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(sortOptions[2]).not.toBeChecked();

    expect(screen.getByLabelText('Search')).toBeInTheDocument();

    const themeFilterGroup = within(
      screen.getByRole('group', { name: 'Filter by theme' }),
    );
    const themeOptions = themeFilterGroup.getAllByRole('radio');
    expect(themeOptions).toHaveLength(5);
    expect(themeOptions[0]).toEqual(
      themeFilterGroup.getByLabelText('All themes'),
    );
    expect(themeOptions[0]).toBeChecked();
    expect(themeOptions[1]).toEqual(themeFilterGroup.getByLabelText('Theme 1'));
    expect(themeOptions[1]).not.toBeChecked();
    expect(themeOptions[2]).toEqual(themeFilterGroup.getByLabelText('Theme 2'));
    expect(themeOptions[2]).not.toBeChecked();
    expect(themeOptions[3]).toEqual(themeFilterGroup.getByLabelText('Theme 3'));
    expect(themeOptions[3]).not.toBeChecked();
    expect(themeOptions[4]).toEqual(themeFilterGroup.getByLabelText('Theme 4'));
    expect(themeOptions[4]).not.toBeChecked();

    expect(
      screen.getByRole('button', { name: 'Release type' }),
    ).toBeInTheDocument();

    const releaseTypeFilterGroup = within(
      screen.getByRole('group', { name: 'Filter by release type' }),
    );
    const releaseTypeOptions = releaseTypeFilterGroup.getAllByRole('radio');
    expect(releaseTypeOptions).toHaveLength(6);
    expect(releaseTypeOptions[0]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Show all'),
    );
    expect(releaseTypeOptions[0]).toBeChecked();
    expect(releaseTypeOptions[1]).toEqual(
      releaseTypeFilterGroup.getByLabelText('National statistics'),
    );
    expect(releaseTypeOptions[1]).not.toBeChecked();
    expect(releaseTypeOptions[2]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Official statistics'),
    );
    expect(releaseTypeOptions[2]).not.toBeChecked();
    expect(releaseTypeOptions[3]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Experimental statistics'),
    );
    expect(releaseTypeOptions[3]).not.toBeChecked();
    expect(releaseTypeOptions[4]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Ad hoc statistics'),
    );
    expect(releaseTypeOptions[4]).not.toBeChecked();
    expect(releaseTypeOptions[5]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Management information'),
    );
    expect(releaseTypeOptions[5]).not.toBeChecked();

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
      '/find-statistics?page=1',
    );
    expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '/find-statistics?page=2',
    );
    expect(pagination.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
      'href',
      '/find-statistics?page=3',
    );
    expect(pagination.getByRole('link', { name: 'Next' })).toHaveAttribute(
      'href',
      '/find-statistics?page=2',
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
    });
    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(
        screen.getByText('No data currently published.'),
      ).toBeInTheDocument();
    });

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
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
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

  test('renders correctly when searched and has no results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?search=Cannot+find+me');
    publicationService.listPublications.mockResolvedValue({
      results: [],
      paging: { ...testPaging, totalPages: 0, totalResults: 0 },
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('0 results')).toBeInTheDocument();
    });

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Cannot find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by theme and has results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?themeId=theme-2');
    publicationService.listPublications.mockResolvedValue({
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
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
    });

    render(<FindStatisticsPage />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: '0 results' }),
    ).toBeInTheDocument();

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by release type and has results', async () => {
    mockRouter.setCurrentUrl('/find-statistics?releaseType=AdHocStatistics');
    publicationService.listPublications.mockResolvedValue({
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Ad hoc statistics' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
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
    });

    render(<FindStatisticsPage />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Experimental statistics',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
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
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Ad hoc statistics' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 1' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
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

  test('adding filters', async () => {
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      })
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);

    // Add release type filter
    userEvent.click(screen.getByLabelText('Ad hoc statistics'));

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { releaseType: 'AdHocStatistics' },
    });

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Ad hoc statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(2);

    // Add theme filter
    userEvent.click(screen.getByLabelText('Theme 1'));

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { releaseType: 'AdHocStatistics', themeId: 'theme-1' },
    });

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Ad hoc statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);
  });

  test('removing filters', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=NationalStatistics&themeId=theme-2',
    );

    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: National statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);

    // remove release type filter
    userEvent.click(
      screen.getByRole('button', {
        name: 'Remove filter: National statistics',
      }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { themeId: 'theme-2' },
    });

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: National statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(2);

    // Remove theme filter
    userEvent.click(
      screen.getByRole('button', { name: 'Remove filter: Theme 2' }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: {},
    });

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: National statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme 2' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Clear all filters' }),
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
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);

    userEvent.type(screen.getByLabelText('Search'), 'Find me');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { search: 'Find me', sortBy: 'relevance' },
    });

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Remove filter: Find me' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();
  });

  test('removing search', async () => {
    mockRouter.setCurrentUrl('/find-statistics?search=Find+me');

    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Find me',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);

    userEvent.click(
      screen.getByRole('button', {
        name: 'Remove filter: Find me',
      }),
    );

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { sortBy: 'newest' },
    });

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Find me' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Clear all filters' }),
    ).not.toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);
  });

  test('sorting', async () => {
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    const sortGroup = within(
      screen.getByRole('group', { name: 'Sort results' }),
    );
    const sortOptions = sortGroup.getAllByRole('radio');
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(sortOptions[0]).toBeChecked();
    expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(sortOptions[1]).not.toBeChecked();
    expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(sortOptions[2]).not.toBeChecked();

    userEvent.click(sortGroup.getByLabelText('A to Z'));

    await waitFor(() => {
      expect(mockRouter).toMatchObject({
        pathname: '/find-statistics',
        query: { sortBy: 'title' },
      });
    });

    expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(sortOptions[0]).not.toBeChecked();
    expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(sortOptions[1]).not.toBeChecked();
    expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(sortOptions[2]).toBeChecked();
  });

  test('sorts by relevance when have a search filter', async () => {
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(3);

    const sortGroup = within(
      screen.getByRole('group', { name: 'Sort results' }),
    );
    const sortOptions = sortGroup.getAllByRole('radio');
    expect(sortOptions).toHaveLength(3);
    expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(sortOptions[0]).toBeChecked();
    expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(sortOptions[1]).not.toBeChecked();
    expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(sortOptions[2]).not.toBeChecked();

    userEvent.type(screen.getByLabelText('Search'), 'Find me');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Remove filter: Find me' }),
    ).toBeInTheDocument();

    const updatedSortOptions = sortGroup.getAllByRole('radio');
    expect(updatedSortOptions).toHaveLength(4);
    expect(updatedSortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(updatedSortOptions[0]).not.toBeChecked();
    expect(updatedSortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(updatedSortOptions[1]).not.toBeChecked();
    expect(updatedSortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(updatedSortOptions[2]).not.toBeChecked();
    expect(updatedSortOptions[3]).toEqual(
      sortGroup.getByLabelText('Relevance'),
    );
    expect(updatedSortOptions[3]).toBeChecked();
  });

  test('reverts the sorting to `newest` when the search filter is removed', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?search=Find+me&sortBy=relevance',
    );
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[0]],
        paging: { ...testPaging, totalPages: 1, totalResults: 1 },
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    const sortGroup = within(
      screen.getByRole('group', { name: 'Sort results' }),
    );
    const sortOptions = sortGroup.getAllByRole('radio');
    expect(sortOptions).toHaveLength(4);
    expect(sortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(sortOptions[0]).not.toBeChecked();
    expect(sortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(sortOptions[1]).not.toBeChecked();
    expect(sortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(sortOptions[2]).not.toBeChecked();
    expect(sortOptions[3]).toEqual(sortGroup.getByLabelText('Relevance'));
    expect(sortOptions[3]).toBeChecked();

    userEvent.click(
      screen.getByRole('button', {
        name: 'Remove filter: Find me',
      }),
    );

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    const updatedSortOptions = sortGroup.getAllByRole('radio');
    expect(updatedSortOptions).toHaveLength(3);
    expect(updatedSortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(updatedSortOptions[0]).toBeChecked();
    expect(updatedSortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(updatedSortOptions[1]).not.toBeChecked();
    expect(updatedSortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(updatedSortOptions[2]).not.toBeChecked();
  });

  test('clear all filters', async () => {
    mockRouter.setCurrentUrl(
      '/find-statistics?releaseType=AdHocStatistics&search=find+me&themeId=theme-1',
    );
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: [testPublications[1], testPublications[2]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: find me' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Ad hoc statistics' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    userEvent.click(screen.getByRole('button', { name: 'Clear all filters' }));

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: {},
    });
    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: find me' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: Ad hoc statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme 1' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Clear all filters' }),
    ).not.toBeInTheDocument();
  });
});
