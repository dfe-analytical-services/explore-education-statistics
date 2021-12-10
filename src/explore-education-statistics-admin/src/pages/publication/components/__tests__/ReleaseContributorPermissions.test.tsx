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

  test('renders a message when there are no release contributors', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue([]);

    const onUserRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={[]}
        onUserRemove={onUserRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('Warning')).toBeInTheDocument();

      expect(
        screen.getByTestId('releaseContributors-warning').textContent,
      ).toContain(
        'There are currently no team members associated with this publication.',
      );
    });
  });

  test('renders the contributors table correctly', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    const onUserRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        onUserRemove={onUserRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);

    expect(within(rows[0]).getByText('User Name 1')).toBeInTheDocument();
    expect(
      within(rows[0]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(within(rows[1]).getByText('User Name 2')).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(within(rows[2]).getByText('User Name 3')).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();
  });

  test('remove user', async () => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    const onUserRemove = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        onUserRemove={onUserRemove}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');

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

    userEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(onUserRemove).toHaveBeenCalledTimes(1);
      expect(onUserRemove).toHaveBeenCalledWith('user-1');
    });
  });
});
