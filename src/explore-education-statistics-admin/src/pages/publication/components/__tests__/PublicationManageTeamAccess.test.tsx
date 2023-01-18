import PublicationManageReleaseTeamAccess from '@admin/pages/publication/components/PublicationManageReleaseTeamAccess';
import { ReleaseSummary } from '@admin/services/releaseService';
import _userService from '@admin/services/userService';
import _releasePermissionService, {
  UserReleaseInvite,
  UserReleaseRole,
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

  const testContributors: UserReleaseRole[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User 1',
      userEmail: 'user1@test.com',
      role: 'Contributor',
    },
  ];

  const testInvites: UserReleaseInvite[] = [
    {
      email: 'user2@test.com',
      role: 'Contributor',
    },
  ];

  test('renders the manage team access table correctly', async () => {
    releasePermissionService.listRoles.mockResolvedValue(testContributors);
    releasePermissionService.listInvites.mockResolvedValue(testInvites);
    render(
      <PublicationManageReleaseTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
        showManageContributorsButton
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release-value')).toHaveTextContent(
        'Academic Year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(
      within(rows[1]).getByText('User 1 (user1@test.com)'),
    ).toBeInTheDocument();
    expect(within(rows[2]).getByText('user2@test.com')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Manage release contributors',
      }),
    ).toBeInTheDocument();
  });

  test('omits the "Manage release contributors" button when requested', async () => {
    releasePermissionService.listRoles.mockResolvedValue(testContributors);
    releasePermissionService.listInvites.mockResolvedValue(testInvites);
    render(
      <PublicationManageReleaseTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
        showManageContributorsButton={false}
      />,
    );

    expect(
      screen.queryByRole('link', {
        name: 'Manage release contributors',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders a message if there are no contributors or invites', async () => {
    releasePermissionService.listRoles.mockResolvedValue([]);
    releasePermissionService.listInvites.mockResolvedValue([]);
    render(
      <PublicationManageReleaseTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
        showManageContributorsButton
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
    releasePermissionService.listRoles.mockResolvedValue(testContributors);
    releasePermissionService.listInvites.mockResolvedValue(testInvites);
    render(
      <PublicationManageReleaseTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
        showManageContributorsButton
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release-value')).toHaveTextContent(
        'Academic Year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');
    });

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
    releasePermissionService.listRoles.mockResolvedValue(testContributors);
    releasePermissionService.listInvites.mockResolvedValue(testInvites);
    render(
      <PublicationManageReleaseTeamAccess
        publicationId={testPublicationId}
        release={testRelease}
        showManageContributorsButton
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release-value')).toHaveTextContent(
        'Academic Year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');
    });

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
