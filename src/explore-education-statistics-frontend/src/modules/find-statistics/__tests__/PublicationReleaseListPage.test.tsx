import { PublicationReleaseSeriesItem } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { render, screen, within } from '@testing-library/react';
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
      pageSize: 10,
      totalResults: 3,
      totalPages: 1,
    },
  };
  test('renders next publish date', () => {
    render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        releases={testReleases}
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
        releases={testReleases}
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
      '/find-statistics/publication-slug/test-release-slug-3?redesign=true', // TODO EES-6449 remove query param when live
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
      '/find-statistics/publication-slug/test-release-slug-2?redesign=true', // TODO EES-6449 remove query param when live
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
  });

  test('renders pagination when multiple pages', () => {
    const testReleasesMultiPage: PaginatedList<PublicationReleaseSeriesItem> = {
      ...testReleases,
      paging: {
        page: 2,
        pageSize: 1,
        totalResults: 3,
        totalPages: 3,
      },
    };
    render(
      <PublicationReleaseListPage
        publicationSummary={testPublicationSummary}
        releases={testReleasesMultiPage}
      />,
    );

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
      'href',
      '?page=1',
    );
    expect(screen.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(screen.getByRole('link', { name: 'Page 3' })).toHaveAttribute(
      'href',
      '?page=3',
    );
  });
});
