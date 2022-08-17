import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _publicationService from '@admin/services/publicationService';
import { ReleaseListItem } from '@admin/services/releaseService';
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
import { PaginatedList } from '@common/services/types/pagination';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

const testReleases: ReleaseListItem[] = [
  {
    id: 'release-1',
    title: 'Release 1',
    approvalStatus: 'Draft',
    live: false,
  },
  {
    id: 'release-2',
    title: 'Release 2',
    approvalStatus: 'Approved',
    live: true,
  },
];

const paginatedTestReleases: PaginatedList<ReleaseListItem> = {
  results: testReleases,
  paging: {
    page: 1,
    pageSize: 2,
    totalPages: 1,
    totalResults: 2,
  },
};

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
    } as PaginatedList<ReleaseListItem>);
    await renderPage({});

    expect(
      screen.getByText(
        'Create a release for this publication to manage team access.',
      ),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with releases', async () => {
    publicationService.listReleases.mockResolvedValue(paginatedTestReleases);
    await renderPage({});

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect).toHaveValue('release-1');
    const releases = within(releaseSelect).queryAllByRole('option');
    expect(releases).toHaveLength(2);
    expect(releases[0]).toHaveTextContent('Release 1');
    expect(releases[0]).toHaveValue('release-1');
    expect(releases[1]).toHaveTextContent('Release 2');
    expect(releases[1]).toHaveValue('release-2');

    expect(
      screen.getByRole('heading', {
        name: 'Release 1 (Not live) Draft',
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
    publicationService.listReleases.mockResolvedValue(paginatedTestReleases);
    await renderPage({
      releaseId: 'release-2',
    });

    expect(
      screen.getByRole('heading', {
        name: 'Release 2 Approved',
      }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-2');
  });

  test('selects the first release if no release is set in the url', async () => {
    publicationService.listReleases.mockResolvedValue(paginatedTestReleases);
    const history = createMemoryHistory();
    await renderPage({ history });

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');

    expect(
      screen.getByRole('heading', {
        name: 'Release 1 (Not live) Draft',
      }),
    ).toBeInTheDocument();

    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-1`,
    );
  });

  test('updates the page and url when select a different release', async () => {
    publicationService.listReleases.mockResolvedValue(paginatedTestReleases);
    const history = createMemoryHistory();
    await renderPage({ history });

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(
      screen.getByRole('heading', {
        name: 'Release 1 (Not live) Draft',
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
          name: 'Release 2 Approved',
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
