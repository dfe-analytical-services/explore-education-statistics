import PublicationManageReleaseContributorsPage from '@admin/pages/publication/PublicationManageReleaseContributorsPage';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationManageReleaseContributorsPageRoute,
  PublicationManageTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _releaseService, { Release } from '@admin/services/releaseService';
import _releasePermissionService, {
  ContributorViewModel,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor } from '@testing-library/react';
import { generatePath, match } from 'react-router';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, createLocation } from 'history';

jest.mock('@admin/services/releaseService');
const releaseService = _releaseService as jest.Mocked<typeof _releaseService>;

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

const testRelease = {
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
} as Release;

const testPublicationContributors: ContributorViewModel[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User Name 1',
    userEmail: 'user1@test.com',
  },
  {
    userId: 'user-2',
    userDisplayName: 'User Name 2',
    userEmail: 'user2@test.com',
  },
  {
    userId: 'user-3',
    userDisplayName: 'User Name 3',
    userEmail: 'user3@test.com',
  },
  {
    userId: 'user-4',
    userDisplayName: 'User Name 4',
    userEmail: 'user4@test.com',
  },
];
const testReleaseContributors: ContributorViewModel[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User Name 1',
    userEmail: 'user1@test.com',
  },
  {
    userId: 'user-2',
    userDisplayName: 'User Name 2',
    userEmail: 'user2@test.com',
  },
];
describe('PublicationManageReleaseContributorsPage', () => {
  test('renders the page correctly', async () => {
    releaseService.getRelease.mockResolvedValue(testRelease);
    releasePermissionService.listPublicationContributors.mockResolvedValue(
      testPublicationContributors,
    );
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testReleaseContributors,
    );
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Manage release contributors (Release 1)'),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('group', {
        name: 'Select contributors for this release',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('User Name 1 (user1@test.com)'),
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText('User Name 2 (user2@test.com)'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Update contributors',
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
    PublicationManageReleaseContributorsPageRoute.path,
    {
      publicationId: testPublication.id,
      releaseId: testRelease.id,
    },
  );

  const mockMatch: match<PublicationManageTeamRouteParams> = {
    isExact: false,
    path,
    url: path,
    params: { publicationId: testPublication.id, releaseId: testRelease.id },
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
        <PublicationManageReleaseContributorsPage
          {...routeComponentPropsMock}
        />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
