import PublicationCreateLegacyReleasePage from '@admin/pages/publication/PublicationCreateLegacyReleasePage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import noop from 'lodash/noop';

describe('PublicationCreateLegacyReleasePage', () => {
  const testContact: PublicationContactDetails = {
    contactName: 'John Smith',
    contactTelNo: '0777777777',
    id: 'contact-id-1',
    teamEmail: 'john.smith@test.com',
    teamName: 'Team Smith',
  };

  const testPublication: MyPublication = {
    id: 'publication-1',
    title: 'Publication 1',
    contact: testContact,
    releases: [],
    legacyReleases: [
      {
        description: 'Legacy release 3',
        id: 'legacy-release-3',
        order: 3,
        publicationId: 'publication-id-1',
        url: 'http://gov.uk/3',
      },
      {
        description: 'Legacy release 2',
        id: 'legacy-release-2',
        order: 2,
        publicationId: 'publication-id-1',
        url: 'http://gov.uk/2',
      },
      {
        description: 'Legacy release 1',
        id: 'legacy-release-1',
        order: 1,
        publicationId: 'publication-id-1',
        url: 'http://gov.uk/1',
      },
    ],
    methodologies: [],
    themeId: 'theme-1',
    topicId: 'theme-1-topic-2',
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

  test('renders the create legacy release page', async () => {
    renderPage(testPublication);

    expect(screen.getByText('Create legacy release')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(screen.getByLabelText('URL')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toBeInTheDocument();
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
        <PublicationCreateLegacyReleasePage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
