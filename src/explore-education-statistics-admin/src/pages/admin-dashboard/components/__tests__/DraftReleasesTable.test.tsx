import DraftReleasesTable from '@admin/pages/admin-dashboard/components/DraftReleasesTable';
import _releaseVersionService, {
  DashboardReleaseVersionSummary,
} from '@admin/services/releaseVersionService';
import { waitFor, within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter } from 'react-router';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

describe('DraftReleasesTable', () => {
  const testReleases: DashboardReleaseVersionSummary[] = [
    {
      id: 'release-1-version-1',
      releaseId: 'release-1',
      latestRelease: true,
      slug: 'release-1-slug',
      title: 'Release 1',
      year: 2020,
      yearTitle: '2020/21',
      publication: {
        id: 'publication-1',
        title: 'Publication 1',
        slug: 'publication-1-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
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
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'Draft',
      amendment: false,
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canDeleteReleaseVersion: false,
        canMakeAmendmentOfReleaseVersion: false,
        canAddPrereleaseUsers: false,
      },
    },
    {
      id: 'release-2-version-1',
      releaseId: 'release-2',
      latestRelease: true,
      slug: 'release-2-slug',
      title: 'Release 2',
      year: 2021,
      yearTitle: '2021/22',
      publication: {
        id: 'publication-2',
        title: 'Publication 2',
        slug: 'publication-2-slug',
        latestReleaseSlug: 'latest-release-slug-2',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
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
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'Draft',
      amendment: true,
      previousVersionId: 'previous-version-id',
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canDeleteReleaseVersion: true,
        canUpdateReleaseVersion: true,
        canMakeAmendmentOfReleaseVersion: false,
        canAddPrereleaseUsers: false,
      },
    },
    {
      id: 'release-3-version-1',
      releaseId: 'release-3',
      latestRelease: false,
      slug: 'release-3-slug',
      title: 'Release 3',
      year: 2022,
      yearTitle: '2022/23',
      publication: {
        id: 'publication-1',
        title: 'Publication 1',
        slug: 'publication-1-slug',
        latestReleaseSlug: 'latest-release-slug-1',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
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
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'HigherLevelReview',
      amendment: false,
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canUpdateReleaseVersion: true,
        canDeleteReleaseVersion: true,
        canMakeAmendmentOfReleaseVersion: false,
        canAddPrereleaseUsers: false,
      },
    },
    {
      id: 'release-4-version-1',
      releaseId: 'release-4',
      latestRelease: true,
      slug: 'release-4-slug',
      title: 'Release 4',
      year: 2023,
      yearTitle: '2023/24',
      publication: {
        id: 'publication-3',
        title: 'Publication 3',
        slug: 'publication-3-slug',
        latestReleaseSlug: 'latest-release-slug-3',
        owner: true,
        contact: {
          teamName: 'Mock Contact Team Name',
          teamEmail: 'Mock Contact Team Email',
          contactName: 'Mock Contact Name',
        },
      },
      live: false,
      timePeriodCoverage: {
        value: 'AY',
        label: 'Academic year',
      },
      type: 'AccreditedOfficialStatistics',
      approvalStatus: 'HigherLevelReview',
      amendment: true,
      previousVersionId: 'previous-version-id',
      permissions: {
        canViewReleaseVersion: true,
        canUpdateRelease: true,
        canDeleteReleaseVersion: true,
        canUpdateReleaseVersion: true,
        canMakeAmendmentOfReleaseVersion: false,
        canAddPrereleaseUsers: false,
      },
    },
  ];

  beforeEach(() => {
    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue({
      errors: [
        {
          code: 'DataFileImportsMustBeCompleted',
        },
        {
          code: 'EmptyContentSectionExists',
        },
      ],
      valid: false,
      warnings: [{ code: 'NoMethodology' }],
    });
  });

  test('renders the table correctly for BAU users', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTable
          isBauUser
          releases={testReleases}
          onChangeRelease={noop}
        />
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

    // Draft
    const row3cells = within(rows[2]).getAllByRole('cell');
    expect(within(row3cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row3cells[1]).getByText('Draft')).toBeInTheDocument();
    expect(
      within(row3cells[2]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-version-1/summary',
    );
    expect(
      within(row3cells[2]).queryByRole('button', { name: /View issues/ }),
    ).not.toBeInTheDocument();
    expect(
      within(row3cells[2]).queryByRole('button', {
        name: 'Cancel amendment for Release 1',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row3cells[2]).queryByRole('link', {
        name: 'View existing version for Release 1',
      }),
    ).not.toBeInTheDocument();

    // In review
    const row4cells = within(rows[3]).getAllByRole('cell');
    expect(within(row4cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(within(row4cells[1]).getByText('In Review')).toBeInTheDocument();
    expect(
      within(row4cells[2]).getByRole('link', { name: 'Edit Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3-version-1/summary',
    );
    expect(
      within(row4cells[2]).queryByRole('button', { name: /View issues/ }),
    ).not.toBeInTheDocument();
    expect(
      within(row4cells[2]).queryByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row4cells[2]).queryByRole('link', {
        name: 'View existing version for Release 3',
      }),
    ).not.toBeInTheDocument();

    expect(within(rows[4]).getByRole('columnheader')).toHaveTextContent(
      'Publication 2',
    );

    // Amendment
    const row6cells = within(rows[5]).getAllByRole('cell');
    expect(row6cells[0]).toHaveTextContent('Release 2');
    expect(within(row6cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(
      within(row6cells[1]).getByText('Draft Amendment'),
    ).toBeInTheDocument();
    expect(
      within(row6cells[2]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-2/release/release-2-version-1/summary',
    );
    expect(
      within(row6cells[2]).getByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row6cells[2]).getByRole('link', {
        name: 'View existing version for Release 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row6cells[2]).queryByRole('button', { name: /View issues/ }),
    ).not.toBeInTheDocument();

    expect(within(rows[6]).getByRole('columnheader')).toHaveTextContent(
      'Publication 3',
    );

    // In review amendment
    const row8cells = within(rows[7]).getAllByRole('cell');
    expect(within(row8cells[0]).getByText('Release 4')).toBeInTheDocument();
    expect(
      within(row8cells[1]).getByText('In Review Amendment'),
    ).toBeInTheDocument();
    expect(
      within(row8cells[2]).getByRole('link', { name: 'Edit Release 4' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-3/release/release-4-version-1/summary',
    );
    expect(
      within(row8cells[2]).queryByRole('button', { name: /View issues/ }),
    ).not.toBeInTheDocument();
    expect(
      within(row8cells[2]).queryByRole('button', {
        name: 'Cancel amendment for Release 4',
      }),
    ).toBeInTheDocument();
    expect(
      within(row8cells[2]).queryByRole('link', {
        name: 'View existing version for Release 4',
      }),
    ).toBeInTheDocument();
  });

  test('renders the table correctly for non-BAU users', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTable
          isBauUser={false}
          releases={testReleases}
          onChangeRelease={noop}
        />
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

    // Draft
    const row3cells = within(rows[2]).getAllByRole('cell');
    expect(within(row3cells[0]).getByText('Release 1')).toBeInTheDocument();
    expect(within(row3cells[1]).getByText('Draft')).toBeInTheDocument();
    expect(
      await within(row3cells[2]).findByRole('button', {
        name: 'View issues (3)',
      }),
    ).toBeInTheDocument();
    expect(
      within(row3cells[3]).getByRole('link', { name: 'Edit Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-version-1/summary',
    );
    expect(
      within(row3cells[3]).queryByRole('button', {
        name: 'Cancel amendment for Release 1',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row3cells[3]).queryByRole('link', {
        name: 'View existing version for Release 1',
      }),
    ).not.toBeInTheDocument();

    // In review
    const row4cells = within(rows[3]).getAllByRole('cell');
    expect(within(row4cells[0]).getByText('Release 3')).toBeInTheDocument();
    expect(within(row4cells[1]).getByText('In Review')).toBeInTheDocument();
    expect(
      within(row4cells[2]).getByRole('button', { name: 'View issues (3)' }),
    ).toBeInTheDocument();
    expect(
      within(row4cells[3]).getByRole('link', { name: 'Edit Release 3' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-3-version-1/summary',
    );
    expect(
      within(row4cells[3]).queryByRole('button', {
        name: 'Cancel amendment for Release 3',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row4cells[3]).queryByRole('link', {
        name: 'View existing version for Release 3',
      }),
    ).not.toBeInTheDocument();

    expect(within(rows[4]).getByRole('columnheader')).toHaveTextContent(
      'Publication 2',
    );

    // Amendment
    const row6cells = within(rows[5]).getAllByRole('cell');
    expect(row6cells[0]).toHaveTextContent('Release 2');
    expect(within(row6cells[0]).getByText('Release 2')).toBeInTheDocument();
    expect(
      within(row6cells[1]).getByText('Draft Amendment'),
    ).toBeInTheDocument();
    expect(
      within(row6cells[2]).getByRole('button', { name: 'View issues (3)' }),
    ).toBeInTheDocument();
    expect(
      within(row6cells[3]).getByRole('link', { name: 'Edit Release 2' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-2/release/release-2-version-1/summary',
    );
    expect(
      within(row6cells[3]).getByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row6cells[3]).getByRole('link', {
        name: 'View existing version for Release 2',
      }),
    ).toBeInTheDocument();

    expect(within(rows[6]).getByRole('columnheader')).toHaveTextContent(
      'Publication 3',
    );

    // In review amendment
    const row8cells = within(rows[7]).getAllByRole('cell');
    expect(within(row8cells[0]).getByText('Release 4')).toBeInTheDocument();
    expect(
      within(row8cells[1]).getByText('In Review Amendment'),
    ).toBeInTheDocument();
    expect(
      within(row8cells[2]).getByRole('button', { name: 'View issues (3)' }),
    ).toBeInTheDocument();
    expect(
      within(row8cells[3]).getByRole('link', { name: 'Edit Release 4' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-3/release/release-4-version-1/summary',
    );
    expect(
      within(row8cells[3]).queryByRole('button', {
        name: 'Cancel amendment for Release 4',
      }),
    ).toBeInTheDocument();
    expect(
      within(row8cells[3]).queryByRole('link', {
        name: 'View existing version for Release 4',
      }),
    ).toBeInTheDocument();
  });

  test('shows a view instead of edit link if you do not have permission to edit the release', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTable
          isBauUser
          releases={[
            {
              ...testReleases[0],
              permissions: {
                ...testReleases[0].permissions,
                canUpdateReleaseVersion: false,
              },
            },
          ]}
          onChangeRelease={noop}
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
      within(row3cells[2]).getByRole('link', { name: 'View Release 1' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-version-1/summary',
    );
  });

  test('does not show the cancel button if you do not have permission to cancel the amendment', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTable
          isBauUser
          releases={[
            {
              ...testReleases[2],
              permissions: {
                ...testReleases[2].permissions,
                canDeleteReleaseVersion: false,
              },
            },
          ]}
          onChangeRelease={noop}
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
      within(row3cells[2]).queryByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    ).not.toBeInTheDocument();
  });

  test('handles cancelling an amendment correctly', async () => {
    const handleOnChange = jest.fn();
    releaseVersionService.getDeleteReleaseVersionPlan.mockResolvedValue({
      scheduledMethodologies: [
        {
          id: 'methodology-1',
          title: 'Methodology 1',
        },
        {
          id: 'methodology-2',
          title: 'Methodology 2',
        },
      ],
    });

    render(
      <MemoryRouter>
        <DraftReleasesTable
          isBauUser
          releases={testReleases}
          onChangeRelease={handleOnChange}
        />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Publication / Release period'),
      ).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    const row6cells = within(rows[5]).getAllByRole('cell');

    await userEvent.click(
      within(row6cells[2]).getByRole('button', {
        name: 'Cancel amendment for Release 2',
      }),
    );

    expect(
      await screen.findByText(
        'Confirm you want to cancel this amended release',
      ),
    ).toBeInTheDocument();

    expect(
      releaseVersionService.getDeleteReleaseVersionPlan,
    ).toHaveBeenCalledWith('release-2-version-1');

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByText('Methodology 1')).toBeInTheDocument();
    expect(modal.getByText('Methodology 2')).toBeInTheDocument();

    await userEvent.click(
      modal.getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(releaseVersionService.deleteReleaseVersion).toHaveBeenCalledWith(
        'release-2-version-1',
      );
    });

    expect(handleOnChange).toHaveBeenCalled();

    expect(
      screen.queryByText('Confirm you want to cancel this amended release'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when no releases are available', async () => {
    render(
      <MemoryRouter>
        <DraftReleasesTable isBauUser releases={[]} onChangeRelease={noop} />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('There are currently no draft releases'),
      ).toBeInTheDocument();
    });
  });
});
