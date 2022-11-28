import { Paging } from '@common/services/types/pagination';
import FindStatisticsPageNew from '@frontend/modules/find-statistics/FindStatisticsPageNew';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: false,
    };
  },
}));

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

    expect(
      screen.getByRole('heading', { name: 'Publication 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Publication 3' }),
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
      />,
    );

    expect(
      screen.getByText('No data currently published.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination navigation' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when searched and has results', () => {
    render(
      <FindStatisticsPageNew
        paging={{ ...testPaging, totalPages: 1, totalResults: 1 }}
        publications={[testPublications[1], testPublications[2]]}
        searchTerm="Find me"
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

    expect(
      screen.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Publication 3' }),
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
        searchTerm="Can't find me"
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
  });
});
