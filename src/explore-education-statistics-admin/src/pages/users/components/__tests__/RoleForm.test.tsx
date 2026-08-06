import RoleForm from '@admin/pages/users/components/RoleForm';
import {
  testBauUser,
  testStandardUser,
} from '@admin/pages/users/__data__/testUserData';
import _usersService from '@admin/services/user-management/usersService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { GlobalRole } from '@admin/services/types/GlobalRole';

jest.mock('@admin/services/user-management/usersService');

const usersService = _usersService as jest.Mocked<typeof _usersService>;

describe('RoleForm', () => {
  test('renders the form for a Standard User', () => {
    render(<RoleForm user={testStandardUser} onUpdate={noop} />);

    const checkbox = screen.getByRole('checkbox', {
      name: 'Super User',
    });

    expect(checkbox).toBeInTheDocument();
    expect(checkbox).not.toBeChecked();

    expect(
      screen.getByRole('button', { name: 'Update access' }),
    ).toBeInTheDocument();
  });

  test('renders the form for a Super User', () => {
    render(<RoleForm user={testBauUser} onUpdate={noop} />);

    const checkbox = screen.getByRole('checkbox', {
      name: 'Super User',
    });

    expect(checkbox).toBeInTheDocument();
    expect(checkbox).toBeChecked();

    expect(
      screen.getByRole('button', { name: 'Update access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with a new global role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();

    render(<RoleForm user={testStandardUser} onUpdate={handleUpdate} />);

    const checkbox = screen.getByRole('checkbox', {
      name: 'Super User',
    });

    expect(checkbox).not.toBeChecked();

    await user.click(checkbox);

    expect(checkbox).toBeChecked();

    expect(usersService.updateUserGlobalRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Update access' }));

    await waitFor(() => {
      expect(usersService.updateUserGlobalRole).toHaveBeenCalledTimes(1);
    });

    expect(usersService.updateUserGlobalRole).toHaveBeenCalledWith(
      'user-1-id',
      {
        targetGlobalRole: GlobalRole.BauUser,
      },
    );
  });
});
