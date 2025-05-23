import PublicationManageReleaseContributorsPage from '@admin/pages/publication/PublicationManageReleaseContributorsPage';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationManageReleaseContributorsPageRoute,
  PublicationManageTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _releaseVersionService, {
  ReleaseVersion,
} from '@admin/services/releaseVersionService';
import _releasePermissionService, {
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor } from '@testing-library/react';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

const testReleaseVersion: ReleaseVersion = {
  amendment: false,
  approvalStatus: 'Draft',
  id: 'release-1',
  releaseId: 'release-series-1',
  latestInternalReleaseNote: 'release1-release-note',
  latestRelease: true,
  live: false,
  preReleaseAccessList: '',
  previousVersionId: '',
  publicationId: 'publication-1',
  publicationTitle: 'Publication 1',
  publicationSlug: 'publication-slug-1',
  publishScheduled: '',
  slug: 'release-slug-1',
  timePeriodCoverage: {
    value: 'AY',
    label: 'Academic year',
  },
  title: 'Release 1',
  version: 0,
  label: undefined,
  type: 'AdHocStatistics',
  year: 2000,
  yearTitle: '2000/01',
  updatePublishedDate: false,
};

const testPublicationContributors: UserReleaseRole[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User Name 1',
    userEmail: 'user1@test.com',
    role: 'Contributor',
  },
  {
    userId: 'user-2',
    userDisplayName: 'User Name 2',
    userEmail: 'user2@test.com',
    role: 'Contributor',
  },
  {
    userId: 'user-3',
    userDisplayName: 'User Name 3',
    userEmail: 'user3@test.com',
    role: 'Contributor',
  },
  {
    userId: 'user-4',
    userDisplayName: 'User Name 4',
    userEmail: 'user4@test.com',
    role: 'Contributor',
  },
];
const testReleaseContributors: UserReleaseRole[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User Name 1',
    userEmail: 'user1@test.com',
    role: 'Contributor',
  },
  {
    userId: 'user-2',
    userDisplayName: 'User Name 2',
    userEmail: 'user2@test.com',
    role: 'Contributor',
  },
];
describe('PublicationManageReleaseContributorsPage', () => {
  test('renders the page correctly', async () => {
    releaseVersionService.getReleaseVersion.mockResolvedValue(
      testReleaseVersion,
    );
    releasePermissionService.listPublicationContributors.mockResolvedValue(
      testPublicationContributors,
    );
    releasePermissionService.listRoles.mockResolvedValue(
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
  const path = generatePath<PublicationManageTeamRouteParams>(
    publicationManageReleaseContributorsPageRoute.path,
    {
      publicationId: testPublication.id,
      releaseVersionId: testReleaseVersion.id,
    },
  );

  render(
    <MemoryRouter initialEntries={[path]}>
      <PublicationContextProvider
        publication={testPublication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <Route
          path={path}
          component={PublicationManageReleaseContributorsPage}
        />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
