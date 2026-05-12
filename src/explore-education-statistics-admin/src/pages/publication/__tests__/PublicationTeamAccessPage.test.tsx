import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import { PublicationWithPermissions } from '@admin/services/publicationService';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { Router } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, MemoryHistory } from 'history';
import { produce } from 'immer';
import baseRender from '@common-test/render';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import publicationRolesService, {
  UserPublicationRoleInvite,
} from '@admin/services/user-management/publicationRolesService';
import { UserPublicationRoleWithUser } from '@admin/services/types/userWithRoles';

jest.mock('@admin/services/user-management/publicationRolesService');

const mockedService = publicationRolesService as jest.Mocked<
  typeof publicationRolesService
>;

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });
}

function renderPage({
  history = createMemoryHistory(),
  publication,
}: {
  history?: MemoryHistory;
  publication?: PublicationWithPermissions;
} = {}) {
  const queryClient = createTestQueryClient();

  history.push(
    generatePath<PublicationTeamRouteParams>(publicationTeamAccessRoute.path, {
      publicationId: 'publication-1',
    }),
  );

  return baseRender(
    <QueryClientProvider client={queryClient}>
      <Router history={history}>
        <PublicationContextProvider
          publication={publication ?? testPublication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <Route
            path={publicationTeamAccessRoute.path}
            component={PublicationTeamAccessPage}
          />
        </PublicationContextProvider>
      </Router>
    </QueryClientProvider>,
  );
}

describe('PublicationTeamAccessPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders empty state correctly', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([]);
    mockedService.listPublicationRoleInvites.mockResolvedValue([]);

    renderPage();

    expect(
      await screen.findByRole('heading', { name: /manage team access/i }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/there are no drafters, or pending drafter invites/i),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/there are no approvers, or pending approver invites/i),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Enter an email address')).toBeInTheDocument();
  });

  test('renders approvers table with active + invited users', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication-1',
        role: PublicationRole.Approver,
        userId: 'user-1',
        userName: 'User 1',
        email: 'user1@test.com',
      },
    ] as UserPublicationRoleWithUser[]);

    mockedService.listPublicationRoleInvites.mockResolvedValue([
      {
        roleId: 'invite-1',
        role: PublicationRole.Approver,
        email: 'user2@test.com',
        userId: 'user-2',
      },
    ] as UserPublicationRoleInvite[]);

    renderPage();

    const table = await screen.findByTestId('publicationApproverRoles');
    expect(table).toBeInTheDocument();

    expect(within(table).getByText('User 1')).toBeInTheDocument();
    expect(within(table).getByText('user2@test.com')).toBeInTheDocument();
  });

  test('renders drafters table with active + invited users', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication-1',
        role: PublicationRole.Drafter,
        userId: 'user-1',
        userName: 'User 1',
        email: 'user1@test.com',
      },
    ] as UserPublicationRoleWithUser[]);

    mockedService.listPublicationRoleInvites.mockResolvedValue([
      {
        roleId: 'invite-1',
        role: PublicationRole.Drafter,
        email: 'user2@test.com',
        userId: 'user-2',
      },
    ] as UserPublicationRoleInvite[]);

    renderPage();

    const table = await screen.findByTestId('publicationDrafterRoles');
    expect(table).toBeInTheDocument();

    expect(screen.getByRole('button', { name: /Remove/i })).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: /Cancel invite/i }),
    ).toBeInTheDocument();
  });

  test('invites a drafter', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([]);
    mockedService.listPublicationRoleInvites.mockResolvedValue([]);
    mockedService.inviteDrafter.mockResolvedValue(true);

    renderPage();

    const input = await screen.findByRole('textbox');

    await userEvent.type(input, 'test@test.com');

    await userEvent.click(
      screen.getByRole('button', { name: /invite drafter/i }),
    );

    expect(mockedService.inviteDrafter).toHaveBeenCalledWith({
      publicationId: 'publication-1',
      email: 'test@test.com',
    });
  });

  test('removes an active drafter', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication-1',
        role: PublicationRole.Drafter,
        userId: 'user-1',
        userName: 'User 1',
        email: 'user1@test.com',
      },
    ] as UserPublicationRoleWithUser[]);

    mockedService.listPublicationRoleInvites.mockResolvedValue([]);

    mockedService.removePublicationDrafter.mockResolvedValue(true);

    renderPage();

    const table = await screen.findByTestId('publicationDrafterRoles');

    const removeButton = within(table).getAllByRole('button', {
      name: /Remove/i,
    })[0];

    await userEvent.click(removeButton);

    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(mockedService.removePublicationDrafter).toHaveBeenCalledWith(
      'role-1',
    );
  });

  test('cancels a pending drafter invite', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([]);
    mockedService.listPublicationRoleInvites.mockResolvedValue([
      {
        roleId: 'invite-1',
        role: PublicationRole.Drafter,
        email: 'user1@test.com',
        userId: 'user-1',
      },
    ] as UserPublicationRoleInvite[]);

    mockedService.removePublicationDrafter.mockResolvedValue(true);

    renderPage();

    const table = await screen.findByTestId('publicationDrafterRoles');

    const cancelInviteButton = within(table).getAllByRole('button', {
      name: /Cancel invite/i,
    })[0];

    await userEvent.click(cancelInviteButton);

    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(mockedService.removePublicationDrafter).toHaveBeenCalledWith(
      'invite-1',
    );
  });

  test('hides invite form when user cannot update drafters', async () => {
    mockedService.listPublicationRoles.mockResolvedValue([]);
    mockedService.listPublicationRoleInvites.mockResolvedValue([]);

    renderPage({
      publication: produce(testPublication, draft => {
        draft.permissions.canUpdateDrafters = false;
      }),
    });

    expect(
      screen.queryByLabelText('Enter an email address'),
    ).not.toBeInTheDocument();
  });
});
