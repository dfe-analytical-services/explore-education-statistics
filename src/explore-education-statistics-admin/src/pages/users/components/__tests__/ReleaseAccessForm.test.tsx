import ReleaseAccessForm from '@admin/pages/users/components/ReleaseAccessForm';
import {
  testUser,
  testResourceRoles,
  testReleases,
} from '@admin/pages/users/__data__/testUserData';
import _userService from '@admin/services/userService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/userService');
const userService = _userService as jest.Mocked<typeof _userService>;

describe('ReleaseAccessForm', () => {
  test('renders the form', () => {
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testResourceRoles.Release}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const releaseSelect = screen.getByLabelText('Release');
    const releases = within(releaseSelect).getAllByRole('option');
    expect(releases).toHaveLength(3);
    expect(releases[0]).toHaveTextContent('Release 1');
    expect(releases[1]).toHaveTextContent('Release 2');
    expect(releases[2]).toHaveTextContent('Release 3');

    const roleSelect = screen.getByLabelText('Release role');
    const roles = within(roleSelect).getAllByRole('option');
    expect(roles).toHaveLength(3);
    expect(roles[0]).toHaveTextContent('Approver');
    expect(roles[1]).toHaveTextContent('Contributor');
    expect(roles[2]).toHaveTextContent('PrereleaseViewer');

    expect(
      screen.getByRole('button', { name: 'Add release access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected release and role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testResourceRoles.Release}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    await user.selectOptions(screen.getByLabelText('Release'), ['Release 1']);
    await user.selectOptions(screen.getByLabelText('Release role'), [
      'Contributor',
    ]);

    expect(userService.addUserReleaseRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Add release access' }),
    );

    await waitFor(() => {
      expect(userService.addUserReleaseRole).toHaveBeenCalledTimes(1);
    });

    expect(userService.addUserReleaseRole).toHaveBeenCalledWith('user-1-id', {
      releaseId: 'release-1-id',
      releaseRole: 'Contributor',
    });
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('can remove a role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testResourceRoles.Release}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    expect(userService.removeUserReleaseRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(removeButtons[0]);

    await waitFor(() => {
      expect(userService.removeUserReleaseRole).toHaveBeenCalledTimes(1);
    });

    expect(userService.removeUserReleaseRole).toHaveBeenCalledWith('rr-id-1');
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('displays a table of releases', () => {
    render(
      <ReleaseAccessForm
        releases={testReleases}
        releaseRoles={testResourceRoles.Release}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(within(rows[1]).getByText('Publication 1')).toBeInTheDocument();
    expect(within(rows[1]).getByText('Release 2')).toBeInTheDocument();
    expect(within(rows[1]).getByText('Contributor')).toBeInTheDocument();
  });
});
