import ReleaseContributorsPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import { Release } from '@admin/services/releaseService';
import _releasePermissionService, {
  ManageAccessPageContributor,
} from '@admin/services/releasePermissionService';
import _userService from '@admin/services/userService';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/releasePermissionService');
jest.mock('@admin/services/userService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;
const userService = _userService as jest.Mocked<typeof _userService>;

describe('ReleaseContributorPermissions', () => {
  const testRelease = {
    id: 'release-1',
    title: 'Release 1',
  } as Release;

  const testReleaseContributors: ManageAccessPageContributor[] = [
    {
      userId: 'user-1',
      userFullName: 'User Name 1',
      userEmail: 'user1@test.com',
      releaseRoleId: 'release-role-1',
    },
    {
      userId: 'user-2',
      userFullName: 'User Name 2',
      userEmail: 'user2@test.com',
      releaseRoleId: undefined,
    },
    {
      userId: 'user-3',
      userFullName: 'User Name 3',
      userEmail: 'user3@test.com',
      releaseRoleId: 'release-role-3',
    },
  ];

  test('renders a message when there are no release contributors', async () => {
    releasePermissionService.getPublicationReleaseContributors.mockResolvedValue(
      [],
    );

    const handleUserRemoval = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={[]}
        handleUserRemoval={handleUserRemoval}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('Warning')).toBeInTheDocument();

      expect(
        screen.getByTestId('releaseContributors-warning').textContent,
      ).toContain(
        'There are currently no team members associated to this publication.',
      );
    });
  });

  test('renders the contributors table correctly', async () => {
    releasePermissionService.getPublicationReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    const handleUserRemoval = (userId: string) => {};

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        handleUserRemoval={handleUserRemoval}
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
    releasePermissionService.getPublicationReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    const handleUserRemoval = jest.fn();

    render(
      <ReleaseContributorsPermissions
        contributors={testReleaseContributors}
        handleUserRemoval={handleUserRemoval}
      />,
    );

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');

    fireEvent.click(
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

    expect(handleUserRemoval).not.toBeCalled();

    fireEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleUserRemoval).toHaveBeenCalledTimes(1);
      expect(handleUserRemoval).toHaveBeenCalledWith('user-1');
    });
  });
});
