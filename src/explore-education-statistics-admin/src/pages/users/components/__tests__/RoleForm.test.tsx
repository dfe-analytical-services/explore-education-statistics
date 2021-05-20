import RoleForm from '@admin/pages/users/components/RoleForm';
import { Role, User } from '@admin/services/userService';
import {
  render,
  screen,
  waitFor,
  fireEvent,
  within,
} from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('RoleForm', () => {
  const testUser: User = {
    id: 'user-guid-1',
    name: 'Florian Schneider',
    email: 'test@test.com',
    role: 'role-guid-1',
    userPublicationRoles: [],
    userReleaseRoles: [],
  };

  const testRoles: Role[] = [
    {
      id: 'role-guid-1',
      name: 'Analyst',
      normalizedName: 'ANALYST',
    },
    {
      id: 'role-guid-2',
      name: 'BAU user',
      normalizedName: 'BAU',
    },
  ];

  test('renders the form', () => {
    render(<RoleForm roles={testRoles} user={testUser} onSubmit={noop} />);

    const roleSelect = screen.getByLabelText('Role');
    const roles = within(roleSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(roles).toHaveLength(2);
    expect(roles[0]).toHaveTextContent(testRoles[0].name);
    expect(roles[1]).toHaveTextContent(testRoles[1].name);

    expect(
      screen.getByRole('button', { name: 'Update role' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected role', async () => {
    const handleSubmit = jest.fn();
    render(
      <RoleForm roles={testRoles} user={testUser} onSubmit={handleSubmit} />,
    );

    fireEvent.change(screen.getByLabelText('Role'), {
      target: { value: testRoles[1].id },
    });

    userEvent.click(screen.getByRole('button', { name: 'Update role' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(
        {
          selectedRoleId: testRoles[1].id,
        },
        expect.anything(),
      );
    });
  });
});
