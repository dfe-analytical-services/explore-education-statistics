import ReleaseContributorsPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import _releasePermissionService, {
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/releasePermissionService');
jest.mock('@admin/services/userService');

const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

describe('ReleaseContributorPermissions', () => {
  const testReleaseContributors: ManageAccessPageContributor[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User Name 1',
      userEmail: 'user1@test.com',
    },
    {
      userId: 'user-2',
      userDisplayName: 'User Name 2',
      userEmail: 'user2@test.com',
    },
    {
      userId: 'user-3',
      userDisplayName: 'User Name 3',
      userEmail: 'user3@test.com',
    },
  ];

  const testPendingInviteEmails: string[] = [
    'user4@test.com',
    'user5@test.com',
  ];

  test('renders a message when there are no release contributors or invites', async () => {
    releasePermissionService.listReleaseContributorsAndInvites.mockResolvedValue(
      { contributors: [], pendingInviteEmails: [] },
    );

    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={[]}
        pendingInviteEmails={[]}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('Warning')).toBeInTheDocument();

      expect(
        screen.getByTestId('releaseContributors-warning').textContent,
      ).toContain(
        'There are currently no team members or pending invites associated with this publication.',
      );
    });
  });

  test('renders the contributors table correctly', async () => {
    releasePermissionService.listReleaseContributorsAndInvites.mockResolvedValue(
      {
        contributors: testReleaseContributors,
        pendingInviteEmails: testPendingInviteEmails,
      },
    );

    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        pendingInviteEmails={testPendingInviteEmails}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(5);

    expect(
      within(rows[0]).getByText('User Name 1 (user1@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[0]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(
      within(rows[1]).getByText('User Name 2 (user2@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(
      within(rows[2]).getByText('User Name 3 (user3@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(within(rows[3]).getByText('user4@test.com')).toBeInTheDocument();
    expect(
      within(rows[3]).getByRole('button', { name: 'Cancel invite' }),
    ).toBeInTheDocument();

    expect(within(rows[4]).getByText('user5@test.com')).toBeInTheDocument();
    expect(
      within(rows[4]).getByRole('button', { name: 'Cancel invite' }),
    ).toBeInTheDocument();
  });

  test('remove user', async () => {
    releasePermissionService.listReleaseContributorsAndInvites.mockResolvedValue(
      { contributors: testReleaseContributors, pendingInviteEmails: [] },
    );

    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        pendingInviteEmails={[]}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);

    userEvent.click(
      within(rows[0]).getByRole('button', { name: 'Remove user' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Confirm user removal',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to remove User Name 1 from all releases in this publication?',
    );

    expect(onUserRemove).not.toBeCalled();
    expect(onUserInvitesRemove).not.toBeCalled();

    userEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(onUserRemove).toHaveBeenCalledTimes(1);
      expect(onUserRemove).toHaveBeenCalledWith('user-1');
      expect(onUserInvitesRemove).not.toBeCalled();
    });
  });

  test('cancel invite', async () => {
    releasePermissionService.listReleaseContributorsAndInvites.mockResolvedValue(
      { contributors: [], pendingInviteEmails: testPendingInviteEmails },
    );

    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={[]}
        pendingInviteEmails={testPendingInviteEmails}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(2);

    userEvent.click(
      within(rows[0]).getByRole('button', { name: 'Cancel invite' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Confirm cancelling of user invites',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to cancel all invites to releases under this publication for email address user4@test.com?',
    );

    expect(onUserRemove).not.toBeCalled();
    expect(onUserInvitesRemove).not.toBeCalled();

    userEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(onUserInvitesRemove).toHaveBeenCalledTimes(1);
      expect(onUserInvitesRemove).toHaveBeenCalledWith('user4@test.com');
      expect(onUserRemove).not.toBeCalled();
    });
  });
});
