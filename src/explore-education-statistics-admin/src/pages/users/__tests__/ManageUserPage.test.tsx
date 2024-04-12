import ManageUserPage from '@admin/pages/users/ManageUserPage';
import {
  testPublicationSummaries,
  testRoles,
  testResourceRoles,
  testReleases,
  testUser,
} from '@admin/pages/users/__data__/testUserData';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService from '@admin/services/publicationService';
import _userService from '@admin/services/userService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route, generatePath } from 'react-router';
import { administrationUserManageRoute } from '@admin/routes/administrationRoutes';

jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/userService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const userService = _userService as jest.Mocked<typeof _userService>;

describe('ManageUserPage', () => {
  test('renders correctly', async () => {
    await renderPage();

    expect(
      screen.getByRole('heading', { name: 'Florian Schneider' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Details' }),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Name')).getByText('Name'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Name')).getByText('Florian Schneider'),
    ).toBeInTheDocument();

    expect(
      within(screen.getByTestId('Email')).getByText('Email'),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('Email')).getByText('test@test.com'),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Update role' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Release')).toBeInTheDocument();
    expect(screen.getByLabelText('Release role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add release access' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Publication')).toBeInTheDocument();
    expect(screen.getByLabelText('Publication role')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add publication access' }),
    ).toBeInTheDocument();
  });

  const renderPage = async () => {
    publicationService.getPublicationSummaries.mockResolvedValue(
      testPublicationSummaries,
    );
    userService.getRoles.mockResolvedValue(testRoles);
    userService.getResourceRoles.mockResolvedValue(testResourceRoles);
    userService.getReleases.mockResolvedValue(testReleases);
    userService.getUser.mockResolvedValue(testUser);

    render(
      <MemoryRouter
        initialEntries={[
          generatePath(administrationUserManageRoute.path, {
            userId: 'user-1-id',
          }),
        ]}
      >
        <TestConfigContextProvider>
          <Route
            component={ManageUserPage}
            path={administrationUserManageRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );

    expect(await screen.findByText('Details')).toBeInTheDocument();
  };
});
