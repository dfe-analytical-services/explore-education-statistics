import PublicationPageContainer from '@admin/pages/publication/PublicationPageContainer';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import { publicationRoute } from '@admin/routes/routes';
import _publicationService, {
  MyPublication,
  PublicationContactDetails,
} from '@admin/services/publicationService';
import { generatePath, MemoryRouter } from 'react-router';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { Route } from 'react-router-dom';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationPageContainer', () => {
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
    legacyReleases: [],
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

  test('renders the page with the releases tab', async () => {
    publicationService.getMyPublication.mockResolvedValue(testPublication);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Publication 1')).toBeInTheDocument();
    });

    expect(screen.getByTestId('page-title-caption')).toHaveTextContent(
      'Manage publication',
    );

    const navLinks = within(
      screen.getByRole('navigation', {
        name: 'Publication',
      }),
    ).getAllByRole('link');
    expect(navLinks).toHaveLength(2);
    expect(navLinks[0]).toHaveTextContent('Releases');
    expect(navLinks[0].getAttribute('aria-current')).toBe('page');
    expect(navLinks[1]).toHaveTextContent('Details');
    expect(navLinks[1]).not.toHaveAttribute('aria-current');

    expect(screen.getByText('Manage releases')).toBeInTheDocument();
  });

  test('shows a message when the publication is archived', async () => {
    publicationService.getMyPublication.mockResolvedValue({
      ...testPublication,
      isSuperseded: true,
      supersededById: 'publication-2',
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Publication 1')).toBeInTheDocument();
    });

    expect(
      screen.getByText('This publication is archived.'),
    ).toBeInTheDocument();
  });

  test('shows a message when the publication is superseded but not yet archived', async () => {
    publicationService.getMyPublication.mockResolvedValue({
      ...testPublication,
      isSuperseded: false,
      supersededById: 'publication-2',
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Publication 1')).toBeInTheDocument();
    });

    expect(
      screen.getByText(
        'This publication will be archived when its superseding publication has a live release published.',
      ),
    ).toBeInTheDocument();
  });
});

function renderPage() {
  const path = generatePath<PublicationRouteParams>(
    publicationReleasesRoute.path,
    {
      publicationId: 'publication-1',
    },
  );

  render(
    <MemoryRouter initialEntries={[path]}>
      <Route
        component={PublicationPageContainer}
        path={publicationRoute.path}
      />
    </MemoryRouter>,
  );
}
