import UserInvitePage from '@admin/pages/users/UserInvitePage';
import {
  testPublicationSummaries,
  testRoles,
  testReleases,
} from '@admin/pages/users/__data__/testUserData';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService from '@admin/services/publicationService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route } from 'react-router';
import { administrationUserInviteRoute } from '@admin/routes/administrationRoutes';
import _globalRolesService from '@admin/services/user-management/globalRolesService';
import _releaseService from '@admin/services/releaseService';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import _userInvitesService from '@admin/services/user-management/userInvitesService';

jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/userService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;
const globalRolesService = _globalRolesService as jest.Mocked<
  typeof _globalRolesService
>;
const userInvitesService = _userInvitesService as jest.Mocked<
  typeof _userInvitesService
>;

describe('UserInvitePage', () => {
  test('renders correctly', async () => {
    renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Invite user' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('User email')).toBeInTheDocument();
    expect(screen.getByLabelText('Role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add pre-release role' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Publication')).toBeInTheDocument();
    expect(screen.getByLabelText('Publication role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add publication role' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Send invite' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('shows validation error if user email is empty', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Provide the users email',
      }),
    ).toHaveAttribute('href', '#inviteUserForm-userEmail');

    expect(
      screen.getByTestId('inviteUserForm-userEmail-error'),
    ).toHaveTextContent('Provide the users email');
  });

  test('shows validation error if role is empty', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText('User email'), 'test@test.com');

    await user.selectOptions(screen.getByLabelText('Role'), 'Choose role');
    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Choose role for the user',
      }),
    ).toHaveAttribute('href', '#inviteUserForm-roleId');

    expect(screen.getByTestId('inviteUserForm-roleId-error')).toHaveTextContent(
      'Choose role for the user',
    );
  });

  describe('adding pre-release roles', () => {
    test('shows validation error if release is empty', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-releaseId-error'),
      ).toHaveTextContent('Choose release to give the user access to');
    });

    test('shows validation error if try to add another pre-release role for the same release', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');

      await user.click(
        screen.getByRole('button', { name: 'Add pre-release role' }),
      );

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');

      await user.click(
        screen.getByRole('button', { name: 'Add pre-release role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-releaseId-error'),
      ).toHaveTextContent(
        'You can only add one pre-release role for each release',
      );
    });

    test('successfully adds pre-release roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');

      await user.click(
        screen.getByRole('button', { name: 'Add pre-release role' }),
      );

      const preReleaseRoleTable = within(
        screen.getByTestId('pre-release-role-table'),
      );
      const rows = preReleaseRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Release 1'));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 2');

      await user.click(
        screen.getByRole('button', { name: 'Add pre-release role' }),
      );

      const updatedRows = preReleaseRoleTable.getAllByRole('row');
      expect(updatedRows).toHaveLength(3);

      expect(within(updatedRows[2]).getByText('Release 2'));
      expect(
        within(updatedRows[2]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();
    });

    test('successfully removes pre-release roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');

      await user.click(
        screen.getByRole('button', { name: 'Add pre-release role' }),
      );

      const preReleaseRoleTable = within(
        screen.getByTestId('pre-release-role-table'),
      );
      const rows = preReleaseRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Release 1'));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.click(within(rows[1]).getByRole('button', { name: 'Remove' }));

      await waitFor(() =>
        expect(
          screen.queryByTestId('pre-release-role-table'),
        ).not.toBeInTheDocument(),
      );
    });
  });

  describe('adding publication roles', () => {
    test('shows validation error if publication is empty', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-publicationId-error'),
      ).toHaveTextContent('Choose publication to give the user access to');
    });

    test('shows validation error if publication role is empty', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-publicationRole-error'),
      ).toHaveTextContent('Choose publication role for the user');
    });

    test('shows validation error if try to add another role for a publication', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 1',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        PublicationRole.Approver,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 1',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        PublicationRole.Drafter,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-publicationId-error'),
      ).toHaveTextContent('You can only add one role for each publication');
    });

    test('successfully adds publication roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 1',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        PublicationRole.Approver,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      const publicationRoleTable = within(
        screen.getByTestId('publication-role-table'),
      );
      const rows = publicationRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Publication 1'));
      expect(within(rows[1]).getByText(PublicationRole.Approver));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 2',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        PublicationRole.Drafter,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      const updatedRows = publicationRoleTable.getAllByRole('row');
      expect(updatedRows).toHaveLength(3);

      expect(within(updatedRows[2]).getByText('Publication 2'));
      expect(within(updatedRows[2]).getByText(PublicationRole.Drafter));
      expect(
        within(updatedRows[2]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();
    });

    test('successfully removes publication roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 1',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        PublicationRole.Approver,
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      const publicationRoleTable = within(
        screen.getByTestId('publication-role-table'),
      );
      const rows = publicationRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Publication 1'));
      expect(within(rows[1]).getByText(PublicationRole.Approver));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.click(within(rows[1]).getByRole('button', { name: 'Remove' }));

      await waitFor(() =>
        expect(
          screen.queryByTestId('publication-role-table'),
        ).not.toBeInTheDocument(),
      );
    });
  });

  test('submits successfully without pre-release or publication roles', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText('User email'), 'test@test.com');

    expect(userInvitesService.inviteUser).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    await waitFor(() => {
      expect(userInvitesService.inviteUser).toHaveBeenCalledTimes(1);
    });
    expect(userInvitesService.inviteUser).toHaveBeenCalledWith({
      email: 'test@test.com',
      roleId: 'role-1-id',
      userPreReleaseRoles: [],
      userPublicationRoles: [],
    });
  });

  test('submits successfully with pre-release and publication roles', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText('User email'), 'test@test.com');

    await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');

    await user.click(
      screen.getByRole('button', { name: 'Add pre-release role' }),
    );

    await user.selectOptions(
      screen.getByLabelText('Publication'),
      'Publication 1',
    );
    await user.selectOptions(
      screen.getByLabelText('Publication role'),
      PublicationRole.Approver,
    );

    await user.click(
      screen.getByRole('button', { name: 'Add publication role' }),
    );

    expect(userInvitesService.inviteUser).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    await waitFor(() => {
      expect(userInvitesService.inviteUser).toHaveBeenCalledTimes(1);
    });
    expect(userInvitesService.inviteUser).toHaveBeenCalledWith({
      email: 'test@test.com',
      roleId: 'role-1-id',
      userPreReleaseRoles: [{ releaseId: 'release-1-id' }],
      userPublicationRoles: [
        {
          publicationId: 'publication-1-id',
          publicationRole: PublicationRole.Approver,
        },
      ],
    });
  });

  const renderPage = () => {
    publicationService.getPublicationSummaries.mockResolvedValue(
      testPublicationSummaries,
    );
    globalRolesService.getRoles.mockResolvedValue(testRoles);
    releaseService.getReleases.mockResolvedValue(testReleases);

    return render(
      <MemoryRouter initialEntries={[administrationUserInviteRoute.path]}>
        <TestConfigContextProvider>
          <Route
            component={UserInvitePage}
            path={administrationUserInviteRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
