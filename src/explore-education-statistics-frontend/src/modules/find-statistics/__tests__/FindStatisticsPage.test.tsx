import FindStatisticsPage from '@frontend/modules/find-statistics/FindStatisticsPage';
import { Paging } from '@common/services/types/pagination';
import render from '@common-test/render';
import _publicationService from '@common/services/publicationService';
import _themeService from '@common/services/themeService';
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

jest.mock('@common/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@common/services/themeService');
const themeService = _themeService as jest.Mocked<typeof _themeService>;

jest.mock('@azure/search-documents', () => ({
  SearchClient: jest.fn(),
  AzureKeyCredential: jest.fn(),
}));

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
    expect(relatedInformationLinks[0]).toHaveTextContent('Data catalogue');
    expect(relatedInformationLinks[1]).toHaveTextContent('Methodology');
    expect(relatedInformationLinks[2]).toHaveTextContent('Glossary');
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

    expect(screen.getByLabelText('Search publications')).toBeInTheDocument();

    const themesSelect = screen.getByLabelText('Filter by Theme');
    const themes = within(themesSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(themes).toHaveLength(5);

    expect(themes[0]).toHaveTextContent('All');
    expect(themes[0]).toHaveValue('all');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 1');
    expect(themes[1]).toHaveValue('theme-1');
    expect(themes[1].selected).toBe(false);

    expect(themes[2]).toHaveTextContent('Theme 2');
    expect(themes[2]).toHaveValue('theme-2');
    expect(themes[2].selected).toBe(false);

    expect(themes[3]).toHaveTextContent('Theme 3');
    expect(themes[3]).toHaveValue('theme-3');
    expect(themes[3].selected).toBe(false);

    expect(themes[4]).toHaveTextContent('Theme 4');
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

    expect(releaseTypes[1]).toHaveTextContent('Accredited official statistics');
    expect(releaseTypes[1]).toHaveValue('AccreditedOfficialStatistics');
    expect(releaseTypes[1].selected).toBe(false);

    expect(releaseTypes[2]).toHaveTextContent('Official statistics');
    expect(releaseTypes[2]).toHaveValue('OfficialStatistics');
    expect(releaseTypes[2].selected).toBe(false);

    expect(releaseTypes[3]).toHaveTextContent(
      'Official statistics in development',
    );
    expect(releaseTypes[3]).toHaveValue('OfficialStatisticsInDevelopment');
    expect(releaseTypes[3].selected).toBe(false);

    expect(releaseTypes[4]).toHaveTextContent('Experimental statistics');
    expect(releaseTypes[4]).toHaveValue('ExperimentalStatistics');
    expect(releaseTypes[4].selected).toBe(false);

    expect(releaseTypes[5]).toHaveTextContent('Ad hoc statistics');
    expect(releaseTypes[5]).toHaveValue('AdHocStatistics');
    expect(releaseTypes[5].selected).toBe(false);

    expect(releaseTypes[6]).toHaveTextContent('Management information');
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
      screen.getByRole('button', { name: 'Remove filter: Search Find me' }),
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
      screen.getByRole('button', {
        name: 'Remove filter: Search Cannot find me',
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
      results: [testPublications[1], testPublications[2]],
      paging: { ...testPaging, totalPages: 1, totalResults: 2 },
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 2' }),
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
    });

    render(<FindStatisticsPage />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.getByText('0 results')).toBeInTheDocument();

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 2' }),
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
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
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
    });

    render(<FindStatisticsPage />);

    expect(
      await screen.findByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Experimental statistics',
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
    });

    render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search find me' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 1' }),
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
    });

    render(<FindStatisticsPage />);

    expect(screen.getByLabelText('Filter by Theme')).toBeInTheDocument();
    expect(screen.getByLabelText('Filter by Release type')).toBeInTheDocument();

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test('renders the mobile filters', async () => {
    mockIsMedia = true;
    publicationService.listPublications.mockResolvedValue({
      results: testPublications,
      paging: testPaging,
    });

    const { user } = render(<FindStatisticsPage />);

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

    const { user } = render(<FindStatisticsPage />);

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
    await user.selectOptions(screen.getByLabelText('Filter by Release type'), [
      'AdHocStatistics',
    ]);

    expect(mockRouter).toMatchObject({
      pathname: '/find-statistics',
      query: { releaseType: 'AdHocStatistics' },
    });

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
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
      query: { releaseType: 'AdHocStatistics', themeId: 'theme-1' },
    });

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 1' }),
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
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    const { user } = render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Accredited official statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(1);

    // remove release type filter
    await user.click(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Accredited official statistics',
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
        name: 'Remove filter: Release type Accredited official statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('publicationsList')).getAllByRole('listitem'),
    ).toHaveLength(2);

    // Remove theme filter
    await user.click(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 2' }),
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
        name: 'Remove filter: Release type Accredited official statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme Theme 2' }),
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
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });

    const { user } = render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('30 results')).toBeInTheDocument();
    });

    expect(
      screen.getByText('Page 1 of 3, showing all publications'),
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

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search Find me' }),
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
      })
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      });

    const { user } = render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('1 result')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Search Find me',
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
        name: 'Remove filter: Search Find me',
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
      screen.queryByRole('button', { name: 'Remove filter: Search Find me' }),
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
    });

    const { user } = render(<FindStatisticsPage />);

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

    await user.click(sortGroup.getByLabelText('A to Z'));

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

  test('adds the relevance sort option when applying a search filter', async () => {
    publicationService.listPublications
      .mockResolvedValueOnce({
        results: testPublications,
        paging: testPaging,
      })
      .mockResolvedValueOnce({
        results: [testPublications[0], testPublications[1]],
        paging: { ...testPaging, totalPages: 1, totalResults: 2 },
      });

    const { user } = render(<FindStatisticsPage />);

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

    await user.type(screen.getByLabelText('Search publications'), 'Find me');
    await user.click(screen.getByRole('button', { name: 'Search' }));

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Remove filter: Search Find me' }),
    ).toBeInTheDocument();

    const updatedSortOptions = sortGroup.getAllByRole('radio');
    expect(updatedSortOptions).toHaveLength(4);
    expect(updatedSortOptions[0]).toEqual(sortGroup.getByLabelText('Newest'));
    expect(updatedSortOptions[0]).toBeChecked();
    expect(updatedSortOptions[1]).toEqual(sortGroup.getByLabelText('Oldest'));
    expect(updatedSortOptions[1]).not.toBeChecked();
    expect(updatedSortOptions[2]).toEqual(sortGroup.getByLabelText('A to Z'));
    expect(updatedSortOptions[2]).not.toBeChecked();
    expect(updatedSortOptions[3]).toEqual(
      sortGroup.getByLabelText('Relevance'),
    );
    expect(updatedSortOptions[3]).not.toBeChecked();
  });

  test('Reset filters', async () => {
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

    const { user } = render(<FindStatisticsPage />);

    await waitFor(() => {
      expect(screen.getByText('2 results')).toBeInTheDocument();
    });

    expect(screen.getByText('Page 1 of 1, filtered by:')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Search find me' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove filter: Theme Theme 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reset filters' }),
    ).toBeInTheDocument();

    const publicationsList = within(screen.getByTestId('publicationsList'));
    expect(publicationsList.getAllByRole('listitem')).toHaveLength(2);

    await user.click(screen.getByRole('button', { name: 'Reset filters' }));

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
      screen.queryByRole('button', { name: 'Remove filter: Search find me' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Remove filter: Release type Ad hoc statistics',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove filter: Theme Theme 1' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reset filters' }),
    ).not.toBeInTheDocument();
  });
});
