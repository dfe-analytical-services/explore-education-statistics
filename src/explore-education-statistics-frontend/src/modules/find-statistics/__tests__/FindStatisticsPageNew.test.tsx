import { Paging } from '@common/services/types/pagination';
import FindStatisticsPageNew from '@frontend/modules/find-statistics/FindStatisticsPageNew';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { testThemeSummaries } from '@frontend/modules/find-statistics/__tests__/__data__/testThemeData';
import { render, screen, within } from '@testing-library/react';

import React from 'react';

jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: false,
    };
  },
}));

jest.mock('@common/services/publicationService');
describe('FindStatisticsPageNew', () => {
  const testPaging: Paging = {
    page: 1,
    pageSize: 10,
    totalResults: 30,
    totalPages: 3,
  };

  test('renders correctly with publications', async () => {
    render(
      <FindStatisticsPageNew
        paging={testPaging}
        publications={testPublications}
        query={{}}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Find statistics and data' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Search and browse statistical summaries and download associated data to help you understand and analyse our range of statistics.',
      ),
    ).toBeInTheDocument();

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
      releaseTypeFilterGroup.getByLabelText('Ad hoc statistics'),
    );
    expect(releaseTypeOptions[1]).not.toBeChecked();
    expect(releaseTypeOptions[2]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Experimental statistics'),
    );
    expect(releaseTypeOptions[2]).not.toBeChecked();
    expect(releaseTypeOptions[3]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Management information'),
    );
    expect(releaseTypeOptions[3]).not.toBeChecked();
    expect(releaseTypeOptions[4]).toEqual(
      releaseTypeFilterGroup.getByLabelText('National statistics'),
    );
    expect(releaseTypeOptions[4]).not.toBeChecked();
    expect(releaseTypeOptions[5]).toEqual(
      releaseTypeFilterGroup.getByLabelText('Official statistics'),
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
      '/?page=1',
    );
    expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '/?page=2',
    );
    expect(pagination.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
      'href',
      '/?page=3',
    );
    expect(pagination.getByRole('link', { name: 'Next' })).toHaveAttribute(
      'href',
      '/?page=2',
    );

    expect(
      screen.queryByText('No data currently published.'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with no publications', async () => {
    render(
      <FindStatisticsPageNew
        paging={{
          ...testPaging,
          totalPages: 0,
          totalResults: 0,
        }}
        publications={[]}
        query={{}}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByText('No data currently published.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination navigation' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when searched and has results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 1, totalResults: 1 }}
        publications={[testPublications[1], testPublications[2]]}
        query={{ search: 'Find me' }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '1 result' }),
    ).toBeInTheDocument();

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

  test('renders correctly when searched and has no results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 0, totalResults: 0 }}
        publications={[]}
        query={{ search: "Can't find me" }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '0 results' }),
    ).toBeInTheDocument();

    expect(screen.getByText('0 pages, filtered by:')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: "Remove filter: Can't find me" }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Clear all filters' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by theme and has results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 1, totalResults: 1 }}
        publications={[testPublications[1], testPublications[2]]}
        query={{ themeId: 'theme-2' }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '1 result' }),
    ).toBeInTheDocument();

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

  test('renders correctly when filtered by theme and has no results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 0, totalResults: 0 }}
        publications={[]}
        query={{ themeId: 'theme-2' }}
        themes={testThemeSummaries}
      />,
    );

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

    expect(
      screen.getByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly when filtered by release type and has results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 1, totalResults: 1 }}
        publications={[testPublications[1], testPublications[2]]}
        query={{ releaseType: 'AdHocStatistics' }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '1 result' }),
    ).toBeInTheDocument();

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

  test('renders correctly when filtered by theme and has no results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 0, totalResults: 0 }}
        publications={[]}
        query={{ releaseType: 'ExperimentalStatistics' }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '0 results' }),
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

    expect(
      screen.getByText('There are no matching results.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('publicationsList')).not.toBeInTheDocument();
  });

  test('renders correctly with all filters and search', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 1, totalResults: 1 }}
        publications={[testPublications[1], testPublications[2]]}
        query={{
          releaseType: 'AdHocStatistics',
          search: 'find me',
          themeId: 'theme-1',
        }}
        themes={testThemeSummaries}
      />,
    );

    expect(
      screen.getByRole('heading', { name: '1 result' }),
    ).toBeInTheDocument();

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

    expect(
      screen.queryByText('There are no matching results.'),
    ).not.toBeInTheDocument();
  });
});
