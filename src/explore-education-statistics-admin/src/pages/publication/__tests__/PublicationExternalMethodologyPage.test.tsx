import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import _publicationService, {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter, Router } from 'react-router-dom';
import { createMemoryHistory } from 'history';
import noop from 'lodash/noop';

jest.mock('@admin/services/publicationService');

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationExternalMethodologyPage', () => {
  const testContact: PublicationContactDetails = {
    id: 'contact-1',
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: testContact,
    releases: [],
    legacyReleases: [],
    methodologies: [],
    themeId: 'theme-1',
    topicId: 'topic-1',
    permissions: {
      canAdoptMethodologies: true,
      canCreateReleases: true,
      canUpdatePublication: true,
      canUpdatePublicationTitle: true,
      canUpdatePublicationSupersededBy: true,
      canCreateMethodologies: true,
      canManageExternalMethodology: true,
    },
  };

  const testExternalMethodology: ExternalMethodology = {
    title: 'External methodolology title',
    url: 'http://test.com',
  };

  test('renders the add an external methodology page correctly', () => {
    renderPage(testPublication);

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

  test('renders the edit external methodology page correctly', () => {
    renderPage({
      ...testPublication,
      externalMethodology: testExternalMethodology,
    });

    expect(
      screen.getByRole('heading', {
        name: 'Edit external methodology link',
      }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Link title')).toHaveValue(
      'External methodolology title',
    );

    expect(screen.getByLabelText('URL')).toHaveValue('http://test.com');

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();

    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('handles successful form submission', async () => {
    const history = createMemoryHistory();

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

    userEvent.type(screen.getByLabelText('Link title'), 'The link title');

    userEvent.type(screen.getByLabelText('URL'), 'test.com');

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(publicationService.updatePublication).toHaveBeenCalledWith(
        'publication-1',
        {
          ...testPublication,
          externalMethodology: {
            title: 'The link title',
            url: 'https://test.com',
          },
        },
      );
    });

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
    );
  });

  test('handles clicking the cancel button', async () => {
    const history = createMemoryHistory();

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

    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(publicationService.updatePublication).not.toHaveBeenCalled();

    expect(history.location.pathname).toBe(
      `/publication/publication-1/methodologies`,
    );
  });
});

function renderPage(publication: MyPublication) {
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
