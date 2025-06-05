import UserInvitePage from '@admin/pages/users/UserInvitePage';
import {
  testPublicationSummaries,
  testRoles,
  testResourceRoles,
  testReleases,
} from '@admin/pages/users/__data__/testUserData';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService from '@admin/services/publicationService';
import _userService from '@admin/services/userService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route } from 'react-router';
import { administrationUserInviteRoute } from '@admin/routes/administrationRoutes';

jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/userService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const userService = _userService as jest.Mocked<typeof _userService>;

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
    expect(screen.getByLabelText('Role')).toBeInTheDocument();
    expect(screen.getByLabelText('Release role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add release role' }),
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

  describe('adding release roles', () => {
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

    test('shows validation error if release role is empty', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-releaseRole-error'),
      ).toHaveTextContent('Choose release role for the user');
    });

    test('shows validation error if try to add another role for a release', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');
      await user.selectOptions(
        screen.getByLabelText('Release role'),
        'Approver',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');
      await user.selectOptions(
        screen.getByLabelText('Release role'),
        'Contributor',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      expect(
        screen.getByTestId('inviteUserForm-releaseId-error'),
      ).toHaveTextContent('You can only add one role for each release');
    });

    test('successfully adds release roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');
      await user.selectOptions(
        screen.getByLabelText('Release role'),
        'Approver',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      const releaseRoleTable = within(screen.getByTestId('release-role-table'));
      const rows = releaseRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Release 1'));
      expect(within(rows[1]).getByText('Approver'));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 2');
      await user.selectOptions(
        screen.getByLabelText('Release role'),
        'Contributor',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      const updatedRows = releaseRoleTable.getAllByRole('row');
      expect(updatedRows).toHaveLength(3);

      expect(within(updatedRows[2]).getByText('Release 2'));
      expect(within(updatedRows[2]).getByText('Contributor'));
      expect(
        within(updatedRows[2]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();
    });

    test('successfully removes release roles', async () => {
      const { user } = renderPage();
      expect(
        await screen.findByText('Manage access to this service'),
      ).toBeInTheDocument();

      await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');
      await user.selectOptions(
        screen.getByLabelText('Release role'),
        'Approver',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add release role' }),
      );

      const releaseRoleTable = within(screen.getByTestId('release-role-table'));
      const rows = releaseRoleTable.getAllByRole('row');
      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('Release 1'));
      expect(within(rows[1]).getByText('Approver'));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.click(within(rows[1]).getByRole('button', { name: 'Remove' }));

      await waitFor(() =>
        expect(
          screen.queryByTestId('release-role-table'),
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
        'Approver',
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
        'Owner',
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
        'Approver',
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
      expect(within(rows[1]).getByText('Approver'));
      expect(
        within(rows[1]).getByRole('button', { name: 'Remove' }),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Publication'),
        'Publication 2',
      );
      await user.selectOptions(
        screen.getByLabelText('Publication role'),
        'Owner',
      );

      await user.click(
        screen.getByRole('button', { name: 'Add publication role' }),
      );

      const updatedRows = publicationRoleTable.getAllByRole('row');
      expect(updatedRows).toHaveLength(3);

      expect(within(updatedRows[2]).getByText('Publication 2'));
      expect(within(updatedRows[2]).getByText('Owner'));
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
        'Approver',
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
      expect(within(rows[1]).getByText('Approver'));
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

  test('submits successfully without release or publication roles', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText('User email'), 'test@test.com');

    expect(userService.inviteUser).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    await waitFor(() => {
      expect(userService.inviteUser).toHaveBeenCalledTimes(1);
    });
    expect(userService.inviteUser).toHaveBeenCalledWith({
      email: 'test@test.com',
      roleId: 'role-1-id',
      userReleaseRoles: [],
      userPublicationRoles: [],
    });
  });

  test('submits successfully with release and publication roles', async () => {
    const { user } = renderPage();
    expect(
      await screen.findByText('Manage access to this service'),
    ).toBeInTheDocument();

    await user.type(screen.getByLabelText('User email'), 'test@test.com');

    await user.selectOptions(screen.getByLabelText('Release'), 'Release 1');
    await user.selectOptions(screen.getByLabelText('Release role'), 'Approver');

    await user.click(screen.getByRole('button', { name: 'Add release role' }));

    await user.selectOptions(
      screen.getByLabelText('Publication'),
      'Publication 1',
    );
    await user.selectOptions(
      screen.getByLabelText('Publication role'),
      'Approver',
    );

    await user.click(
      screen.getByRole('button', { name: 'Add publication role' }),
    );

    expect(userService.inviteUser).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Send invite' }));

    await waitFor(() => {
      expect(userService.inviteUser).toHaveBeenCalledTimes(1);
    });
    expect(userService.inviteUser).toHaveBeenCalledWith({
      email: 'test@test.com',
      roleId: 'role-1-id',
      userReleaseRoles: [
        { releaseId: 'release-1-id', releaseRole: 'Approver' },
      ],
      userPublicationRoles: [
        { publicationId: 'publication-1-id', publicationRole: 'Allower' },
      ],
    });
  });

  const renderPage = () => {
    publicationService.getPublicationSummaries.mockResolvedValue(
      testPublicationSummaries,
    );
    userService.getRoles.mockResolvedValue(testRoles);
    userService.getResourceRoles.mockResolvedValue(testResourceRoles);
    userService.getReleases.mockResolvedValue(testReleases);

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
