import PublicationReleaseSeriesPage from '@admin/pages/publication/PublicationReleaseSeriesPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import _publicationService, {
  PublicationWithPermissions,
  ReleaseSeriesTableEntry,
} from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationReleaseSeriesPage', () => {
  const testReleaseSeries: ReleaseSeriesTableEntry[] = [
    {
      id: 'legacy-release-3',
      isLegacyLink: true,
      description: 'Legacy link 3',
      legacyLinkUrl: 'http://gov.uk/3',
    },
    {
      id: 'legacy-release-2',
      isLegacyLink: true,
      description: 'Legacy link 2',
      legacyLinkUrl: 'http://gov.uk/2',
    },
    {
      id: 'legacy-release-1',
      isLegacyLink: true,
      description: 'Legacy link 1',
      legacyLinkUrl: 'http://gov.uk/1',
    },
    {
      id: 'release-1',
      isLegacyLink: false,
      description: 'Academic year 2000/01',
      releaseId: 'release-id',
      releaseSlug: '2000-01',
      isLatest: true,
      isPublished: true,
    },
  ];

  test('renders the legacy releases page', async () => {
    publicationService.getReleaseSeries.mockResolvedValue(testReleaseSeries);

    renderPage(testPublication);
    await waitFor(() => {
      expect(screen.getByText('Legacy releases')).toBeInTheDocument();
    });

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(5);

    const row1Cells = within(rows[0]).getAllByRole('columnheader');
    expect(row1Cells[0]).toHaveTextContent('Description');
    expect(row1Cells[1]).toHaveTextContent('URL');
    expect(row1Cells[2]).toHaveTextContent('Actions');

    const row2Cells = within(rows[1]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Legacy link 3');
    expect(row2Cells[1]).toHaveTextContent('http://gov.uk/3');

    const row3Cells = within(rows[2]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Legacy link 2');
    expect(row3Cells[1]).toHaveTextContent('http://gov.uk/2');

    const row4Cells = within(rows[3]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Legacy link 1');
    expect(row4Cells[1]).toHaveTextContent('http://gov.uk/1');

    const row5Cells = within(rows[4]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('Academic year 2000/01');
    expect(row5Cells[1]).toHaveTextContent(
      'http://localhost/find-statistics/publication-1-slug/2000-01',
    );

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
