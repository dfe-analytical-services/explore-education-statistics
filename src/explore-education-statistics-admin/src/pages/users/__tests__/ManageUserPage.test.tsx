import ManageUserPage from '@admin/pages/users/ManageUserPage';
import {
  testPublicationSummaries,
  testReleases,
  testStandardUser,
} from '@admin/pages/users/__data__/testUserData';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService from '@admin/services/publicationService';
import _usersService from '@admin/services/user-management/usersService';
import _releaseService from '@admin/services/releaseService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route, generatePath } from 'react-router';
import { administrationUserManageRoute } from '@admin/routes/administrationRoutes';

jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/releaseService');
jest.mock('@admin/services/user-management/usersService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const usersService = _usersService as jest.Mocked<typeof _usersService>;
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

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

    expect(
      screen.getByRole('checkbox', {
        name: 'Super User',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Update access' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Release')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add pre-release access' }),
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
    releaseService.getReleases.mockResolvedValue(testReleases);
    usersService.getUser.mockResolvedValue(testStandardUser);

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
