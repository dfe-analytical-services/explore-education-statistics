import PublicationInviteUsersPage from '@admin/pages/publication/PublicationInviteUsersPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationInviteUsersPageRoute,
  PublicationManageTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _publicationService from '@admin/services/publicationService';
import { ReleaseSummary } from '@admin/services/releaseService';
import { render, screen, waitFor } from '@testing-library/react';
import { generatePath, match } from 'react-router';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, createLocation } from 'history';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

const testReleases: ReleaseSummary[] = [
  {
    id: 'release-1',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Release 1',
    releaseName: '2000',
    type: 'AdHocStatistics',
    publishScheduled: '',
    latestInternalReleaseNote: 'release1-release-note',
    approvalStatus: 'Draft',
    yearTitle: '2000/01',
    live: false,
  },
  {
    id: 'release-2',
    timePeriodCoverage: {
      value: 'AY',
      label: 'Academic Year',
    },
    title: 'Release 2',
    releaseName: '2001',
    type: 'AdHocStatistics',
    publishScheduled: '',
    latestInternalReleaseNote: 'release2-release-note',
    approvalStatus: 'Approved',
    yearTitle: '2001/02',
    live: true,
  },
];

describe('PublicationInviteUsersPage', () => {
  test('renders the page correctly', async () => {
    publicationService.getReleases.mockResolvedValue(testReleases);
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Invite a user to edit Publication 1'),
      ).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Enter an email address')).toBeInTheDocument();

    expect(
      screen.getByRole('group', {
        name: 'Select which releases you wish the user to have access',
      }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Release 1')).toBeInTheDocument();

    expect(screen.getByLabelText('Release 2')).toBeInTheDocument();

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
  const history = createMemoryHistory();
  const path = generatePath<PublicationManageTeamRouteParams>(
    publicationInviteUsersPageRoute.path,
    {
      publicationId: testPublication.id,
      releaseId: testReleases[0].id,
    },
  );

  const mockMatch: match<PublicationManageTeamRouteParams> = {
    isExact: false,
    path,
    url: path,
    params: {
      publicationId: testPublication.id,
      releaseId: testReleases[0].id,
    },
  };

  const routeComponentPropsMock = {
    history,
    location: createLocation(path),
    match: mockMatch,
  };

  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={testPublication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationInviteUsersPage {...routeComponentPropsMock} />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
