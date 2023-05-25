import { testPaginatedReleaseSummaries } from '@admin/pages/publication/__data__/testReleases';
import PublicationInviteUsersPage from '@admin/pages/publication/PublicationInviteUsersPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationInviteUsersPageRoute,
  PublicationManageTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _publicationService from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationInviteUsersPage', () => {
  test('renders the page correctly', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit this publication'),
      ).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Enter an email address')).toBeInTheDocument();

    expect(
      screen.getByRole('group', {
        name: 'Select which releases you wish the user to have access',
      }),
    ).toBeInTheDocument();

    const checkboxes = screen.getAllByLabelText(/Academic year/);
    expect(checkboxes).toHaveLength(3);

    expect(screen.getByLabelText('Academic year 2023/24')).toBeInTheDocument();
    expect(screen.getByLabelText('Academic year 2022/23')).toBeInTheDocument();
    expect(screen.getByLabelText('Academic year 2021/22')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Invite user',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    ).toBeInTheDocument();
  });
});

function renderPage() {
  const path = generatePath<PublicationManageTeamRouteParams>(
    publicationInviteUsersPageRoute.path,
    {
      publicationId: testPublication.id,
      releaseId: testPaginatedReleaseSummaries.results[0].id,
    },
  );

  render(
    <MemoryRouter initialEntries={[path]}>
      <PublicationContextProvider
        publication={testPublication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <Route path={path} component={PublicationInviteUsersPage} />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
