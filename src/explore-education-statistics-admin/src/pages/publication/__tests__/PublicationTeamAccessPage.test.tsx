import {
  testPaginatedReleaseSummaries,
  testPaginatedReleaseSummariesNoResults,
} from '@admin/pages/publication/__data__/testReleases';
import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import _releasePermissionService, {
  UserReleaseInvite,
  UserReleaseRole,
} from '@admin/services/releasePermissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { generatePath, Route } from 'react-router';
import React from 'react';
import { Router } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, MemoryHistory } from 'history';
import { produce } from 'immer';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@admin/services/releasePermissionService');
const releasePermissionService = _releasePermissionService as jest.Mocked<
  typeof _releasePermissionService
>;

const testContributors: UserReleaseRole[] = [
  {
    userId: 'user-1',
    userDisplayName: 'User 1',
    userEmail: 'user1@test.com',
    role: 'Contributor',
  },
];

const testInvites: UserReleaseInvite[] = [
  {
    email: 'user2@test.com',
    role: 'Contributor',
  },
];

describe('PublicationTeamAccessPage', () => {
  beforeEach(() => {
    releasePermissionService.listRoles.mockResolvedValue(testContributors);
    releasePermissionService.listInvites.mockResolvedValue(testInvites);
    publicationService.listRoles.mockResolvedValue([]);
  });

  test('renders the page correctly without any publication roles assigned', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummariesNoResults,
    );

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText('There are no publication roles currently assigned.'),
    ).toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');
  });

  test('renders the page correctly with publication roles assigned', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummariesNoResults,
    );
    publicationService.listRoles.mockClear();
    publicationService.listRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication',
        role: 'Owner',
        userName: 'Analyst1 User1',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: 'Owner',
        userName: 'Analyst2 User2',
      },
      {
        id: 'role-3',
        publication: 'publication',
        role: 'Approver',
        userName: 'Analyst2 User2',
      },
    ]);

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(/To request changing the assigned publication roles/),
    ).toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Name');
    expect(headerCells[1]).toHaveTextContent('Publication role');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Analyst1 User1');
    expect(row1Cells[1]).toHaveTextContent('Owner');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('Approver');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row3Cells[1]).toHaveTextContent('Owner');
  });

  test('renders the page correctly with no releases', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummariesNoResults,
    );

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Create a release for this publication to manage release access.',
      ),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with releases', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

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

    expect(screen.getByTestId('Release-value')).toHaveTextContent(
      'Academic Year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);
    expect(within(rows[1]).getByText('User 1')).toBeInTheDocument();
    expect(within(rows[2]).getByText('user2@test.com')).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Add or remove release contributors',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Add or remove publication contributors',
      }),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with no contributor management permissions', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    await renderPage({
      publication: produce(testPublication, draft => {
        draft.permissions.canUpdateContributorReleaseRole = false;
      }),
    });

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Release access' }),
    ).toBeInTheDocument();

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

    expect(screen.getByTestId('Release-value')).toHaveTextContent(
      'Academic Year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);
    expect(within(rows[1]).getByText('User 1')).toBeInTheDocument();
    expect(within(rows[2]).getByText('user2@test.com')).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Add or remove release contributors',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Add or remove publication contributors',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the page correctly with no permission to view release access section', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );
    await renderPage({
      publication: produce(testPublication, draft => {
        draft.permissions.canViewReleaseTeamAccess = false;
      }),
    });

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(publicationService.listReleases).toHaveBeenCalled();

    expect(
      screen.queryByRole('heading', { name: 'Release access' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Update release access' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Add or remove release contributors',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Add or remove publication contributors',
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

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-3');
    expect(screen.getByTestId('Release-value')).toHaveTextContent(
      'Academic Year 2021/22',
    );
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Approved');
  });

  test('selects the first release if no release is set in the url', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    const history = createMemoryHistory();
    await renderPage({ history });

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(screen.getByText('Update release access')).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(screen.getByTestId('Release-value')).toHaveTextContent(
      'Academic Year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

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

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(screen.getByTestId('Release-value')).toHaveTextContent(
      'Academic Year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-1`,
    );

    userEvent.selectOptions(
      screen.getByLabelText('Select release'),
      'release-2',
    );

    expect(screen.getByLabelText('Select release')).toHaveValue('release-2');

    await waitFor(() => {
      expect(screen.getByTestId('Release-value')).toHaveTextContent(
        'Academic Year 2022/23 (Not live)',
      );
    });
    expect(screen.getByTestId('Status-value')).toHaveTextContent('Draft');

    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-2`,
    );
  });
});

async function renderPage({
  history = createMemoryHistory(),
  releaseId,
  publication,
}: {
  history?: MemoryHistory;
  releaseId?: string;
  publication?: PublicationWithPermissions;
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
        publication={publication ?? testPublication}
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
}
