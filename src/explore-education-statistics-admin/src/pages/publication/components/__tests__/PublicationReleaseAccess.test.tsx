import baseRender from '@common-test/render';
import PublicationReleaseAccess from '@admin/pages/publication/components/PublicationReleaseAccess';
import { ReleaseSummary } from '@admin/services/releaseService';
import _userService from '@admin/services/userService';
import _releasePermissionService, {
  UserReleaseInvite,
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';
import React, { ReactElement } from 'react';

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

jest.mock('@admin/services/userService');
const userService = _userService as jest.Mocked<typeof _userService>;

describe('PublicationReleaseAccess', () => {
  const testPublicationId = 'publication-1';

  const testRelease: ReleaseSummary = {
    id: 'release1-id',
    slug: 'release-1-slug',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic year',
    },
    title: 'Academic year 2000/01',
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
      <PublicationReleaseAccess
        publicationId={testPublicationId}
        release={testRelease}
        hasReleaseTeamManagementPermission
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release')).toHaveTextContent(
        'Academic year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status')).toHaveTextContent('Draft');
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('User 1')).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('user1@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Remove User 1' }),
    ).toBeInTheDocument();

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
      <PublicationReleaseAccess
        publicationId={testPublicationId}
        release={testRelease}
        hasReleaseTeamManagementPermission={false}
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
      <PublicationReleaseAccess
        publicationId={testPublicationId}
        release={testRelease}
        hasReleaseTeamManagementPermission
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
      <PublicationReleaseAccess
        publicationId={testPublicationId}
        release={testRelease}
        hasReleaseTeamManagementPermission
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release')).toHaveTextContent(
        'Academic year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status')).toHaveTextContent('Draft');
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
      <PublicationReleaseAccess
        publicationId={testPublicationId}
        release={testRelease}
        hasReleaseTeamManagementPermission
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('Release')).toHaveTextContent(
        'Academic year 2000/01 (Not live)',
      );
    });

    await waitFor(() => {
      expect(screen.getByTestId('Status')).toHaveTextContent('Draft');
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
  return baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
