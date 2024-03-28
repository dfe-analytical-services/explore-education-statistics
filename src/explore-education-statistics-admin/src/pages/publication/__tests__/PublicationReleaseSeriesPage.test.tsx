import PublicationReleaseSeriesPage from '@admin/pages/publication/PublicationReleaseSeriesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { testReleaseSeries } from '@admin/pages/publication/__data__/testReleaseSeries';
import { mapToReleaseSeriesItemUpdateRequest } from '@admin/pages/publication/PublicationEditReleaseSeriesLegacyLinkPage';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Router } from 'react-router-dom';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';

jest.mock('@admin/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationReleaseSeriesPage', () => {
  test('renders the release series page', async () => {
    publicationService.getReleaseSeries.mockResolvedValue(testReleaseSeries);

    renderPage(testPublication);
    expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

    const table = screen.getByRole('table');
    expect(within(table).getAllByRole('row')).toHaveLength(5);

    const rows = within(table).getAllByRole('row');
    expect(rows).toHaveLength(5);

    const tableHeaders = within(rows[0]).getAllByRole('columnheader');
    expect(tableHeaders[0]).toHaveTextContent('Description');
    expect(tableHeaders[1]).toHaveTextContent('URL');
    expect(tableHeaders[2]).toHaveTextContent('Status');
    expect(tableHeaders[3]).toHaveTextContent('Actions');

    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder releases' }),
    ).toBeInTheDocument();
  });

  test('shows a message when there are no releases', async () => {
    publicationService.getReleaseSeries.mockResolvedValue([]);
    renderPage(testPublication);

    expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

    expect(
      screen.getByText('No releases for this publication.'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();

    expect(screen.queryByRole('table')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Reorder releases' }),
    ).not.toBeInTheDocument();
  });

  describe('reordering', () => {
    test('shows a warning modal when the reorder releases button is clicked', async () => {
      const user = userEvent.setup();
      publicationService.getReleaseSeries.mockResolvedValue(testReleaseSeries);
      renderPage(testPublication);

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Reorder releases' }),
      );

      expect(await screen.findByText('Warning')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      expect(
        modal.getByRole('heading', { name: 'Reorder releases' }),
      ).toBeInTheDocument();
      expect(
        modal.getByText(
          'All changes made to releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'OK' })).toBeInTheDocument();
    });

    test('shows the reordering UI when OK is clicked', async () => {
      const user = userEvent.setup();
      publicationService.getReleaseSeries.mockResolvedValue(testReleaseSeries);
      renderPage(testPublication);

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Reorder releases' }),
      );

      expect(await screen.findByText('Warning')).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      await user.click(modal.getByRole('button', { name: 'OK' }));

      expect(await screen.findByText('Sort')).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Confirm order' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel reordering' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Edit release' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Delete release' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Create legacy release' }),
      ).not.toBeInTheDocument();
    });

    test('does not show button to reorder when user does not have permission to manage legacy releases', async () => {
      publicationService.getReleaseSeries.mockResolvedValue(testReleaseSeries);
      renderPage({
        ...testPublication,
        permissions: {
          ...testPublication.permissions,
          canManageReleaseSeries: false,
        },
      });

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Reorder releases' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('deleting', () => {
    test('sends the delete request and updates the releases table when confirm is clicked', async () => {
      const user = userEvent.setup();
      publicationService.getReleaseSeries.mockResolvedValueOnce(
        testReleaseSeries,
      );
      publicationService.getReleaseSeries.mockResolvedValueOnce([
        testReleaseSeries[0],
        testReleaseSeries[1],
        testReleaseSeries[2],
      ]);
      renderPage(testPublication);

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Delete Legacy link 1' }),
      );
      expect(
        await screen.findByText('Delete legacy release'),
      ).toBeInTheDocument();

      expect(publicationService.updateReleaseSeries).not.toHaveBeenCalled();

      await user.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      expect(publicationService.updateReleaseSeries).toHaveBeenCalledTimes(1);
      expect(publicationService.updateReleaseSeries).toHaveBeenCalledWith(
        testPublication.id,
        mapToReleaseSeriesItemUpdateRequest(
          testReleaseSeries.filter(rsi => rsi.id !== testReleaseSeries[3].id),
        ),
      );

      await waitFor(() => {
        expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      });

      const table = within(screen.getByRole('table'));
      await waitFor(() => {
        expect(table.queryByText('Legacy link 1')).not.toBeInTheDocument();
      });
      const rows = table.getAllByRole('row');
      expect(rows).toHaveLength(4);
    });
  });

  describe('creating', () => {
    test('shows a warning modal when the create legacy release button is clicked', async () => {
      const user = userEvent.setup();
      publicationService.getReleaseSeries.mockResolvedValueOnce(
        testReleaseSeries,
      );
      renderPage(testPublication);

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Create legacy release' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      expect(
        modal.getByRole('heading', { name: 'Create legacy release' }),
      ).toBeInTheDocument();
      expect(
        modal.getByText(
          'All changes made to legacy releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'OK' })).toBeInTheDocument();
    });

    test('goes to the create page when OK is clicked', async () => {
      const history = createMemoryHistory();
      const user = userEvent.setup();
      publicationService.getReleaseSeries.mockResolvedValueOnce(
        testReleaseSeries,
      );
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <PublicationContextProvider
              publication={testPublication}
              onPublicationChange={noop}
              onReload={noop}
            >
              <PublicationReleaseSeriesPage />
            </PublicationContextProvider>
          </TestConfigContextProvider>
        </Router>,
      );

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      await user.click(
        screen.getByRole('button', { name: 'Create legacy release' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      await user.click(
        within(screen.getByRole('dialog')).getByRole('button', { name: 'OK' }),
      );
      await waitFor(() => {
        expect(history.location.pathname).toBe(
          `/publication/${testPublication.id}/legacy/create`,
        );
      });
    });

    test('does not show button to create when user does not have permission to manage legacy releases', async () => {
      publicationService.getReleaseSeries.mockResolvedValueOnce(
        testReleaseSeries,
      );
      renderPage({
        ...testPublication,
        permissions: {
          ...testPublication.permissions,
          canManageReleaseSeries: false,
        },
      });

      expect(await screen.findByText('Legacy releases')).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Create legacy release' }),
      ).not.toBeInTheDocument();
    });
  });
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <TestConfigContextProvider>
        <PublicationContextProvider
          publication={publication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationReleaseSeriesPage />
        </PublicationContextProvider>
      </TestConfigContextProvider>
    </MemoryRouter>,
  );
}
