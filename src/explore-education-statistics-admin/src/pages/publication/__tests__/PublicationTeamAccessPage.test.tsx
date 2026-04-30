import PublicationTeamAccessPage from '@admin/pages/publication/PublicationTeamAccessPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import {
  publicationTeamAccessRoute,
  PublicationTeamRouteParams,
} from '@admin/routes/publicationRoutes';
import { PublicationWithPermissions } from '@admin/services/publicationService';
import { screen, waitFor, within } from '@testing-library/react';
import { generatePath, MemoryRouter, Route } from 'react-router';
import React, { ReactNode } from 'react';
import { Router } from 'react-router-dom';
import noop from 'lodash/noop';
import { createMemoryHistory, MemoryHistory } from 'history';
import { produce } from 'immer';
import baseRender from '@common-test/render';
import { PublicationRole } from '@admin/services/types/PublicationRole';
import _publicationRolesService from '@admin/services/user-management/publicationRolesService';

jest.mock('@admin/services/publicationService');
const publicationRolesService = _publicationRolesService as jest.Mocked<
  typeof _publicationRolesService
>;

jest.mock('@admin/services/releasePermissionService');

describe('PublicationTeamAccessPage', () => {
  beforeEach(() => {
    publicationRolesService.listPublicationRoles.mockResolvedValue([]);
  });

  test('renders the page correctly without any publication roles assigned', async () => {
    await renderPage({});

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Update publication access' }),
      ).toBeInTheDocument();
    });

    expect(
      screen.getByText(/There are no publication drafters or approvers\./),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /To edit the publication's drafters or approvers please contact/,
      ),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication drafters\./),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');

    expect(
      screen.queryByRole('link', {
        name: 'Manage publication drafters',
      }),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with a mix of publication roles assigned', async () => {
    publicationRolesService.listPublicationRoles.mockClear();
    publicationRolesService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication',
        role: PublicationRole.Drafter,
        userId: 'user-1',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: PublicationRole.Drafter,
        userId: 'user-2',
        userName: 'Analyst2 User2',
        email: 'analyst2@example.com',
      },
      {
        id: 'role-3',
        publication: 'publication',
        role: PublicationRole.Approver,
        userId: 'user-2',
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
        /To edit the publication's drafters or approvers please contact/,
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication drafters or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication drafters\./),
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
    expect(row1Cells[2]).toHaveTextContent('Drafter');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row2Cells[2]).toHaveTextContent('Approver');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row3Cells[2]).toHaveTextContent('Drafter');

    expect(
      screen.queryByRole('link', {
        name: 'Manage publication drafters',
      }),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with only publication drafters assigned', async () => {
    publicationRolesService.listPublicationRoles.mockClear();
    publicationRolesService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication',
        role: PublicationRole.Drafter,
        userId: 'user-1',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: PublicationRole.Drafter,
        userId: 'user-2',
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
      screen.queryByText(/There are no publication drafters or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/To edit the publication's drafters or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication drafters\./),
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
    expect(row1Cells[2]).toHaveTextContent('Drafter');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Analyst2 User2');
    expect(row2Cells[1]).toHaveTextContent('analyst2@example.com');
    expect(row2Cells[2]).toHaveTextContent('Drafter');

    expect(
      screen.queryByRole('link', {
        name: 'Manage publication drafters',
      }),
    ).toBeInTheDocument();
  });

  test('renders the page correctly with only publication approvers assigned', async () => {
    publicationRolesService.listPublicationRoles.mockClear();
    publicationRolesService.listPublicationRoles.mockResolvedValue([
      {
        id: 'role-1',
        publication: 'publication',
        role: PublicationRole.Approver,
        userId: 'user-1',
        userName: 'Analyst1 User1',
        email: 'analyst1@example.com',
      },
      {
        id: 'role-2',
        publication: 'publication',
        role: PublicationRole.Approver,
        userId: 'user-2',
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
      screen.getByText(/There are no publication drafters\./),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/There are no publication drafters or approvers\./),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText(
        /To edit the publication's drafters or approvers please contact/,
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

  test('renders the page correctly with no drafter management permissions', async () => {
    await renderPage({
      publication: produce(testPublication, draft => {
        draft.permissions.canUpdateDrafters = false;
      }),
    });

    expect(
      screen.getByRole('heading', { name: 'Publication access' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Manage publication drafters',
      }),
    ).not.toBeInTheDocument();
  });
});

async function renderPage({
  history = createMemoryHistory(),
  publication,
}: {
  history?: MemoryHistory;
  releaseVersionId?: string;
  publication?: PublicationWithPermissions;
}) {
  history.push(
    generatePath<PublicationTeamRouteParams>(publicationTeamAccessRoute.path, {
      publicationId: 'publication-1',
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

function render(element: ReactNode) {
  return baseRender(<MemoryRouter>{element}</MemoryRouter>);
}
