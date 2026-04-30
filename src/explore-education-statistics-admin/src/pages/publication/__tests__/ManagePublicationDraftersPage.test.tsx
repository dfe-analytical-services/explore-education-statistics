import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  managePublicationDraftersPageRoute,
  ManagePublicationDraftersRouteParams,
} from '@admin/routes/publicationRoutes';
import userEvent from '@testing-library/user-event';
import { render, screen, waitFor } from '@testing-library/react';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import _publicationRolesService from '@admin/services/user-management/publicationRolesService';
import { UserPublicationRoleWithUser } from '@admin/services/types/userWithRoles';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import ManagePublicationDraftersPage from '../ManagePublicationDraftersPage';

jest.mock('@admin/services/user-management/publicationRolesService');
const publicationRolesService = _publicationRolesService as jest.Mocked<
  typeof _publicationRolesService
>;

const testPublicationRoles: UserPublicationRoleWithUser[] = [
  {
    id: 'role-1',
    publication: 'publication 1',
    role: PublicationRole.Drafter,
    userId: 'user-1',
    userName: 'User Name 1',
    email: 'user1@test.com',
  },
  {
    id: 'role-2',
    publication: 'publication 2',
    role: PublicationRole.Drafter,
    userId: 'user-2',
    userName: 'User Name 2',
    email: 'user2@test.com',
  },
  {
    id: 'role-3',
    publication: 'publication 3',
    role: PublicationRole.Approver,
    userId: 'user-3',
    userName: 'User Name 3',
    email: 'user3@test.com',
  },
  {
    id: 'role-4',
    publication: 'publication 4',
    role: PublicationRole.Drafter,
    userId: 'user-4',
    userName: 'User Name 4',
    email: 'user4@test.com',
  },
  {
    id: 'role-5',
    publication: 'publication 5',
    role: PublicationRole.Drafter,
    userId: 'user-5',
    userName: 'User Name 5',
    email: 'user5@test.com',
  },
];

describe('ManagePublicationDraftersPage', () => {
  test('renders the page correctly', async () => {
    renderPage();

    publicationRolesService.listPublicationRoles.mockResolvedValue(
      testPublicationRoles,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit this publication'),
      ).toBeInTheDocument();
      expect(
        screen.getByText('Edit drafters for this publication'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 1 (user1@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 2 (user2@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 4 (user4@test.com)'),
      ).toBeInTheDocument();
      expect(
        screen.getByLabelText('User Name 5 (user5@test.com)'),
      ).toBeInTheDocument();
    });

    // User 3 should be filtered out due to them being an Approver
    expect(
      screen.getByLabelText('User Name 3 (user3@test.com)'),
    ).not.toBeInTheDocument();

    const checkboxes = screen.getAllByLabelText(
      /User Name /,
    ) as HTMLInputElement[];
    expect(checkboxes).toHaveLength(4);

    expect(checkboxes[0].checked).toBe(true);
    expect(checkboxes[0]).toHaveAttribute('value', 'user-1');

    expect(checkboxes[1].checked).toBe(true);
    expect(checkboxes[1]).toHaveAttribute('value', 'user-2');

    expect(checkboxes[2].checked).toBe(true);
    expect(checkboxes[2]).toHaveAttribute('value', 'user-4');

    expect(checkboxes[2].checked).toBe(true);
    expect(checkboxes[2]).toHaveAttribute('value', 'user-5');

    expect(screen.getByLabelText('Enter an email address')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Invite user',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Update drafters',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    ).toBeInTheDocument();
  });

  test('submits correct request when inviting a new user', async () => {
    renderPage();

    publicationRolesService.listPublicationRoles.mockResolvedValue([]);

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit this publication'),
      ).toBeInTheDocument();
    });

    const emailInput = screen.getByLabelText('Enter an email address');
    await userEvent.type(emailInput, 'test@test.com');

    expect(publicationRolesService.inviteDrafter).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: 'Invite user' }));

    await waitFor(() => {
      expect(publicationRolesService.inviteDrafter).toHaveBeenCalledTimes(1);
      expect(publicationRolesService.inviteDrafter).toHaveBeenCalledWith({
        email: 'test@test.com',
        publicationId: 'publication-id',
      });
    });
  });

  test('submits correct request when updating drafters', async () => {
    renderPage();

    publicationRolesService.listPublicationRoles.mockResolvedValue(
      testPublicationRoles,
    );

    const checkboxes = screen.getAllByLabelText(
      /User Name /,
    ) as HTMLInputElement[];

    await userEvent.click(checkboxes[0]);
    await userEvent.click(checkboxes[2]);

    await waitFor(() => {
      expect(checkboxes[0].checked).toBe(false);
      expect(checkboxes[1].checked).toBe(true);
      expect(checkboxes[2].checked).toBe(false);
      expect(checkboxes[3].checked).toBe(true);
    });

    expect(
      publicationRolesService.updatePublicationDrafters,
    ).not.toHaveBeenCalled();

    await userEvent.click(
      screen.getByRole('button', { name: 'Update drafters' }),
    );

    await waitFor(() => {
      expect(
        publicationRolesService.updatePublicationDrafters,
      ).toHaveBeenCalledTimes(1);
      expect(
        publicationRolesService.updatePublicationDrafters,
      ).toHaveBeenCalledWith(
        testPublication.id,
        expect.arrayContaining(['user-2', 'user-5']),
      );
    });
  });

  test('shows an error if no email is entered when inviting a new user', async () => {
    renderPage();

    publicationRolesService.listPublicationRoles.mockResolvedValue([]);

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit this publication'),
      ).toBeInTheDocument();
    });

    expect(publicationRolesService.inviteDrafter).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: 'Invite user' }));

    await waitFor(() => {
      expect(publicationRolesService.inviteDrafter).toHaveBeenCalledTimes(0);
      expect(
        screen.getByText('Enter an email address', {
          selector: '#inviteDrafterForm-email-error',
        }),
      );
    });
  });

  test('displays correct message when no drafters exist', async () => {
    renderPage();

    publicationRolesService.listPublicationRoles.mockResolvedValue([]);

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit this publication'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText('There are no drafters for this publication.'),
    ).toBeInTheDocument();

    const checkboxes = screen.getAllByLabelText(
      /User Name /,
    ) as HTMLInputElement[];
    expect(checkboxes).toHaveLength(0);
  });
});

function renderPage() {
  const path = generatePath<ManagePublicationDraftersRouteParams>(
    managePublicationDraftersPageRoute.path,
    {
      publicationId: testPublication.id,
    },
  );

  render(
    <MemoryRouter initialEntries={[path]}>
      <PublicationContextProvider
        publication={testPublication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <Route path={path} component={ManagePublicationDraftersPage} />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
