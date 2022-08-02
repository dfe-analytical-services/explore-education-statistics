import PublicationEditLegacyReleasePage from '@admin/pages/publication/PublicationEditLegacyReleasePage';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditLegacyReleaseRouteParams,
  publicationEditLegacyReleaseRoute,
} from '@admin/routes/publicationRoutes';
import _legacyReleaseService from '@admin/services/legacyReleaseService';
import {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { generatePath, match } from 'react-router';
import { createMemoryHistory, createLocation } from 'history';
import noop from 'lodash/noop';

jest.mock('@admin/services/legacyReleaseService');
const legacyReleaseService = _legacyReleaseService as jest.Mocked<
  typeof _legacyReleaseService
>;

describe('PublicationEditLegacyReleasePage', () => {
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

  test('renders the edit legacy release page', async () => {
    legacyReleaseService.getLegacyRelease.mockResolvedValue({
      description: 'Legacy release 3',
      id: 'legacy-release-3',
      order: 3,
      publicationId: 'publication-id-1',
      url: 'http://gov.uk/3',
    });
    renderPage(testPublication);

    await waitFor(() => {
      expect(screen.getByText('Edit legacy release')).toBeInTheDocument();
    });
    expect(screen.getByLabelText('Description')).toHaveValue(
      'Legacy release 3',
    );
    expect(screen.getByLabelText('URL')).toHaveValue('http://gov.uk/3');
    expect(screen.getByLabelText('Order')).toHaveValue(3);
    expect(
      screen.getByRole('button', { name: 'Save legacy release' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Cancel' })).toBeInTheDocument();
  });
});

function renderPage(publication: MyPublication) {
  const history = createMemoryHistory();
  const path = generatePath<PublicationEditLegacyReleaseRouteParams>(
    publicationEditLegacyReleaseRoute.path,
    {
      publicationId: publication.id,
      legacyReleaseId: 'legacy-3',
    },
  );

  const mockMatch: match<PublicationEditLegacyReleaseRouteParams> = {
    isExact: false,
    path,
    url: path,
    params: {
      publicationId: publication.id,
      legacyReleaseId: 'legacy-3',
    },
  };

  const routeComponentPropsMock = {
    history,
    location: createLocation(path),
    match: mockMatch,
  };

  render(
    <MemoryRouter>
      <PublicationContextProvider
        publication={publication}
        onPublicationChange={noop}
        onReload={noop}
      >
        <PublicationEditLegacyReleasePage {...routeComponentPropsMock} />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
