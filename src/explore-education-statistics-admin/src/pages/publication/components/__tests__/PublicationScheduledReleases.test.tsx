import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import _releaseService, {
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React, { ReactElement } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

describe('PublicationScheduledReleases', () => {
  const testPublicationId = 'publication-1';

  const testRelease1: ReleaseSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1',
    live: false,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canDeleteRelease: false,
      canMakeAmendmentOfRelease: true,
      canViewRelease: false,
    },
    previousVersionId: 'release-previous-id',
    publishScheduled: '2022-01-01T00:00:00',
    year: 2021,
    yearTitle: '2021/22',
    slug: 'release-1-slug',
    title: 'Release 1',
    timePeriodCoverage: {
      label: 'Academic year',
      value: 'AY',
    },
    type: 'AdHocStatistics',
  };

  const testRelease2: ReleaseSummaryWithPermissions = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-2',
    publishScheduled: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testReleases = [testRelease1, testRelease2];

  beforeEach(() => {
    releaseService.getReleaseStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });
  });

  test('renders the scheduled releases table correctly', async () => {
    render(
      <PublicationScheduledReleases
        publicationId={testPublicationId}
        releases={testReleases}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('Release 1')).toBeInTheDocument();

    await waitFor(() => {
      expect(within(row1Cells[1]).getByText('Scheduled')).toBeInTheDocument();
    });

    expect(
      within(row1Cells[2]).getByRole('button', { name: 'View stages' }),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[3]).getByText('1 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2Cells[1]).getByText('Scheduled')).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'View stages' }),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[3]).getByText('2 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[4]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2/summary',
    );
  });

  test('shows an empty message when there are no scheduled releases', () => {
    render(
      <PublicationScheduledReleases
        publicationId={testPublicationId}
        releases={[]}
      />,
    );

    expect(
      screen.getByText('You have no scheduled releases.'),
    ).toBeInTheDocument();
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', () => {
    render(
      <PublicationScheduledReleases
        publicationId={testPublicationId}
        releases={[
          {
            ...testRelease1,
            permissions: {
              ...testRelease1.permissions,
              canUpdateRelease: false,
            },
          },
          testRelease2,
        ]}
      />,
    );

    const rows = screen.getAllByRole('row');
    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(within(row1Cells[0]).getByText('Release 1')).toBeInTheDocument();

    expect(
      within(row1Cells[4]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/summary',
    );
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
