import PublicationAccessForm from '@admin/pages/users/components/PublicationAccessForm';
import {
  testUser,
  testPublicationSummaries,
  testResourceRoles,
} from '@admin/pages/users/__data__/testUserData';
import _userService from '@admin/services/userService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/userService');
const userService = _userService as jest.Mocked<typeof _userService>;

describe('PublicationAccessForm', () => {
  test('renders the form', () => {
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
        publicationRoles={testResourceRoles.Publication}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const publicationSelect = screen.getByLabelText('Publication');
    const publications = within(publicationSelect).getAllByRole('option');
    expect(publications).toHaveLength(3);
    expect(publications[0]).toHaveTextContent('Publication 1');
    expect(publications[1]).toHaveTextContent('Publication 2');
    expect(publications[2]).toHaveTextContent('Publication 3');

    const roleSelect = screen.getByLabelText('Publication role');
    const roles = within(roleSelect).getAllByRole('option');
    expect(roles).toHaveLength(2);
    expect(roles[0]).toHaveTextContent('Approver');
    expect(roles[1]).toHaveTextContent('Owner');

    expect(
      screen.getByRole('button', { name: 'Add publication access' }),
    ).toBeInTheDocument();
  });

  test('can submit the form with the selected publication and role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
        publicationRoles={testResourceRoles.Publication}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    await user.selectOptions(screen.getByLabelText('Publication'), [
      'Publication 1',
    ]);
    await user.selectOptions(screen.getByLabelText('Publication role'), [
      'Approver',
    ]);

    expect(userService.addUserPublicationRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Add publication access' }),
    );

    await waitFor(() => {
      expect(userService.addUserPublicationRole).toHaveBeenCalledTimes(1);
    });

    expect(userService.addUserPublicationRole).toHaveBeenCalledWith(
      'user-1-id',
      {
        publicationId: 'publication-1-id',
        publicationRole: 'Allower',
      },
    );
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('can remove a role', async () => {
    const user = userEvent.setup();
    const handleUpdate = jest.fn();
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
        publicationRoles={testResourceRoles.Publication}
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    expect(userService.removeUserPublicationRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(removeButtons[0]);

    await waitFor(() => {
      expect(userService.removeUserPublicationRole).toHaveBeenCalledTimes(1);
    });

    expect(userService.removeUserPublicationRole).toHaveBeenCalledWith(
      'user-1-id',
      {
        id: 'pr-id-1',
        publication: 'Publication 1',
        role: 'Allower',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
    );
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('displays a table of publications', () => {
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
        publicationRoles={testResourceRoles.Publication}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(within(rows[1]).getByText('Publication 1')).toBeInTheDocument();
    expect(within(rows[1]).getByText('Approver')).toBeInTheDocument();
  });
});
