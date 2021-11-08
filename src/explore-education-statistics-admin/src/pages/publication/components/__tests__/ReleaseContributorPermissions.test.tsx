import ReleaseContributorsPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import { Release } from '@admin/services/releaseService';
import _releasePermissionService, {
  ReleaseContributor,
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

  const testReleaseContributors: ReleaseContributor[] = [
    {
      userId: 'user-1',
      userFullName: 'User Name 1',
      releaseId: 'release-1',
      releaseRoleId: 'release-role-1',
    },
    {
      userId: 'user-2',
      userFullName: 'User Name 2',
      releaseId: 'release-1',
    },
    {
      userId: 'user-3',
      userFullName: 'User Name 3',
      releaseId: 'release-1',
      releaseRoleId: 'release-role-3',
    },
  ];

  test('renders a message when there are no release contributors', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue([]);

    render(<ReleaseContributorsPermissions release={testRelease} />);

    await waitFor(() => {
      expect(screen.getByText('Warning')).toBeInTheDocument();

      expect(
        screen.getByTestId('releaseContributors-warning').textContent,
      ).toContain(
        'There are currently no team members associated to this publication.Please invite new users to join.',
      );
    });
  });

  test('renders the contributors table correctly', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    render(<ReleaseContributorsPermissions release={testRelease} />);

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(4);

    expect(within(rows[0]).getByText('User Name 1')).toBeInTheDocument();
    expect(within(rows[0]).getByText('Access granted')).toBeInTheDocument();
    expect(
      within(rows[0]).getByRole('button', { name: 'Remove access' }),
    ).toBeInTheDocument();
    expect(
      within(rows[0]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(within(rows[1]).getByText('User Name 2')).toBeInTheDocument();
    expect(within(rows[1]).getByText('No access')).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Grant access' }),
    ).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(within(rows[2]).getByText('User Name 3')).toBeInTheDocument();
    expect(within(rows[2]).getByText('Access granted')).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove access' }),
    ).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove user' }),
    ).toBeInTheDocument();

    expect(
      within(rows[3]).getByRole('button', { name: 'Grant access to all' }),
    ).toBeInTheDocument();
  });

  test('granting user access', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    userService.addUserReleaseRole.mockResolvedValue(true);

    render(<ReleaseContributorsPermissions release={testRelease} />);

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');

    fireEvent.click(
      within(rows[1]).getByRole('button', { name: 'Grant access' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Change Access for User Name 2',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to grant access for Release 1?',
    );

    fireEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(userService.addUserReleaseRole).toHaveBeenCalledWith('user-2', {
        releaseId: 'release-1',
        releaseRole: 'Contributor',
      });
    });
  });

  test('removing user access', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    userService.removeUserReleaseRole.mockResolvedValue(true);

    render(<ReleaseContributorsPermissions release={testRelease} />);

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');

    fireEvent.click(
      within(rows[0]).getByRole('button', { name: 'Remove access' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Change Access for User Name 1',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to remove access for Release 1?',
    );

    fireEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(userService.removeUserReleaseRole).toHaveBeenCalledWith(
        'release-role-1',
      );
    });
  });

  test('remove user', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    render(<ReleaseContributorsPermissions release={testRelease} />);

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

    fireEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    // TO DO:
    // await waitFor(() => {
    // Expect an endpoint to have been called
    // });
  });

  test('grant all users access', async () => {
    releasePermissionService.getReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );

    render(<ReleaseContributorsPermissions release={testRelease} />);

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
    });
    const rows = screen.getAllByRole('row');

    fireEvent.click(
      within(rows[3]).getByRole('button', { name: 'Grant access to all' }),
    );

    const modal = screen.getByRole('dialog');

    expect(
      within(modal).getByRole('heading', {
        name: 'Grant access to all listed users',
      }),
    ).toBeInTheDocument();

    expect(modal.textContent).toContain(
      'Are you sure you want to grant access to all listed users?',
    );

    fireEvent.click(within(modal).getByRole('button', { name: 'Confirm' }));

    // TO DO:
    // await waitFor(() => {
    // Expect an endpoint to have been called
    // });
  });
});
