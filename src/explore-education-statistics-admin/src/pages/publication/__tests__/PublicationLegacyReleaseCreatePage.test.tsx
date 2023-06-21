import PublicationLegacyReleaseCreatePage from '@admin/pages/publication/PublicationLegacyReleaseCreatePage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _legacyReleaseService from '@admin/services/legacyReleaseService';
import { PublicationWithPermissions } from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

jest.mock('@admin/services/legacyReleaseService');
const legacyReleaseService = _legacyReleaseService as jest.Mocked<
  typeof _legacyReleaseService
>;

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

  test('handles successfully submitting the form', async () => {
    renderPage(testPublication);

    userEvent.type(screen.getByLabelText('Description'), 'Test description');
    userEvent.type(screen.getByLabelText('URL'), 'http://test.com');
    userEvent.click(
      screen.getByRole('button', {
        name: 'Save legacy release',
      }),
    );

    await waitFor(() => {
      expect(legacyReleaseService.createLegacyRelease).toHaveBeenCalledWith({
        description: 'Test description',
        url: 'http://test.com',
        publicationId: 'publication-1',
      });
    });
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
