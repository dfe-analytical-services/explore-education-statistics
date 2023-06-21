import ReleaseUserTable from '@admin/pages/publication/components/ReleaseUserTable';
import {
  UserReleaseInvite,
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/userService');

describe('ReleaseUserTable', () => {
  const testReleaseContributors: UserReleaseRole[] = [
    {
      userId: 'user-1',
      userDisplayName: 'User 1',
      userEmail: 'user1@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-2',
      userDisplayName: 'User 2',
      userEmail: 'user2@test.com',
      role: 'Contributor',
    },
    {
      userId: 'user-3',
      userDisplayName: 'User 3',
      userEmail: 'user3@test.com',
      role: 'Contributor',
    },
  ];

  const testInvites: UserReleaseInvite[] = [
    {
      email: 'user4@test.com',
      role: 'Contributor',
    },
    {
      email: 'user5@test.com',
      role: 'Contributor',
    },
  ];

  test('renders the contributors table correctly', async () => {
    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseUserTable
        users={testReleaseContributors}
        invites={testInvites}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(6);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('User 1')).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('user1@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Remove User 1' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2Cells[0]).getByText('User 2')).toBeInTheDocument();
    expect(
      within(row2Cells[1]).getByText('user2@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'Remove User 2' }),
    ).toBeInTheDocument();

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3Cells[0]).getByText('User 3')).toBeInTheDocument();
    expect(
      within(row3Cells[1]).getByText('user3@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row3Cells[2]).getByRole('button', { name: 'Remove User 3' }),
    ).toBeInTheDocument();

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('');
    expect(
      within(row4Cells[1]).getByText('user4@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row4Cells[2]).getByRole('button', {
        name: 'Cancel invite for user4@test.com',
      }),
    ).toBeInTheDocument();

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('');
    expect(
      within(row5Cells[1]).getByText('user5@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row5Cells[2]).getByRole('button', {
        name: 'Cancel invite for user5@test.com',
      }),
    ).toBeInTheDocument();
  });

  test('remove user', async () => {
    const onUserRemove = jest.fn();
    const onUserInvitesRemove = jest.fn();

    render(
      <ReleaseUserTable
        users={testReleaseContributors}
        invites={[]}
        onUserRemove={onUserRemove}
        onUserInvitesRemove={onUserInvitesRemove}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('User 1')).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('user1@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Remove User 1' }),
    ).toBeInTheDocument();

    userEvent.click(
      within(rows[1]).getByRole('button', { name: 'Remove User 1' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Confirm user removal',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to remove User 1 from all releases in this publication?',
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
      <ReleaseUserTable
        users={[]}
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
