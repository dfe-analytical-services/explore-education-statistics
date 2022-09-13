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

    expect(
      screen.getByRole('heading', { name: 'Publication 1' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Publication 2' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Publication 3' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Publication 4' }),
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
          page: 1,
          pageSize: 10,
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
});
