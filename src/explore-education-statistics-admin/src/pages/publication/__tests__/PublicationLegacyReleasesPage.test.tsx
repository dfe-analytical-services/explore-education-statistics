import PublicationLegacyReleasesPage from '@admin/pages/publication/PublicationLegacyReleasesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _legacyReleaseService, {
  CombinedRelease,
} from '@admin/services/legacyReleaseService';
import { PublicationWithPermissions } from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/legacyReleaseService');

const legacyReleaseService = _legacyReleaseService as jest.Mocked<
  typeof _legacyReleaseService
>;

describe('PublicationLegacyReleasesPage', () => {
  const testCombinedReleases: CombinedRelease[] = [
    {
      description: 'Legacy release 3',
      id: 'legacy-release-3',
      order: 3,
      url: 'http://gov.uk/3',
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'Legacy release 2',
      id: 'legacy-release-2',
      order: 2,
      url: 'http://gov.uk/2',
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
    {
      description: 'Legacy release 1',
      id: 'legacy-release-1',
      order: 1,
      url: 'http://gov.uk/1',
      isDraft: false,
      isLegacy: true,
      isAmendment: false,
      isLatest: false,
    },
  ];

  test('renders the legacy releases page', async () => {
    legacyReleaseService.listCombinedReleases.mockResolvedValue(
      testCombinedReleases,
    );

    renderPage(testPublication);
    await waitFor(() => {
      expect(screen.getByText('Legacy releases')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells[0]).toHaveTextContent('Description');
    expect(row1Cells[1]).toHaveTextContent('URL');
    expect(row1Cells[2]).toHaveTextContent('Actions');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Legacy release 3');
    expect(row2Cells[1]).toHaveTextContent('http://gov.uk/3');

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Legacy release 2');
    expect(row3Cells[1]).toHaveTextContent('http://gov.uk/2');

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Legacy release 1');
    expect(row4Cells[1]).toHaveTextContent('http://gov.uk/1');

    expect(
      screen.getByRole('button', { name: 'Create legacy release' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder releases' }),
    ).toBeInTheDocument();
  });
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationLegacyReleasesPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
