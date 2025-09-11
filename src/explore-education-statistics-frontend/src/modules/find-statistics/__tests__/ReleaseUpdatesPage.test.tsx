import { ReleaseUpdate } from '@common/services/releaseUpdatesService';
import { PaginatedList } from '@common/services/types/pagination';
import { render, screen, within } from '@testing-library/react';
import React from 'react';
import ReleaseUpdatesPage from '../ReleaseUpdatesPage';
import {
  testPublicationSummary,
  testReleaseVersionSummary,
} from './__data__/testReleaseData';

describe('Release updates page', () => {
  const testReleaseUpdates: PaginatedList<ReleaseUpdate> = {
    results: [
      {
        date: '2025-09-04T16:42:37',
        summary: `Update 3`,
      },
      {
        date: '2025-07-09T11:30:12',
        summary: 'Update 2',
      },
      {
        date: '2024-12-30T09:11:59',
        summary: 'Update 1',
      },
    ],
    paging: {
      page: 1,
      pageSize: 10,
      totalResults: 3,
      totalPages: 1,
    },
  };
  test('renders link to all releases', () => {
    render(
      <ReleaseUpdatesPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        releaseUpdates={testReleaseUpdates}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Release home',
      }),
    ).toHaveAttribute('href', '/find-statistics/publication-slug/release-slug');
  });

  test('renders table data correctly', () => {
    render(
      <ReleaseUpdatesPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        releaseUpdates={testReleaseUpdates}
      />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(4); // including header row
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(
      within(row1Cells[0]).getByText('4 September 2025'),
    ).toBeInTheDocument();
    expect(within(row1Cells[1]).getByText('Update 3')).toBeInTheDocument();
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('9 July 2025')).toBeInTheDocument();
    expect(within(row2Cells[1]).getByText('Update 2')).toBeInTheDocument();
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[0]).getByText('30 December 2024'),
    ).toBeInTheDocument();
    expect(within(row3Cells[1]).getByText('Update 1')).toBeInTheDocument();
  });

  test('renders pagination when multiple pages', () => {
    const testReleaseUpdatesMultiPage: PaginatedList<ReleaseUpdate> = {
      ...testReleaseUpdates,
      paging: {
        page: 2,
        pageSize: 1,
        totalResults: 3,
        totalPages: 3,
      },
    };
    render(
      <ReleaseUpdatesPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        releaseUpdates={testReleaseUpdatesMultiPage}
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
