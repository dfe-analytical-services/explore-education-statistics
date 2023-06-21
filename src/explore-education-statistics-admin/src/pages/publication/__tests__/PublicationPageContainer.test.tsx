import render from '@common-test/render';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import PublicationPageContainer from '@admin/pages/publication/PublicationPageContainer';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import { publicationRoute } from '@admin/routes/routes';
import _publicationService from '@admin/services/publicationService';
import { ReleaseSummaryWithPermissions } from '@admin/services/releaseService';
import { PaginatedList } from '@common/services/types/pagination';
import { screen, waitFor, within } from '@testing-library/react';
import { generatePath, MemoryRouter } from 'react-router';
import { Route } from 'react-router-dom';
import React from 'react';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationPageContainer', () => {
  const testEmptyReleases: PaginatedList<ReleaseSummaryWithPermissions> = {
    paging: {
      page: 1,
      pageSize: 5,
      totalPages: 1,
      totalResults: 0,
    },
    results: [],
  };

  test('renders the page with the releases tab', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);
    publicationService.listReleases.mockResolvedValue(testEmptyReleases);

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
    expect(navLinks).toHaveLength(6);
    expect(navLinks[0]).toHaveTextContent('Releases');
    expect(navLinks[0].getAttribute('aria-current')).toBe('page');
    expect(navLinks[1]).toHaveTextContent('Methodologies');
    expect(navLinks[1]).not.toHaveAttribute('aria-current');
    expect(navLinks[2]).toHaveTextContent('Details');
    expect(navLinks[2]).not.toHaveAttribute('aria-current');
    expect(navLinks[3]).toHaveTextContent('Contact');
    expect(navLinks[3]).not.toHaveAttribute('aria-current');
    expect(navLinks[4]).toHaveTextContent('Team access');
    expect(navLinks[4]).not.toHaveAttribute('aria-current');
    expect(navLinks[5]).toHaveTextContent('Legacy releases');
    expect(navLinks[5]).not.toHaveAttribute('aria-current');

    expect(screen.getByText('Manage releases')).toBeInTheDocument();
  });

  test('shows a message when the publication is archived', async () => {
    publicationService.getPublication.mockResolvedValue({
      ...testPublication,
      isSuperseded: true,
      supersededById: 'publication-2',
    });
    publicationService.listReleases.mockResolvedValue(testEmptyReleases);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Publication 1')).toBeInTheDocument();
    });

    expect(
      screen.getByText('This publication is archived.'),
    ).toBeInTheDocument();
  });

  test('shows a message when the publication is superseded but not yet archived', async () => {
    publicationService.getPublication.mockResolvedValue({
      ...testPublication,
      isSuperseded: false,
      supersededById: 'publication-2',
    });
    publicationService.listReleases.mockResolvedValue(testEmptyReleases);

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
