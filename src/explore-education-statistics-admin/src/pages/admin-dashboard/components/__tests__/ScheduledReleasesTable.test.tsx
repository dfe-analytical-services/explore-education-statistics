import ScheduledReleasesTable from '@admin/pages/admin-dashboard/components/ScheduledReleasesTable';
import _releaseVersionService, {
  DashboardReleaseVersionSummary,
} from '@admin/services/releaseVersionService';
import { waitFor, within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

describe('ScheduledReleasesTable', () => {
  const testReleases: DashboardReleaseVersionSummary[] = [
    {
      id: 'release-1-version-1',
      releaseId: 'release-1',
      latestRelease: true,
      publishScheduled: '2021-06-30T00:00:00',
      slug: 'release-1-slug',
      title: 'Release 1',
      amendment: false,
      live: false,
      year: 2021,
      yearTitle: '2021',
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      publication: {
        id: 'publication-1',
        slug: 'publication-1-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        title: 'Publication 1',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canAddPrereleaseUsers: false,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
      approvalStatus: 'Approved',
    },
    {
      id: 'release-2-version-1',
      releaseId: 'release-2',
      latestRelease: true,
      publishScheduled: '2021-05-30T00:00:00',
      slug: 'release-2-slug',
      title: 'Release 2',
      amendment: false,
      live: false,
      year: 2021,
      yearTitle: '2021',
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      publication: {
        id: 'publication-2',
        slug: 'publication-2-slug',
        latestReleaseSlug: 'latest-release-slug-2',
        title: 'Publication 2',
        owner: false,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canAddPrereleaseUsers: false,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
      approvalStatus: 'Approved',
    },
    {
      id: 'release-1-version-2',
      releaseId: 'release-1',
      latestRelease: false,
      publishScheduled: '2021-01-01T00:00:00',
      slug: 'release-1-slug',
      title: 'Release 1',
      amendment: false,
      live: false,
      year: 2021,
      yearTitle: '2021',
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      publication: {
        id: 'publication-1',
        slug: 'publication-1-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        title: 'Publication 1',
        owner: false,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canDeleteReleaseVersion: true,
        canAddPrereleaseUsers: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
      approvalStatus: 'Approved',
    },
    {
      id: 'release-3-version-1',
      releaseId: 'release-3',
      latestRelease: true,
      publishScheduled: '2021-05-30T00:00:00',
      slug: 'release-3-slug',
      title: 'Release 3',
      amendment: false,
      live: false,
      year: 2021,
      yearTitle: '2021',
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      publication: {
        id: 'publication-3',
        slug: 'publication-3-slug',
        latestReleaseSlug: 'latest-release-slug-3',
        title: 'Publication 3',
        owner: false,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canAddPrereleaseUsers: false,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
      approvalStatus: 'Approved',
    },
  ];

  beforeEach(() => {
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Scheduled',
    });
  });

  test('renders the table of releases grouped by publication ', async () => {
    render(
      <MemoryRouter>
        <ScheduledReleasesTable releases={testReleases} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Publication / Release period'),
      ).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(8);

    expect(within(rows[1]).getByRole('columnheader')).toHaveTextContent(
      'Publication 1',
    );

    const row3cells = within(rows[2]).getAllByRole('cell');
    expect(row3cells[0]).toHaveTextContent('Release 1');
    expect(row3cells[1]).toHaveTextContent('Scheduled');
    expect(row3cells[2]).toHaveTextContent('View stages');
    expect(row3cells[3]).toHaveTextContent('30 June 2021');
    expect(
      within(row3cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toBeInTheDocument();

    const row4cells = within(rows[3]).getAllByRole('cell');
    expect(row4cells[0]).toHaveTextContent('Release 1');
    expect(row4cells[1]).toHaveTextContent('Scheduled');
    expect(row4cells[2]).toHaveTextContent('View stages');
    expect(row4cells[3]).toHaveTextContent('1 January 2021');
    expect(
      within(row4cells[4]).getByRole('link', { name: 'Edit Release 1' }),
    ).toBeInTheDocument();

    expect(within(rows[4]).getByRole('columnheader')).toHaveTextContent(
      'Publication 2',
    );

    const row6cells = within(rows[5]).getAllByRole('cell');
    expect(row6cells[0]).toHaveTextContent('Release 2');
    expect(row6cells[1]).toHaveTextContent('Scheduled');
    expect(row6cells[2]).toHaveTextContent('View stages');
    expect(row6cells[3]).toHaveTextContent('30 May 2021');
    expect(
      within(row6cells[4]).getByRole('link', { name: 'Edit Release 2' }),
    ).toBeInTheDocument();

    expect(within(rows[6]).getByRole('columnheader')).toHaveTextContent(
      'Publication 3',
    );

    const row8cells = within(rows[7]).getAllByRole('cell');
    expect(row8cells[0]).toHaveTextContent('Release 3');
    expect(row8cells[1]).toHaveTextContent('Scheduled');
    expect(row8cells[2]).toHaveTextContent('View stages');
    expect(row8cells[3]).toHaveTextContent('30 May 2021');
    expect(
      within(row8cells[4]).getByRole('link', { name: 'Edit Release 3' }),
    ).toBeInTheDocument();
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', async () => {
    render(
      <MemoryRouter>
        <ScheduledReleasesTable
          releases={[
            {
              ...testReleases[0],
              permissions: {
                ...testReleases[0].permissions,
                canUpdateReleaseVersion: false,
              },
            },
          ]}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Publication / Release period'),
      ).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    const row3cells = within(rows[2]).getAllByRole('cell');

    expect(
      within(row3cells[4]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-version-1/summary',
    );
  });

  test('renders correctly when no releases are available', async () => {
    render(
      <MemoryRouter>
        <ScheduledReleasesTable releases={[]} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('There are currently no scheduled releases'),
      ).toBeInTheDocument();
    });
  });
});
