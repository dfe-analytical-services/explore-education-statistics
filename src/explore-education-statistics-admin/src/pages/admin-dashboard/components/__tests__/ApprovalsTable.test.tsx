import ApprovalsTable from '@admin/pages/admin-dashboard/components/ApprovalsTable';
import { MethodologyVersion } from '@admin/services/methodologyService';
import { Release } from '@admin/services/releaseService';
import { waitFor, within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('ApprovalsTable', () => {
  const testMethodologies: MethodologyVersion[] = [
    {
      id: 'c8c911e3-39c1-452b-801f-25bb79d1deb7',
      methodologyId: 'b8bd000c-f9d8-4319-a2b3-6bc18675e5ac',
      owningPublication: {
        id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
        title: 'Permanent and fixed-period exclusions in England',
      },
      otherPublications: [],
      published: '2018-08-25T00:00:00',
      publishingStrategy: 'Immediately',
      slug: 'permanent-and-fixed-period-exclusions-in-england',
      status: 'Approved',
      title: 'Pupil exclusion statistics: methodology',
      amendment: false,
    },
  ];

  const testReleases: Release[] = [
    {
      id: 'test-id',
      title: 'Academic year 2016/17',
      slug: '2024-25',
      publicationId: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
      publicationTitle: 'Permanent and fixed-period exclusions in England',
      publicationSlug: 'pub-slug',
      year: 2024,
      yearTitle: '2024/25',
      nextReleaseDate: {
        year: '2200',
        month: '1',
        day: '',
      },
      live: false,
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      preReleaseAccessList: '<p>Test public access list</p>',
      preReleaseUsersOrInvitesAdded: false,
      latestRelease: false,
      type: 'NationalStatistics',
      approvalStatus: 'Approved',
      notifySubscribers: false,
      amendment: false,
      permissions: {
        canAddPrereleaseUsers: true,
        canViewRelease: true,
        canUpdateRelease: true,
        canDeleteRelease: false,
        canMakeAmendmentOfRelease: false,
      },
      updatePublishedDate: false,
    },
    {
      id: '86d868cf-ff4b-4325-ef26-08d93c9b5089',
      title: 'Academic year 2024/25',
      slug: '2024-25',
      publicationId: '959bd40c-4685-46ff-396d-08d93c9b5159',
      publicationTitle:
        'UI tests - Publication and Release UI Permissions Publication Owner',
      publicationSlug:
        'ui-tests-publication-and-release-ui-permissions-publication-owner',
      year: 2024,
      yearTitle: '2024/25',
      nextReleaseDate: {
        year: '2200',
        month: '1',
        day: '',
      },
      publishScheduled: '2048-11-16',
      live: false,
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      preReleaseAccessList: '<p>Test public access list</p>',
      preReleaseUsersOrInvitesAdded: false,
      latestRelease: false,
      type: 'NationalStatistics',
      approvalStatus: 'Approved',
      notifySubscribers: false,
      amendment: false,
      permissions: {
        canAddPrereleaseUsers: true,
        canViewRelease: true,
        canUpdateRelease: true,
        canDeleteRelease: false,
        canMakeAmendmentOfRelease: false,
      },
      updatePublishedDate: false,
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
      'Permanent and fixed-period exclusions in England',
    );

    const row3cells = within(rows[2]).getAllByRole('cell');
    expect(row3cells[0]).toHaveTextContent('Academic year 2016/17');
    expect(row3cells[1]).toHaveTextContent('Release');
    expect(
      within(row3cells[2]).getByRole('link', { name: /Review this page/ }),
    ).toBeInTheDocument();

    const row4cells = within(rows[3]).getAllByRole('cell');
    expect(row4cells[0]).toHaveTextContent(
      'Pupil exclusion statistics: methodology',
    );
    expect(row4cells[1]).toHaveTextContent('Methodology');
    expect(
      within(row4cells[2]).getByRole('link', { name: /Review this page/ }),
    ).toBeInTheDocument();

    expect(within(rows[4]).getByRole('columnheader')).toHaveTextContent(
      'UI tests - Publication and Release UI Permissions Publication Owner',
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
