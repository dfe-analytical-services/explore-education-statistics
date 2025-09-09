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
        name: 'All releases in this series',
      }),
    ).toHaveAttribute('href', '/find-statistics/publication-slug/releases');
  });

  test('renders correct back link', () => {
    render(
      <ReleaseUpdatesPage
        releaseVersionSummary={testReleaseVersionSummary}
        publicationSummary={testPublicationSummary}
        releaseUpdates={testReleaseUpdates}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Back',
      }),
    ).toHaveAttribute(
      'href',
      '/find-statistics/publication-slug/release-slug?redesign=true',
    ); // TODO EES-6449 remove redesign query param
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
  });
});
