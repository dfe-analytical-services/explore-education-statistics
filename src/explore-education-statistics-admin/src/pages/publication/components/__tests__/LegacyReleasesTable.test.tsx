import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import _legacyReleaseService, {
  CombinedRelease,
} from '@admin/services/legacyReleaseService';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
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
  const testCombinedReleases: CombinedRelease[] = [
    {
      description: 'EES release 3 amendment',
      id: 'ees-release-3a',
      url: 'http://explore-education-statistics/3a',
      order: 6,
      isDraft: true,
      isLegacy: false,
      isAmendment: true,
      isLatest: false,
    },
    {
      description: 'EES release 3',
      id: 'ees-release-3',
      url: 'http://explore-education-statistics/3',
      order: 6,
      isDraft: false,
      isLegacy: false,
      isAmendment: false,
      isLatest: true,
    },
    {
      description: 'EES release 2',
      id: 'ees-release-2',
      url: 'http://explore-education-statistics/2',
      order: 5,
      isDraft: true,
      isLegacy: false,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'EES release 1',
      id: 'ees-release-1',
      url: 'http://explore-education-statistics/1',
      order: 4,
      isDraft: false,
      isLegacy: false,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'Legacy release 3',
      id: 'legacy-release-3',
      url: 'http://gov.uk/3',
      order: 3,
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'Legacy release 2',
      id: 'legacy-release-2',
      url: 'http://gov.uk/2',
      order: 2,
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'Legacy release 1',
      id: 'legacy-release-1',
      url: 'http://gov.uk/1',
      order: 1,
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
  ];

  const testPublicationId = 'publication-1';

  test('renders the legacy releases table correctly', () => {
    render(
      <TestConfigContextProvider>
        <LegacyReleasesTable
          canManageLegacyReleases
          combinedReleases={testCombinedReleases}
          publicationId={testPublicationId}
        />
      </TestConfigContextProvider>,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');

    expect(rows).toHaveLength(8);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells[0]).toHaveTextContent('Description');
    expect(row1Cells[1]).toHaveTextContent('URL');
    expect(row1Cells[2]).toHaveTextContent('Actions');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'EES release 3 amendmentDraft Amendment',
    );
    expect(row2Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/3a',
    );
    expect(within(row2Cells[2]).queryByRole('button')).not.toBeInTheDocument();

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('EES release 3Latest');
    expect(row3Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/3',
    );
    expect(within(row2Cells[2]).queryByRole('button')).not.toBeInTheDocument();

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('EES release 2Draft');
    expect(row4Cells[1]).toHaveTextContent('');
    expect(within(row4Cells[2]).queryByRole('button')).not.toBeInTheDocument();

    const row5Cells = within(rows[4]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('EES release 1');
    expect(row5Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/1',
    );
    expect(within(row5Cells[2]).queryByRole('button')).not.toBeInTheDocument();

    const row6Cells = within(rows[5]).getAllByRole('cell');
    expect(row6Cells[0]).toHaveTextContent('Legacy release 3');
    expect(row6Cells[1]).toHaveTextContent('http://gov.uk/3');
    expect(
      within(row6Cells[2]).getByRole('button', {
        name: 'Edit Legacy release 3',
      }),
    ).toBeInTheDocument();
    expect(
      within(row6Cells[2]).getByRole('button', {
        name: 'Delete Legacy release 3',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder releases' }),
    ).toBeInTheDocument();
  });

  test('shows a message when there are no legacy releases', () => {
    render(
      <TestConfigContextProvider>
        <LegacyReleasesTable
          canManageLegacyReleases
          combinedReleases={[]}
          publicationId={testPublicationId}
        />
      </TestConfigContextProvider>,
    );

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

  describe('editing', () => {
    test('shows a warning modal when the edit release button is clicked', async () => {
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <LegacyReleasesTable
              canManageLegacyReleases
              combinedReleases={testCombinedReleases}
              publicationId={testPublicationId}
            />
          </TestConfigContextProvider>
        </Router>,
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Edit Legacy release 3' }),
      );
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
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
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <LegacyReleasesTable
              canManageLegacyReleases
              combinedReleases={testCombinedReleases}
              publicationId={testPublicationId}
            />
          </TestConfigContextProvider>
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
          `/publication/${testPublicationId}/legacy/${testCombinedReleases[4].id}/edit`,
        );
      });
    });
  });

  test('does not show edit and delete actions when user does not have permission to manage legacy releases', () => {
    render(
      <TestConfigContextProvider>
        <LegacyReleasesTable
          canManageLegacyReleases={false}
          combinedReleases={testCombinedReleases}
          publicationId={testPublicationId}
        />
        ,
      </TestConfigContextProvider>,
    );

    const table = screen.getByRole('table');
    const rows = within(table).getAllByRole('row');

    expect(rows).toHaveLength(8);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells).toHaveLength(2);
    expect(row1Cells[0]).toHaveTextContent('Description');
    expect(row1Cells[1]).toHaveTextContent('URL');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells).toHaveLength(2);
    expect(row2Cells[0]).toHaveTextContent(
      'EES release 3 amendmentDraft Amendment',
    );
    expect(row2Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/3a',
    );

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells).toHaveLength(2);
    expect(row3Cells[0]).toHaveTextContent('EES release 3');
    expect(row3Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/3',
    );

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells).toHaveLength(2);
    expect(row4Cells[0]).toHaveTextContent('EES release 2');
    expect(row4Cells[1]).toHaveTextContent('');

    const row5Cells = within(rows[4]).getAllByRole('cell');
    expect(row5Cells).toHaveLength(2);
    expect(row5Cells[0]).toHaveTextContent('EES release 1');
    expect(row5Cells[1]).toHaveTextContent(
      'http://explore-education-statistics/1',
    );

    const row6Cells = within(rows[5]).getAllByRole('cell');
    expect(row6Cells).toHaveLength(2);
    expect(row6Cells[0]).toHaveTextContent('Legacy release 3');
    expect(row6Cells[1]).toHaveTextContent('http://gov.uk/3');

    const row7Cells = within(rows[6]).getAllByRole('cell');
    expect(row7Cells).toHaveLength(2);
    expect(row7Cells[0]).toHaveTextContent('Legacy release 2');
    expect(row7Cells[1]).toHaveTextContent('http://gov.uk/2');

    const row8Cells = within(rows[7]).getAllByRole('cell');
    expect(row8Cells).toHaveLength(2);
    expect(row8Cells[0]).toHaveTextContent('Legacy release 1');
    expect(row8Cells[1]).toHaveTextContent('http://gov.uk/1');
  });

  describe('deleting', () => {
    test('shows a warning modal when the delete release button is clicked', async () => {
      render(
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
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
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
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
        testCombinedReleases[4].id,
      );

      await waitFor(() => {
        expect(screen.queryByText('Confirm')).not.toBeInTheDocument();
      });

      const table = within(screen.getByRole('table'));
      await waitFor(() => {
        expect(table.queryByText('Legacy release 3')).not.toBeInTheDocument();
      });
      const rows = table.getAllByRole('row');
      expect(rows).toHaveLength(7);
    });
  });

  describe('creating', () => {
    test('shows a warning modal when the create legacy release button is clicked', async () => {
      const history = createMemoryHistory();
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <LegacyReleasesTable
              canManageLegacyReleases
              combinedReleases={testCombinedReleases}
              publicationId={testPublicationId}
            />
          </TestConfigContextProvider>
        </Router>,
      );
      userEvent.click(
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
      render(
        <Router history={history}>
          <TestConfigContextProvider>
            <LegacyReleasesTable
              canManageLegacyReleases
              combinedReleases={testCombinedReleases}
              publicationId={testPublicationId}
            />
          </TestConfigContextProvider>
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
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases={false}
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
      );

      expect(
        screen.queryByRole('button', { name: 'Create legacy release' }),
      ).not.toBeInTheDocument();
    });
  });

  describe('reordering', () => {
    test('shows a warning modal when the reorder releases button is clicked', async () => {
      render(
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
      );
      userEvent.click(screen.getByRole('button', { name: 'Reorder releases' }));
      await waitFor(() => {
        expect(screen.getByText('Warning')).toBeInTheDocument();
      });
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
      render(
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
      );
      userEvent.click(screen.getByRole('button', { name: 'Reorder releases' }));
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
        <TestConfigContextProvider>
          <LegacyReleasesTable
            canManageLegacyReleases={false}
            combinedReleases={testCombinedReleases}
            publicationId={testPublicationId}
          />
        </TestConfigContextProvider>,
      );

      expect(
        screen.queryByRole('button', { name: 'Reorder releases' }),
      ).not.toBeInTheDocument();
    });
  });
});
