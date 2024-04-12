import RoleForm from '@admin/pages/users/components/RoleForm';
import { testUser, testRoles } from '@admin/pages/users/__data__/testUserData';
import _userService from '@admin/services/userService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/userService');
const userService = _userService as jest.Mocked<typeof _userService>;

describe('RoleForm', () => {
  test('renders the form', () => {
    render(<RoleForm roles={testRoles} user={testUser} onUpdate={noop} />);

    const roleSelect = screen.getByLabelText('Role');
    const roles = within(roleSelect).getAllByRole('option');
    expect(roles).toHaveLength(3);
    expect(roles[0]).toHaveTextContent('Choose role');
    expect(roles[1]).toHaveTextContent('Role 1');
    expect(roles[2]).toHaveTextContent('Role 2');

    expect(
      screen.getByRole('button', { name: 'Update role' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <RoleForm roles={testRoles} user={testUser} onUpdate={handleUpdate} />,
    );

    await user.selectOptions(screen.getByLabelText('Role'), ['Role 1']);

    expect(userService.updateUser).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Update role' }));

    await waitFor(() => {
      expect(userService.updateUser).toHaveBeenCalledTimes(1);
    });
    expect(userService.updateUser).toHaveBeenCalledWith('user-1-id', {
      roleId: 'role-1-id',
    });
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });
});
