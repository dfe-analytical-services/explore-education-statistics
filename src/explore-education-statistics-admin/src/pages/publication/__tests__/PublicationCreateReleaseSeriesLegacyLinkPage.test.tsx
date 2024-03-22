import PublicationCreateReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationCreateReleaseSeriesLegacyLinkPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationCreateReleaseSeriesLegacyLinkPage', () => {
  test('renders the create release series page', async () => {
    renderPage(testPublication);

    expect(screen.getByText('Create legacy release')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(screen.getByLabelText('URL')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/publication/publication-1/legacy',
    );
  });
  // @MarkFix another test to actually create a new series item
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationCreateReleaseSeriesLegacyLinkPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
