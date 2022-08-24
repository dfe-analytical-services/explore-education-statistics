import PublicationManageTeamAccess from '@admin/pages/publication/components/PublicationManageTeamAccess';
import { ReleaseSummary } from '@admin/services/releaseService';
import _userService from '@admin/services/userService';
import _releasePermissionService, {
  ContributorInvite,
  ContributorViewModel,
} from '@admin/services/releasePermissionService';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import React, { ReactElement } from 'react';

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

jest.mock('@admin/services/userService');
const userService = _userService as jest.Mocked<typeof _userService>;

describe('PublicationManageTeamAccess', () => {
  const testPublicationId = 'publication-1';

  const testRelease: ReleaseSummary = {
    id: 'release1-id',
    slug: 'release-1-slug',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Academic Year 2000/01',
    type: 'AdHocStatistics',
    publishScheduled: '2001-01-01',
    approvalStatus: 'Draft',
    year: 2000,
    yearTitle: '2000/01',
    live: false,
    amendment: false,
  };

  const testContributors: ContributorViewModel[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User 1',
      userEmail: 'user1@test.com',
    },
  ];

  const testInvites: ContributorInvite[] = [{ email: 'user2@test.com' }];

  test('renders the manage team access table correctly', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testContributors,
    );
    releasePermissionService.listReleaseContributorInvites.mockResolvedValue(
      testInvites,
    );
    render(
      <PublicationManageTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Academic Year 2000/01 (Not live)'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2000/01 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(
      within(rows[1]).getByText('User 1 (user1@test.com)'),
    ).toBeInTheDocument();
    expect(within(rows[2]).getByText('user2@test.com')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Add or remove users',
      }),
    ).toBeInTheDocument();
  });

  test('renders a message if there are no contributors or invites', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue([]);
    releasePermissionService.listReleaseContributorInvites.mockResolvedValue(
      [],
    );
    render(
      <PublicationManageTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          'There are no contributors or pending contributor invites for this release.',
        ),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('handles removing a user correctly', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testContributors,
    );
    releasePermissionService.listReleaseContributorInvites.mockResolvedValue(
      testInvites,
    );
    render(
      <PublicationManageTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Academic Year 2000/01 (Not live)'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2000/01 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Remove User 1' }));

    userEvent.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(
        releasePermissionService.removeAllUserContributorPermissionsForPublication,
      ).toHaveBeenCalledWith(testPublicationId, 'user-1');
    });
  });

  test('handles cancelling an invite correctly', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testContributors,
    );
    releasePermissionService.listReleaseContributorInvites.mockResolvedValue(
      testInvites,
    );
    render(
      <PublicationManageTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Academic Year 2000/01 (Not live)'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2000/01 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    userEvent.click(
      screen.getByRole('button', { name: 'Cancel invite for user2@test.com' }),
    );

    userEvent.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(userService.removeContributorReleaseInvites).toHaveBeenCalledWith(
        'user2@test.com',
        testPublicationId,
      );
    });
  });
});

function render(element: ReactElement) {
  baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
