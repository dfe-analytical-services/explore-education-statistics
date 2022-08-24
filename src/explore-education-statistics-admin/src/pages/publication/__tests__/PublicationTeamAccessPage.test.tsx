import { testPaginatedReleaseSummaries } from '@admin/pages/publication/__data__/testReleases';
import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _publicationService from '@admin/services/publicationService';
import _releasePermissionService, {
  ContributorInvite,
  ContributorViewModel,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { Router } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, MemoryHistory } from 'history';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

const testContributors: ContributorViewModel[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User 1',
    userEmail: 'user1@test.com',
  },
];

const testInvites: ContributorInvite[] = [{ email: 'user2@test.com' }];

describe('PublicationTeamAccessPage', () => {
  beforeEach(() => {
    releasePermissionService.listReleaseContributors.mockResolvedValue(
      testContributors,
    );
    releasePermissionService.listReleaseContributorInvites.mockResolvedValue(
      testInvites,
    );
  });

  test('renders the page correctly with no releases', async () => {
    publicationService.listReleases.mockResolvedValue({
      results: [],
      paging: { page: 1, pageSize: 1, totalPages: 1, totalResults: 0 },
    });

    await renderPage({});

    expect(
      screen.getByText(
        'Create a release for this publication to manage team access.',
      ),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with releases', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );
    await renderPage({});

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect).toHaveValue('release-1');
    const releases = within(releaseSelect).queryAllByRole('option');
    expect(releases).toHaveLength(3);
    expect(releases[0]).toHaveTextContent('Academic Year 2023/24');
    expect(releases[0]).toHaveValue('release-1');
    expect(releases[1]).toHaveTextContent('Academic Year 2022/23');
    expect(releases[1]).toHaveValue('release-2');
    expect(releases[2]).toHaveTextContent('Academic Year 2021/22');
    expect(releases[2]).toHaveValue('release-3');

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2023/24 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);
    expect(within(rows[1]).getByText('User 1')).toBeInTheDocument();
    expect(within(rows[2]).getByText('user2@test.com')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Add or remove users',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Invite new users',
      }),
    ).toBeInTheDocument();
  });

  test('selects the release from the id in the url', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    await renderPage({
      releaseId: 'release-3',
    });

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2021/22 Approved',
      }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-3');
  });

  test('selects the first release if no release is set in the url', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    const history = createMemoryHistory();
    await renderPage({ history });

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');

    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2023/24 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-1`,
    );
  });

  test('updates the page and url when select a different release', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    const history = createMemoryHistory();
    await renderPage({ history });

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(
      screen.getByRole('heading', {
        name: 'Academic Year 2023/24 (Not live) Draft',
      }),
    ).toBeInTheDocument();
    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-1`,
    );

    userEvent.selectOptions(
      screen.getByLabelText('Select release'),
      'release-2',
    );

    expect(screen.getByLabelText('Select release')).toHaveValue('release-2');

    await waitFor(() => {
      expect(
        screen.getByRole('heading', {
          name: 'Academic Year 2022/23 (Not live) Draft',
        }),
      ).toBeInTheDocument();
    });
    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-2`,
    );
  });
});

async function renderPage({
  history = createMemoryHistory(),
  releaseId,
}: {
  history?: MemoryHistory;
  releaseId?: string;
}) {
  history.push(
    generatePath<PublicationTeamRouteParams>(publicationTeamAccessRoute.path, {
      publicationId: 'publication-1',
      releaseId,
    }),
  );

  render(
    <Router history={history}>
      <PublicationContextProvider
        publication={testPublication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <Route
          path={publicationTeamAccessRoute.path}
          component={PublicationTeamAccessPage}
        />
      </PublicationContextProvider>
    </Router>,
  );

  await waitFor(() => {
    expect(screen.getByText('Update release access')).toBeInTheDocument();
  });
}
