import ApprovalsTable from '@admin/pages/admin-dashboard/components/ApprovalsTable';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { DashboardReleaseVersionSummary } from '@admin/services/releaseVersionService';
import { waitFor, within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('ApprovalsTable', () => {
  const testMethodologies: MethodologyVersion[] = [
    {
      id: 'methodology-version-1',
      methodologyId: 'methodology-1',
      owningPublication: {
        id: 'publication-1',
        title: 'Publication 1 title',
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
        slug: 'mock-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        owner: true,
      },
      otherPublications: [],
      published: '2018-08-25T00:00:00',
      publishingStrategy: 'Immediately',
      slug: 'methodology-1-slug',
      status: 'Approved',
      title: 'Methodology 1',
      amendment: false,
    },
  ];

  const testReleases: DashboardReleaseVersionSummary[] = [
    {
      id: 'test-id',
      releaseId: 'release-1',
      title: 'Academic year 2016/17',
      slug: '2024-25',
      publication: {
        id: 'publication-1',
        title: 'Publication 1 title',
        slug: 'publication-1-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      year: 2024,
      yearTitle: '2024/25',
      nextReleaseDate: {
        year: '2200',
        month: '1',
        day: '',
      },
      live: false,
      latestRelease: false,
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'Approved',
      amendment: false,
      permissions: {
        canAddPrereleaseUsers: true,
        canUpdateRelease: true,
        canViewReleaseVersion: true,
        canUpdateReleaseVersion: true,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
    },
    {
      id: '86d868cf-ff4b-4325-ef26-08d93c9b5089',
      releaseId: 'release-1',
      title: 'Academic year 2024/25',
      slug: '2024-25',
      publication: {
        id: 'publication-2',
        title: 'Publication 2 title',
        slug: 'publication-2-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      year: 2024,
      yearTitle: '2024/25',
      nextReleaseDate: {
        year: '2200',
        month: '1',
        day: '',
      },
      publishScheduled: '2048-11-16',
      live: false,
      latestRelease: false,
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'Approved',
      previousVersionId: 'old-release-2-id',
      amendment: false,
      permissions: {
        canAddPrereleaseUsers: true,
        canUpdateRelease: true,
        canViewReleaseVersion: true,
        canUpdateReleaseVersion: true,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
      },
    },
  ];

  test('renders the table of releases and methodologies grouped by publication ', async () => {
    render(
      <MemoryRouter>
        <ApprovalsTable
          methodologyApprovals={testMethodologies}
          releaseApprovals={testReleases}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Publication / Page')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(6);

    expect(within(rows[1]).getByRole('columnheader')).toHaveTextContent(
      'Publication 1 title',
    );

    const row3cells = within(rows[2]).getAllByRole('cell');
    expect(row3cells[0]).toHaveTextContent('Academic year 2016/17');
    expect(row3cells[1]).toHaveTextContent('Release');
    expect(
      within(row3cells[2]).getByRole('link', { name: /Review this page/ }),
    ).toBeInTheDocument();

    const row4cells = within(rows[3]).getAllByRole('cell');
    expect(row4cells[0]).toHaveTextContent('Methodology 1');
    expect(row4cells[1]).toHaveTextContent('Methodology');
    expect(
      within(row4cells[2]).getByRole('link', { name: /Review this page/ }),
    ).toBeInTheDocument();

    expect(within(rows[4]).getByRole('columnheader')).toHaveTextContent(
      'Publication 2 title',
    );

    const row6cells = within(rows[5]).getAllByRole('cell');
    expect(row6cells[0]).toHaveTextContent('Academic year 2024/25');
    expect(row6cells[1]).toHaveTextContent('Release');
    expect(
      within(row6cells[2]).getByRole('link', { name: /Review this page/ }),
    ).toBeInTheDocument();
  });

  test('renders correctly when there are no approvals', async () => {
    render(
      <MemoryRouter>
        <ApprovalsTable methodologyApprovals={[]} releaseApprovals={[]} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          'There are no releases or methodologies awaiting your approval.',
        ),
      ).toBeInTheDocument();
    });
  });
});
