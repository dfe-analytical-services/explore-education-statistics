import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import _releaseVersionService, {
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React, { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

describe('PublicationScheduledReleases', () => {
  const testPublicationId = 'publication-1';

  const testRelease1: ReleaseVersionSummaryWithPermissions = {
    amendment: false,
    approvalStatus: 'Approved',
    id: 'release-1-version-1',
    releaseId: 'release-1',
    live: false,
    permissions: {
      canAddPrereleaseUsers: false,
      canUpdateRelease: true,
      canUpdateReleaseVersion: true,
      canDeleteReleaseVersion: false,
      canMakeAmendmentOfReleaseVersion: true,
      canViewReleaseVersion: false,
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
    latestRelease: false,
  };

  const testRelease2: ReleaseVersionSummaryWithPermissions = {
    ...testRelease1,
    approvalStatus: 'Approved',
    id: 'release-2',
    publishScheduled: '2022-01-02T00:00:00',
    slug: 'release-2-slug',
    title: 'Release 2',
  };

  const testReleases = [testRelease1, testRelease2];

  beforeEach(() => {
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
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
      within(row1Cells[2]).getByRole('button', {
        name: 'View stages for Release 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[3]).getByText('1 January 2022'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-version-1/summary',
    );

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(within(row2Cells[1]).getByText('Scheduled')).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'View stages for Release 2',
      }),
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
              canUpdateReleaseVersion: false,
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
      '/publication/publication-1/release/release-1-version-1/summary',
    );
  });
});

function render(element: ReactNode) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
