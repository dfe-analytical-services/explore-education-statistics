import PublicationExternalMethodologyPage from '@admin/pages/publication/PublicationExternalMethodologyPage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  ExternalMethodology,
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

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
