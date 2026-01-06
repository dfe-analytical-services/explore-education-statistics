import ReleaseSeriesTable from '@admin/pages/publication/components/ReleaseSeriesTable';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testReleaseSeries } from '@admin/pages/publication/__data__/testReleaseSeries';
import { render, screen, waitFor, within } from '@testing-library/react';
import { Router } from 'react-router';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React from 'react';
import noop from 'lodash/noop';

describe('ReleaseSeriesTable', () => {
  const testPublicationId = 'publication-1';
  const testPublicationSlug = 'publication-1-slug';

  test('renders the release series table correctly', () => {
    render(
      <TestConfigContextProvider>
        <ReleaseSeriesTable
          canManageReleaseSeries
          isReordering={false}
          releaseSeries={testReleaseSeries}
          publicationId={testPublicationId}
          publicationSlug={testPublicationSlug}
          onCancelReordering={noop}
          onConfirmReordering={noop}
          onDelete={noop}
        />
      </TestConfigContextProvider>,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');
    expect(rows).toHaveLength(5);

    const tableHeaders = within(rows[0]).getAllByRole('columnheader');
    expect(tableHeaders[0]).toHaveTextContent('Description');
    expect(tableHeaders[1]).toHaveTextContent('URL');
    expect(tableHeaders[2]).toHaveTextContent('Status');
    expect(tableHeaders[3]).toHaveTextContent('Actions');

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('EES release 3');
    expect(row1Cells[1]).toHaveTextContent(
      'http://localhost/find-statistics/publication-1-slug/3',
    );
    expect(row1Cells[2]).toHaveTextContent('Unpublished');
    expect(within(row1Cells[2]).queryByRole('button')).not.toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('EES release 2');
    expect(
      within(row2Cells[1]).getByRole('link', {
        name: 'http://localhost/find-statistics/publication-1-slug/2',
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/find-statistics/publication-1-slug/2',
    );
    expect(row2Cells[2]).toHaveTextContent('Latest release');
    expect(within(row2Cells[3]).queryByRole('button')).not.toBeInTheDocument();

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('EES release 1');
    expect(
      within(row3Cells[1]).getByRole('link', {
        name: 'http://localhost/find-statistics/publication-1-slug/1',
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/find-statistics/publication-1-slug/1',
    );
    expect(row3Cells[2]).toBeEmptyDOMElement();
    expect(within(row3Cells[3]).queryByRole('button')).not.toBeInTheDocument();

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Legacy link 1');
    expect(
      within(row4Cells[1]).getByRole('link', {
        name: 'http://gov.uk/1',
      }),
    ).toHaveAttribute('href', 'http://gov.uk/1');
    expect(row4Cells[2]).toHaveTextContent('Legacy release');
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Edit Legacy link 1',
      }),
    ).toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Delete Legacy link 1',
      }),
    ).toBeInTheDocument();
  });

  describe('editing', () => {
    test('shows a warning modal when the edit release button is clicked', async () => {
      const user = userEvent.setup();
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <ReleaseSeriesTable
              canManageReleaseSeries
              isReordering={false}
              releaseSeries={testReleaseSeries}
              publicationId={testPublicationId}
              publicationSlug={testPublicationSlug}
              onCancelReordering={noop}
              onConfirmReordering={noop}
              onDelete={noop}
            />
          </TestConfigContextProvider>
        </Router>,
      );
      await user.click(
        screen.getByRole('button', { name: 'Edit Legacy link 1' }),
      );
      expect(await screen.findByText('Warning')).toBeInTheDocument();
      const modal = within(screen.getByRole('dialog'));
      expect(
        modal.getByRole('heading', { name: 'Edit legacy release' }),
      ).toBeInTheDocument();
      expect(
        modal.getByText(
          'All changes made to legacy releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'OK' })).toBeInTheDocument();
    });

    test('goes to the edit page when OK is clicked', async () => {
      const user = userEvent.setup();
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <ReleaseSeriesTable
              canManageReleaseSeries
              isReordering={false}
              releaseSeries={testReleaseSeries}
              publicationId={testPublicationId}
              publicationSlug={testPublicationSlug}
              onCancelReordering={noop}
              onConfirmReordering={noop}
              onDelete={noop}
            />
          </TestConfigContextProvider>
        </Router>,
      );
      await user.click(
        screen.getByRole('button', { name: 'Edit Legacy link 1' }),
      );

      expect(
        await screen.findByText('Edit legacy release'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));
      await user.click(modal.getByRole('button', { name: 'OK' }));

      await waitFor(() => {
        expect(history.location.pathname).toBe(
          `/publication/${testPublicationId}/releases/legacy/${testReleaseSeries[3].id}/edit`,
        );
      });
    });
  });

  test('does not show edit and delete actions when user does not have permission to manage the release series', () => {
    render(
      <TestConfigContextProvider>
        <ReleaseSeriesTable
          canManageReleaseSeries={false}
          isReordering={false}
          releaseSeries={testReleaseSeries}
          publicationId={testPublicationId}
          publicationSlug={testPublicationSlug}
          onCancelReordering={noop}
          onConfirmReordering={noop}
          onDelete={noop}
        />
        ,
      </TestConfigContextProvider>,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells).toHaveLength(3);
    expect(row4Cells[0]).toHaveTextContent('Legacy link 1');
    expect(
      within(row4Cells[1]).getByRole('link', {
        name: 'http://gov.uk/1 (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'http://gov.uk/1');
    expect(row4Cells[2]).toHaveTextContent('Legacy release');
    expect(
      within(table).queryByRole('button', {
        name: 'Edit Legacy link 1',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(table).queryByRole('button', {
        name: 'Delete Legacy link 1',
      }),
    ).not.toBeInTheDocument();
  });

  describe('deleting', () => {
    test('shows a warning modal when the delete release button is clicked', async () => {
      const user = userEvent.setup();
      render(
        <TestConfigContextProvider>
          <ReleaseSeriesTable
            canManageReleaseSeries
            isReordering={false}
            releaseSeries={testReleaseSeries}
            publicationId={testPublicationId}
            publicationSlug={testPublicationSlug}
            onCancelReordering={noop}
            onConfirmReordering={noop}
            onDelete={noop}
          />
        </TestConfigContextProvider>,
      );
      await user.click(
        screen.getByRole('button', { name: 'Delete Legacy link 1' }),
      );
      expect(
        await screen.findByText('Delete legacy release'),
      ).toBeInTheDocument();
      const modal = within(screen.getByRole('dialog'));
      expect(
        modal.getByText('Are you sure you want to delete this legacy release?'),
      ).toBeInTheDocument();
      expect(
        modal.getByText(
          'All changes made to legacy releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(
        modal.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    test('calls the `onDelete` handler when confirm is clicked', async () => {
      const user = userEvent.setup();
      const handleDelete = jest.fn();
      render(
        <TestConfigContextProvider>
          <ReleaseSeriesTable
            canManageReleaseSeries
            isReordering={false}
            releaseSeries={testReleaseSeries}
            publicationId={testPublicationId}
            publicationSlug={testPublicationSlug}
            onCancelReordering={noop}
            onConfirmReordering={noop}
            onDelete={handleDelete}
          />
        </TestConfigContextProvider>,
      );
      await user.click(
        screen.getByRole('button', { name: 'Delete Legacy link 1' }),
      );
      expect(
        await screen.findByText('Delete legacy release'),
      ).toBeInTheDocument();
      await user.click(
        within(screen.getByRole('dialog')).getByRole('button', {
          name: 'Confirm',
        }),
      );

      expect(handleDelete).toHaveBeenCalledWith('legacy-release-1');
    });
  });

  describe('reordering', () => {
    test('shows the reordering UI when `isReordering` is true', async () => {
      render(
        <TestConfigContextProvider>
          <ReleaseSeriesTable
            canManageReleaseSeries
            isReordering
            releaseSeries={testReleaseSeries}
            publicationId={testPublicationId}
            publicationSlug={testPublicationSlug}
            onCancelReordering={noop}
            onConfirmReordering={noop}
            onDelete={noop}
          />
        </TestConfigContextProvider>,
      );

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
  });
});
