import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import _legacyReleaseService, {
  LegacyRelease,
} from '@admin/services/legacyReleaseService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { Router } from 'react-router';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React from 'react';

jest.mock('@admin/services/legacyReleaseService');
const legacyReleaseService = _legacyReleaseService as jest.Mocked<
  typeof _legacyReleaseService
>;

describe('LegacyReleasesTable', () => {
  const testLegacyReleases: LegacyRelease[] = [
    {
      description: 'Legacy release 3',
      id: 'legacy-release-3',
      order: 3,
      url: 'http://gov.uk/3',
    },
    {
      description: 'Legacy release 2',
      id: 'legacy-release-2',
      order: 2,
      url: 'http://gov.uk/2',
    },
    {
      description: 'Legacy release 1',
      id: 'legacy-release-1',
      order: 1,
      url: 'http://gov.uk/1',
    },
  ];

  const testPublicationId = 'publication-1';

  test('renders the legacy releases table correctly', () => {
    render(
      <LegacyReleasesTable
        canManageLegacyReleases
        legacyReleases={testLegacyReleases}
        publicationId={testPublicationId}
      />,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');

    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells[0]).toHaveTextContent('Order');
    expect(row1Cells[1]).toHaveTextContent('Description');
    expect(row1Cells[2]).toHaveTextContent('URL');
    expect(row1Cells[3]).toHaveTextContent('Actions');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('3');
    expect(row2Cells[1]).toHaveTextContent('Legacy release 3');
    expect(row2Cells[2]).toHaveTextContent('http://gov.uk/3');
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Edit Legacy release 3',
      }),
    );
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Delete Legacy release 3',
      }),
    ).toBeInTheDocument();

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('2');
    expect(row3Cells[1]).toHaveTextContent('Legacy release 2');
    expect(row3Cells[2]).toHaveTextContent('http://gov.uk/2');
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Edit Legacy release 2',
      }),
    );
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Delete Legacy release 2',
      }),
    ).toBeInTheDocument();

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('1');
    expect(row4Cells[1]).toHaveTextContent('Legacy release 1');
    expect(row4Cells[2]).toHaveTextContent('http://gov.uk/1');
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Edit Legacy release 1',
      }),
    );
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Delete Legacy release 1',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder legacy releases' }),
    ).toBeInTheDocument();
  });

  test('shows a message when there are no legacy releases', () => {
    render(
      <LegacyReleasesTable
        canManageLegacyReleases
        legacyReleases={[]}
        publicationId={testPublicationId}
      />,
    );

    expect(
      screen.getByText('No legacy releases for this publication.'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();

    expect(screen.queryByRole('table')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Reorder legacy releases' }),
    ).not.toBeInTheDocument();
  });

  describe('editing', () => {
    test('shows a warning modal when the edit release button is clicked', async () => {
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <LegacyReleasesTable
            canManageLegacyReleases
            legacyReleases={testLegacyReleases}
            publicationId={testPublicationId}
          />
        </Router>,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Edit Legacy release 3' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading', { name: 'Edit legacy release' }));
      expect(
        modal.getByText(
          'All changes made to legacy releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'OK' })).toBeInTheDocument();
    });

    test('goes to the edit page when OK is clicked', async () => {
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <LegacyReleasesTable
            canManageLegacyReleases
            legacyReleases={testLegacyReleases}
            publicationId={testPublicationId}
          />
        </Router>,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Edit Legacy release 3' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'OK' }));

      await waitFor(() => {
        expect(history.location.pathname).toBe(
          `/publication/${testPublicationId}/legacy/${testLegacyReleases[0].id}/edit`,
        );
      });
    });
  });

  test('does not show edit and delete actions when user does not have permission to manage legacy releases', () => {
    render(
      <LegacyReleasesTable
        canManageLegacyReleases={false}
        legacyReleases={testLegacyReleases}
        publicationId={testPublicationId}
      />,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');

    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells).toHaveLength(3);
    expect(row1Cells[0]).toHaveTextContent('Order');
    expect(row1Cells[1]).toHaveTextContent('Description');
    expect(row1Cells[2]).toHaveTextContent('URL');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells).toHaveLength(3);
    expect(row2Cells[0]).toHaveTextContent('3');
    expect(row2Cells[1]).toHaveTextContent('Legacy release 3');
    expect(row2Cells[2]).toHaveTextContent('http://gov.uk/3');

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells).toHaveLength(3);
    expect(row3Cells[0]).toHaveTextContent('2');
    expect(row3Cells[1]).toHaveTextContent('Legacy release 2');
    expect(row3Cells[2]).toHaveTextContent('http://gov.uk/2');

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells).toHaveLength(3);
    expect(row4Cells[0]).toHaveTextContent('1');
    expect(row4Cells[1]).toHaveTextContent('Legacy release 1');
    expect(row4Cells[2]).toHaveTextContent('http://gov.uk/1');
  });

  describe('deleting', () => {
    test('shows a warning modal when the delete release button is clicked', async () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Delete Legacy release 3' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Delete legacy release')).toBeInTheDocument();
      });
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

    test('sends the delete request and updates the releases table when confirm is clicked', async () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Delete Legacy release 3' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Delete legacy release')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

      expect(legacyReleaseService.deleteLegacyRelease).toHaveBeenCalledWith(
        testLegacyReleases[0].id,
      );

      const table = within(screen.getByRole('table'));
      await waitFor(() => {
        expect(table.queryByText('Legacy release 3')).not.toBeInTheDocument();
      });
      const rows = table.getAllByRole('row');
      expect(rows).toHaveLength(3);
    });
  });

  describe('creating', () => {
    test('shows a warning modal when the create legacy release button is clicked', async () => {
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <LegacyReleasesTable
            canManageLegacyReleases
            legacyReleases={testLegacyReleases}
            publicationId={testPublicationId}
          />
        </Router>,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Create legacy release' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading', { name: 'Create legacy release' }));
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
      render(
        <Router history={history}>
          <LegacyReleasesTable
            canManageLegacyReleases
            legacyReleases={testLegacyReleases}
            publicationId={testPublicationId}
          />
        </Router>,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Create legacy release' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'OK' }));
      await waitFor(() => {
        expect(history.location.pathname).toBe(
          `/publication/${testPublicationId}/legacy/create`,
        );
      });
    });

    test('does not show button to create when user does not have permission to manage legacy releases', () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases={false}
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );

      expect(
        screen.queryByRole('button', { name: 'Create legacy release' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('reordering', () => {
    test('shows a warning modal when the reorder legacy releases button is clicked', async () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Reorder legacy releases' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByRole('heading', { name: 'Reorder legacy releases' }));
      expect(
        modal.getByText(
          'All changes made to legacy releases appear immediately on the public website.',
        ),
      ).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
      expect(modal.getByRole('button', { name: 'OK' })).toBeInTheDocument();
    });

    test('shows the reordering UI when OK is clicked', async () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Reorder legacy releases' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
      const modal = within(screen.getByRole('dialog'));
      userEvent.click(modal.getByRole('button', { name: 'OK' }));

      await waitFor(() => {
        expect(screen.getByText('Sort')).toBeInTheDocument();
      });

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

    test('does not show button to reorder when user does not have permission to manage legacy releases', () => {
      render(
        <LegacyReleasesTable
          canManageLegacyReleases={false}
          legacyReleases={testLegacyReleases}
          publicationId={testPublicationId}
        />,
      );

      expect(
        screen.queryByRole('button', { name: 'Reorder legacy releases' }),
      ).not.toBeInTheDocument();
    });
  });
});
