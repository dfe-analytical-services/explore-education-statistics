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
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { generatePath, MemoryRouter, Route } from 'react-router';
import React, { ReactElement } from 'react';
import { Router } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, MemoryHistory } from 'history';
import { produce } from 'immer';
import baseRender from '@common-test/render';

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
      screen.getByText(/There are no publication owners or approvers\./),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /To edit the publication's owners or approvers please contact/,
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners\./),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');
  });

  test('renders the page correctly with a mix of publication roles assigned', async () => {
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
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: 'Owner',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
      {
        id: 'role-3',
        publication: 'publication',
        role: 'Approver',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
    ]);

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(
        /To edit the publication's owners or approvers please contact/,
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners\./),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells).toHaveLength(3);
    expect(headerCells[0]).toHaveTextContent('Name');
    expect(headerCells[1]).toHaveTextContent('Email');
    expect(headerCells[2]).toHaveTextContent('Publication role');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Analyst1 User1');
    expect(row1Cells[1]).toHaveTextContent('analyst1@example.com');
    expect(row1Cells[2]).toHaveTextContent('Owner');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row2Cells[2]).toHaveTextContent('Approver');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row3Cells[2]).toHaveTextContent('Owner');
  });

  test('renders the page correctly with only publication owners assigned', async () => {
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
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: 'Owner',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
    ]);

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(/There are no publication approvers\./),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/To edit the publication's owners or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners\./),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Name');
    expect(headerCells[1]).toHaveTextContent('Email');
    expect(headerCells[2]).toHaveTextContent('Publication role');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Analyst1 User1');
    expect(row1Cells[1]).toHaveTextContent('analyst1@example.com');
    expect(row1Cells[2]).toHaveTextContent('Owner');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row2Cells[2]).toHaveTextContent('Owner');
  });

  test('renders the page correctly with only publication approvers assigned', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummariesNoResults,
    );
    publicationService.listRoles.mockClear();
    publicationService.listRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication',
        role: 'Approver',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: 'Approver',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
    ]);

    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(/There are no publication owners\./),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication owners or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(
        /To edit the publication's owners or approvers please contact/,
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');

    expect(screen.getByRole('table')).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells[0]).toHaveTextContent('Name');
    expect(headerCells[1]).toHaveTextContent('Email');
    expect(headerCells[2]).toHaveTextContent('Publication role');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Analyst1 User1');
    expect(row1Cells[1]).toHaveTextContent('analyst1@example.com');
    expect(row1Cells[2]).toHaveTextContent('Approver');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row2Cells[2]).toHaveTextContent('Approver');
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
      expect(screen.getByTestId('Release')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update publication access' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect).toHaveValue('release-1');
    const releases = within(releaseSelect).queryAllByRole('option');
    expect(releases).toHaveLength(3);
    expect(releases[0]).toHaveTextContent('Academic year 2023/24');
    expect(releases[0]).toHaveValue('release-1');
    expect(releases[1]).toHaveTextContent('Academic year 2022/23');
    expect(releases[1]).toHaveValue('release-2');
    expect(releases[2]).toHaveTextContent('Academic year 2021/22');
    expect(releases[2]).toHaveValue('release-3');

    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status')).toHaveTextContent('Draft');

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1Cells[0]).getByText('User 1')).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('user1@test.com'),
    ).toBeInTheDocument();
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Remove User 1' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('');
    expect(row2Cells[1]).toHaveTextContent('user2@test.com');
    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'Cancel invite for user2@test.com',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Manage release contributors',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Invite new contributors',
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
      expect(screen.getByTestId('Release')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Publication access' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Release access' }),
    ).toBeInTheDocument();

    const releaseSelect = screen.getByLabelText('Select release');
    expect(releaseSelect).toHaveValue('release-1');
    const releases = within(releaseSelect).queryAllByRole('option');
    expect(releases).toHaveLength(3);
    expect(releases[0]).toHaveTextContent('Academic year 2023/24');
    expect(releases[0]).toHaveValue('release-1');
    expect(releases[1]).toHaveTextContent('Academic year 2022/23');
    expect(releases[1]).toHaveValue('release-2');
    expect(releases[2]).toHaveTextContent('Academic year 2021/22');
    expect(releases[2]).toHaveValue('release-3');

    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status')).toHaveTextContent('Draft');

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const headerCells = within(rows[0]).getAllByRole('columnheader');
    expect(headerCells).toHaveLength(2);
    expect(headerCells[0]).toHaveTextContent('Name');
    expect(headerCells[1]).toHaveTextContent('Email');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells).toHaveLength(2);
    expect(within(row1Cells[0]).getByText('User 1')).toBeInTheDocument();
    expect(
      within(row1Cells[1]).getByText('user1@test.com'),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('');
    expect(row2Cells[1]).toHaveTextContent('user2@test.com');
    expect(row2Cells[1]).toHaveTextContent('Pending invite');

    expect(
      screen.queryByRole('link', {
        name: 'Manage release contributors',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Invite new contributors',
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
        name: 'Manage release contributors',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Invite new contributors',
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
      expect(screen.getByTestId('Release')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update publication access' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-3');
    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2021/22',
    );
    expect(screen.getByTestId('Status')).toHaveTextContent('Approved');
  });

  test('selects the first release if no release is set in the url', async () => {
    publicationService.listReleases.mockResolvedValue(
      testPaginatedReleaseSummaries,
    );

    const history = createMemoryHistory();
    await renderPage({ history });

    await waitFor(() => {
      expect(screen.getByTestId('Release')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update publication access' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Update release access')).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status')).toHaveTextContent('Draft');

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
      expect(screen.getByTestId('Release')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Update publication access' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Update release access' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Select release')).toHaveValue('release-1');
    expect(screen.getByTestId('Release')).toHaveTextContent(
      'Academic year 2023/24 (Not live)',
    );
    expect(screen.getByTestId('Status')).toHaveTextContent('Draft');

    expect(history.location.pathname).toBe(
      `/publication/publication-1/team/release-1`,
    );

    userEvent.selectOptions(
      screen.getByLabelText('Select release'),
      'release-2',
    );

    expect(screen.getByLabelText('Select release')).toHaveValue('release-2');

    await waitFor(() => {
      expect(screen.getByTestId('Release')).toHaveTextContent(
        'Academic year 2022/23 (Not live)',
      );
    });
    expect(screen.getByTestId('Status')).toHaveTextContent('Draft');

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

function render(element: ReactElement) {
  return baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
