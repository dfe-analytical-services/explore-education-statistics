import PublicationAccessForm from '@admin/pages/users/components/PublicationAccessForm';
import {
  testUser,
  testPublicationSummaries,
} from '@admin/pages/users/__data__/testUserData';
import _publicationRolesService from '@admin/services/user-management/publicationRolesService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { PublicationRole } from '@admin/services/types/PublicationRole';

jest.mock('@admin/services/user-management/publicationRolesService');

const publicationRolesService = _publicationRolesService as jest.Mocked<
  typeof _publicationRolesService
>;

describe('PublicationAccessForm', () => {
  test('renders the form', () => {
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
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
    expect(roles[0]).toHaveTextContent(PublicationRole.Approver);
    expect(roles[1]).toHaveTextContent(PublicationRole.Drafter);

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
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    await user.selectOptions(screen.getByLabelText('Publication'), [
      'Publication 1',
    ]);
    await user.selectOptions(screen.getByLabelText('Publication role'), [
      PublicationRole.Approver,
    ]);

    expect(publicationRolesService.addPublicationRole).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Add publication access' }),
    );

    await waitFor(() => {
      expect(publicationRolesService.addPublicationRole).toHaveBeenCalledTimes(
        1,
      );
    });

    expect(publicationRolesService.addPublicationRole).toHaveBeenCalledWith(
      'user-1-id',
      {
        publicationId: 'publication-1-id',
        publicationRole: PublicationRole.Approver,
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
        user={testUser}
        onUpdate={handleUpdate}
      />,
    );

    const removeButtons = screen.getAllByRole('button', { name: 'Remove' });

    expect(
      publicationRolesService.removeUserPublicationRole,
    ).not.toHaveBeenCalled();
    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(removeButtons[0]);

    await waitFor(() => {
      expect(
        publicationRolesService.removeUserPublicationRole,
      ).toHaveBeenCalledTimes(1);
    });

    expect(
      publicationRolesService.removeUserPublicationRole,
    ).toHaveBeenCalledWith('pr-id-1');
    expect(handleUpdate).toHaveBeenCalledTimes(1);
  });

  test('displays a table of publications', () => {
    render(
      <PublicationAccessForm
        publications={testPublicationSummaries}
        user={testUser}
        onUpdate={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows.length).toBe(3);
    expect(within(rows[1]).getByText('Publication 1')).toBeInTheDocument();
    expect(
      within(rows[1]).getByText(PublicationRole.Approver),
    ).toBeInTheDocument();
  });
});
