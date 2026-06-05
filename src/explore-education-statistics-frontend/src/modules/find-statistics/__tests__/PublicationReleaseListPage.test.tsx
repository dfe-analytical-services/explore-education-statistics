import { PublicationReleaseSeriesItem } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import PublicationReleaseListPage from '../PublicationReleaseListPage';
import { testPublicationSummary } from './__data__/testReleaseData';

describe('PublicationReleaseListPage', () => {
  const testReleases: PaginatedList<PublicationReleaseSeriesItem> = {
    results: [
      {
        releaseId: 'test-release-id-3',
        slug: 'test-release-slug-3',
        title: 'Test Release Title 3 - Final',
        yearTitle: '2021/22',
        coverageTitle: 'Academic Year',
        label: 'Final',
        published: '2025-08-10T09:30:00+01:00',
        lastUpdated: '2025-08-11T14:30:00+01:00',
        isLatestRelease: true,
      },
      {
        releaseId: 'test-release-id-2',
        slug: 'test-release-slug-2',
        title: 'Test Release Title 2',
        yearTitle: '2021/22',
        coverageTitle: 'Academic Year',
        published: '2025-07-01T09:30:00+01:00',
        lastUpdated: '2025-07-11T14:30:00+01:00',
        isLatestRelease: false,
      },
      {
        title: 'Legacy title',
        url: 'https://example.com/legacy-title',
      },
    ],
    paging: {
      page: 1,
      pageSize: 3,
      totalResults: 3,
      totalPages: 1,
    },
  };

  const testReleasesLonger: PaginatedList<PublicationReleaseSeriesItem> = {
    results: [
      ...testReleases.results,
      {
        title: 'Legacy title 4',
        url: 'https://example.com/legacy-title-4',
      },
      {
        title: 'Legacy title 5',
        url: 'https://example.com/legacy-title-5',
      },
      {
        title: 'Legacy title 6',
        url: 'https://example.com/legacy-title-6',
      },
      {
        title: 'Legacy title 7',
        url: 'https://example.com/legacy-title-7',
      },
      {
        title: 'Legacy title 8',
        url: 'https://example.com/legacy-title-8',
      },
      {
        title: 'Legacy title 9',
        url: 'https://example.com/legacy-title-9',
      },
      {
        title: 'Legacy title 10',
        url: 'https://example.com/legacy-title-10',
      },
      {
        title: 'Legacy title 11',
        url: 'https://example.com/legacy-title-11',
      },
      {
        title: 'Legacy title 12',
        url: 'https://example.com/legacy-title-12',
      },
      {
        title: 'Legacy title 13',
        url: 'https://example.com/legacy-title-13',
      },
      {
        title: 'Legacy title 14',
        url: 'https://example.com/legacy-title-14',
      },
      {
        title: 'Legacy title 15',
        url: 'https://example.com/legacy-title-15',
      },
      {
        title: 'Legacy title 16',
        url: 'https://example.com/legacy-title-16',
      },
      {
        title: 'Legacy title 17',
        url: 'https://example.com/legacy-title-17',
      },
      {
        title: 'Legacy title 18',
        url: 'https://example.com/legacy-title-18',
      },
      {
        title: 'Legacy title 19',
        url: 'https://example.com/legacy-title-19',
      },
      {
        title: 'Legacy title 20',
        url: 'https://example.com/legacy-title-20',
      },
      {
        title: 'Legacy title 21',
        url: 'https://example.com/legacy-title-21',
      },
      {
        title: 'Legacy title 22',
        url: 'https://example.com/legacy-title-22',
      },
      {
        title: 'Legacy title 23',
        url: 'https://example.com/legacy-title-23',
      },
      {
        title: 'Legacy title 24',
        url: 'https://example.com/legacy-title-24',
      },
      {
        title: 'Legacy title 25',
        url: 'https://example.com/legacy-title-25',
      },
      {
        title: 'Legacy title 26',
        url: 'https://example.com/legacy-title-26',
      },
    ],
    paging: {
      page: 1,
      pageSize: 26,
      totalResults: 26,
      totalPages: 1,
    },
  };

  test('renders next publish date', () => {
    render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        allReleases={testReleases}
      />,
    );

    const nextReleaseDate = screen.getByTestId('next-release-date');
    expect(nextReleaseDate).toBeInTheDocument();
    expect(within(nextReleaseDate).getByText('March 2026')).toBeInTheDocument();
  });

  test('renders table data correctly', () => {
    render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        allReleases={testReleases}
      />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(4); // including header row
    const row1Cells = within(rows[1]).getAllByRole('cell');
    const row1Link = within(row1Cells[0]).getByRole('link', {
      name: 'Test Release Title 3 - Final',
    });
    expect(row1Link).toBeInTheDocument();
    expect(row1Link).toHaveAttribute(
      'href',
      '/find-statistics/publication-slug/test-release-slug-3',
    );
    expect(
      within(row1Cells[1]).getByText('10 August 2025'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('Latest release'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByText('11 August 2025'),
    ).toBeInTheDocument();
    const row2Cells = within(rows[2]).getAllByRole('cell');
    const row2link = within(row2Cells[0]).getByRole('link', {
      name: 'Test Release Title 2',
    });
    expect(row2link).toBeInTheDocument();
    expect(row2link).toHaveAttribute(
      'href',
      '/find-statistics/publication-slug/test-release-slug-2',
    );
    expect(within(row2Cells[1]).getByText('1 July 2025')).toBeInTheDocument();
    expect(
      within(row2Cells[1]).queryByText('Latest release'),
    ).not.toBeInTheDocument();
    expect(within(row2Cells[2]).getByText('11 July 2025')).toBeInTheDocument();
    const row3Cells = within(rows[3]).getAllByRole('cell');
    const row3link = within(row3Cells[0]).getByRole('link', {
      name: 'Legacy title',
    });
    expect(row3link).toBeInTheDocument();
    expect(row3link).toHaveAttribute(
      'href',
      'https://example.com/legacy-title',
    );
    expect(within(row3Cells[1]).getByText('Not available')).toBeInTheDocument();
    expect(within(row3Cells[2]).getByText('Not available')).toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByLabelText('Number of results per page'),
    ).not.toBeInTheDocument();
  });

  test('pagination', async () => {
    const { user } = render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        allReleases={testReleasesLonger}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText('Number of results per page'),
    ).toBeInTheDocument();

    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(
      26,
    ); // including header row

    await user.click(screen.getByRole('button', { name: 'Page 2' }));

    await waitFor(() =>
      expect(screen.queryByText('10 August 2025')).not.toBeInTheDocument(),
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(2);
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Legacy title 26');

    expect(
      screen.getByRole('button', { name: 'Previous page' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Next page' }),
    ).not.toBeInTheDocument();

    // Check pagination window functionality
    await user.selectOptions(
      screen.getByLabelText('Number of results per page'),
      '50',
    );
    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(
      27,
    );
    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });

  test('search', async () => {
    const { user } = render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        allReleases={testReleasesLonger}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();

    const searchInput = screen.getByLabelText(/Search release periods/);
    expect(searchInput).toBeInTheDocument();

    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(
      26,
    );

    await user.type(searchInput, 'title 2');

    await waitFor(() => {
      const rows = within(screen.getByRole('table')).getAllByRole('row');
      // Should match: header + Test Release Title 2 + Legacy title 20-26 (8 data rows)
      expect(rows).toHaveLength(9);
      const row1Cells = within(rows[1]).getAllByRole('cell');
      expect(row1Cells[0]).toHaveTextContent('Test Release Title 2');
    });

    expect(screen.queryByText('Test Release Title 3')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });
});
