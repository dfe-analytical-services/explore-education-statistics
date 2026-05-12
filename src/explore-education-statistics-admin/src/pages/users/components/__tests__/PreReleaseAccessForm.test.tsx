import PreReleaseAccessForm from '@admin/pages/users/components/PreReleaseAccessForm';
import {
  testUser,
  testReleases,
} from '@admin/pages/users/__data__/testUserData';
import _preReleaseUsersService from '@admin/services/user-management/preReleaseUsersService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/user-management/preReleaseUsersService');
const preReleaseUsersService = _preReleaseUsersService as jest.Mocked<
  typeof _preReleaseUsersService
>;

describe('PreReleaseAccessForm', () => {
  test('renders the form', () => {
    render(
      <PreReleaseAccessForm
        releases={testReleases}
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

    expect(
      screen.getByRole('button', { name: 'Add pre-release access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected release and role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <PreReleaseAccessForm
        releases={testReleases}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    await user.selectOptions(screen.getByLabelText('Release'), ['Release 1']);

    expect(preReleaseUsersService.grantPreReleaseAccess).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Add pre-release access' }),
    );

    await waitFor(() => {
      expect(
        preReleaseUsersService.grantPreReleaseAccess,
      ).toHaveBeenCalledTimes(1);
    });

    expect(preReleaseUsersService.grantPreReleaseAccess).toHaveBeenCalledWith(
      'user-1-id',
      'release-1-id',
    );
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('can remove a role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <PreReleaseAccessForm
        releases={testReleases}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    expect(
      preReleaseUsersService.removePreReleaseRoleById,
    ).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(removeButtons[0]);

    await waitFor(() => {
      expect(
        preReleaseUsersService.removePreReleaseRoleById,
      ).toHaveBeenCalledTimes(1);
    });

    expect(
      preReleaseUsersService.removePreReleaseRoleById,
    ).toHaveBeenCalledWith('rr-id-1');
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('displays a table of releases', () => {
    render(
      <PreReleaseAccessForm
        releases={testReleases}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(within(rows[1]).getByText('Publication 1')).toBeInTheDocument();
    expect(within(rows[1]).getByText('Release 2')).toBeInTheDocument();
  });
});
