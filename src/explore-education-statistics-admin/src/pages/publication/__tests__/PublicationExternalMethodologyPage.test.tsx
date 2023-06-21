import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import _publicationService, {
  ExternalMethodology,
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Router } from 'react-router-dom';
import { createMemoryHistory } from 'history';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationExternalMethodologyPage', () => {
  const testExternalMethodology: ExternalMethodology = {
    title: 'External methodology title',
    url: 'http://test.com',
  };

  test('renders the add an external methodology page correctly', async () => {
    publicationService.getExternalMethodology.mockResolvedValue(undefined);

    renderPage(testPublication);

    await waitFor(() => {
      expect(publicationService.getExternalMethodology).toBeCalledWith(
        testPublication.id,
      );
    });

    expect(
      screen.getByRole('heading', {
        name: 'Link to an externally hosted methodology',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('Link title')).toBeInTheDocument();

    expect(screen.getByLabelText('URL')).toBeInTheDocument();

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders the edit external methodology page correctly', async () => {
    publicationService.getExternalMethodology.mockResolvedValue(
      testExternalMethodology,
    );

    renderPage(testPublication);

    await waitFor(() => {
      expect(publicationService.getExternalMethodology).toBeCalledWith(
        testPublication.id,
      );
    });

    expect(
      screen.getByRole('heading', {
        name: 'Edit external methodology link',
      }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Link title')).toHaveValue(
      'External methodology title',
    );

    expect(screen.getByLabelText('URL')).toHaveValue('http://test.com');

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();

    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('handles successful form submission', async () => {
    const history = createMemoryHistory();

    publicationService.getExternalMethodology.mockResolvedValue(undefined);

    render(
      <Router history={history}>
        <PublicationContextProvider
          publication={testPublication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationExternalMethodologyPage />
        </PublicationContextProvider>
      </Router>,
    );

    await waitFor(() => {
      expect(publicationService.getExternalMethodology).toBeCalledWith(
        testPublication.id,
      );
    });

    userEvent.type(screen.getByLabelText('Link title'), 'The link title');

    userEvent.type(screen.getByLabelText('URL'), 'test.com');

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(publicationService.updateExternalMethodology).toHaveBeenCalledWith(
        testPublication.id,
        {
          title: 'The link title',
          url: 'https://test.com',
        },
      );
    });

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
    );
  });

  test('handles clicking the cancel button', async () => {
    const history = createMemoryHistory();

    publicationService.getExternalMethodology.mockResolvedValue(
      testExternalMethodology,
    );

    render(
      <Router history={history}>
        <PublicationContextProvider
          publication={testPublication}
          onPublicationChange={noop}
          onReload={noop}
        >
          <PublicationExternalMethodologyPage />
        </PublicationContextProvider>
      </Router>,
    );

    await waitFor(() => {
      expect(publicationService.getExternalMethodology).toBeCalledWith(
        testPublication.id,
      );
    });

    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(publicationService.updatePublication).not.toHaveBeenCalled();

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
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
        <PublicationExternalMethodologyPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
