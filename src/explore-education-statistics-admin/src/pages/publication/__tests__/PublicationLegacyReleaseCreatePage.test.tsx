import PublicationLegacyReleaseCreatePage from '@admin/pages/publication/PublicationLegacyReleaseCreatePage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { PublicationWithPermissions } from '@admin/services/publicationService';
import { render, screen, } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

// jest.mock('@admin/services/legacyReleaseService'); // @MarkFix
// const legacyReleaseService = _legacyReleaseService as jest.Mocked<
//  typeof _legacyReleaseService
// >;

describe('PublicationLegacyReleaseCreatePage', () => {
  test('renders the create legacy release page', async () => {
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
});

function renderPage(publication: PublicationWithPermissions) {
  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationLegacyReleaseCreatePage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
