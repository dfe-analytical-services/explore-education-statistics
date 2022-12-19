import ReleaseContributorsPermissions from '@admin/pages/publication/components/ReleaseUserTable';
import {
  ContributorInvite,
  ContributorViewModel,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/userService');

describe('ReleaseContributorTable', () => {
  const testReleaseContributors: ContributorViewModel[] = [
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

  const testInvites: ContributorInvite[] = [
    { email: 'user4@test.com' },
    { email: 'user5@test.com' },
  ];

  test('renders the contributors table correctly', async () => {
    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        invites={testInvites}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('User Name 1 (user1@test.com)'));
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(6);

    expect(
      within(rows[1]).getByText('User Name 1 (user1@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Remove User Name 1' }),
    ).toBeInTheDocument();

    expect(
      within(rows[2]).getByText('User Name 2 (user2@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove User Name 2' }),
    ).toBeInTheDocument();

    expect(
      within(rows[3]).getByText('User Name 3 (user3@test.com)'),
    ).toBeInTheDocument();
    expect(
      within(rows[3]).getByRole('button', { name: 'Remove User Name 3' }),
    ).toBeInTheDocument();

    expect(within(rows[4]).getByText('user4@test.com')).toBeInTheDocument();
    expect(
      within(rows[4]).getByRole('button', {
        name: 'Cancel invite for user4@test.com',
      }),
    ).toBeInTheDocument();

    expect(within(rows[5]).getByText('user5@test.com')).toBeInTheDocument();
    expect(
      within(rows[5]).getByRole('button', {
        name: 'Cancel invite for user5@test.com',
      }),
    ).toBeInTheDocument();
  });

  test('remove user', async () => {
    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        invites={[]}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('User Name 1 (user1@test.com)'));
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(4);

    userEvent.click(
      within(rows[1]).getByRole('button', { name: 'Remove User Name 1' }),
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
    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={[]}
        invites={testInvites}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('user4@test.com'));
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);

    userEvent.click(
      within(rows[1]).getByRole('button', {
        name: 'Cancel invite for user4@test.com',
      }),
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
