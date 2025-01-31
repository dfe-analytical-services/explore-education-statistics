import PublicationCreateReleaseSeriesLegacyLinkPage from '@admin/pages/publication/PublicationCreateReleaseSeriesLegacyLinkPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationCreateReleaseSeriesLegacyLinkPage', () => {
  test('renders the create release series page', async () => {
    renderPage(testPublication);

    await waitFor(() => {
      expect(screen.getByText('Create legacy release')).toBeInTheDocument();
    });

    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(screen.getByLabelText('URL')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/publication/publication-1/releases/order',
    );
  });

  test('handles successfully submitting the form', async () => {
    renderPage(testPublication);

    await waitFor(() => {
      expect(screen.getByText('Create legacy release')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Description'), 'legacy link 1');
    await userEvent.type(
      screen.getByLabelText('URL'),
      'https://www.test.com/1',
    );
    await userEvent.click(
      screen.getByRole('button', { name: 'Save legacy release' }),
    );

    await waitFor(() => {
      expect(
        publicationService.addReleaseSeriesLegacyLink,
      ).toHaveBeenCalledWith(testPublication.id, {
        description: 'legacy link 1',
        url: 'https://www.test.com/1',
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
        <PublicationCreateReleaseSeriesLegacyLinkPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
