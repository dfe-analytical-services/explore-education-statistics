import PreReleaseMethodologiesPage from '@admin/pages/release/pre-release/PreReleaseMethodologiesPage';
import { preReleaseMethodologiesRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import _publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PreReleaseMethodologiesPage', () => {
  const testPublicationNoMethodologies: MyPublication = {
    id: 'pub-1-id',
    title: 'Publication 1',
    releases: [],
    methodologies: [],
    legacyReleases: [],
    topicId: 'topic-id',
    themeId: 'theme-id',
    permissions: {
      canAdoptMethodologies: false,
      canCreateMethodologies: false,
      canCreateReleases: false,
      canManageExternalMethodology: false,
      canUpdatePublication: false,
      canUpdatePublicationTitle: false,
    },
  };

  const methodologyPermissions = {
    canApproveMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
    canUpdateMethodology: false,
  };

  test('renders correctly with no methodologies', async () => {
    publicationService.getMyPublication.mockResolvedValue(
      testPublicationNoMethodologies,
    );
    renderPage();

    expect(screen.getByRole('heading', { name: 'Methodologies' }));

    await waitFor(() => {
      expect(screen.getByText('No Methodologies available.'));
    });
  });

  test('renders correctly with published owned and adopted methodologies and external methodologies', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      externalMethodology: {
        title: 'An external methodology',
        url: 'http://hiveit.co.uk',
      },
      methodologies: [
        {
          methodology: {
            id: 'methodology-1-id',
            methodologyId: 'methodologyId-1',
            title: 'Methodology 1',
            slug: 'methodology-1',
            owningPublication: {
              id: 'owning-publication-1-id',
              title: 'Owning publication 1',
            },
            permissions: methodologyPermissions,
            amendment: false,
            published: '2018-03-22T00:00:00',
            status: 'Approved',
          },
          owner: true,
          permissions: {
            canDropMethodology: false,
          },
        },
        {
          methodology: {
            id: 'methodology-2-id',
            methodologyId: 'methodologyId-2',
            title: 'Methodology 2',
            slug: 'methodology-2',
            owningPublication: {
              id: 'owning-publication-2-id',
              title: 'Owning publication 2',
            },
            permissions: methodologyPermissions,
            amendment: false,
            published: '2018-03-22T00:00:00',
            status: 'Approved',
          },
          owner: false,
          permissions: {
            canDropMethodology: false,
          },
        },
      ],
    };
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology 1 (Owned)'));
    });

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(3);

    expect(
      within(items[0]).getByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-1-id',
    );
    expect(within(items[0]).getByText('Published'));

    expect(
      within(items[1]).getByRole('link', { name: 'Methodology 2 (Adopted)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-2-id',
    );
    expect(within(items[1]).getByText('Published'));

    expect(
      within(items[2]).getByRole('link', {
        name: 'An external methodology (External)',
      }),
    ).toHaveAttribute('href', 'http://hiveit.co.uk');
  });

  test('renders approved and scheduled methodologies correctly', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      methodologies: [
        {
          methodology: {
            id: 'methodology-1-id',
            methodologyId: 'methodologyId-1',
            title: 'Methodology 1',
            slug: 'methodology-1',
            owningPublication: {
              id: 'owning-publication-1-id',
              title: 'Owning publication 1',
            },
            permissions: methodologyPermissions,
            amendment: false,
            status: 'Approved',
          },
          owner: true,
          permissions: {
            canDropMethodology: false,
          },
        },
      ],
    };
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology 1 (Owned)'));
    });

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(1);

    expect(
      within(items[0]).getByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-1-id',
    );
    expect(within(items[0]).getByText('Approved'));
  });

  test('does not show unapproved draft methodologies', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      methodologies: [
        {
          methodology: {
            id: 'methodology-1-id',
            methodologyId: 'methodologyId-1',
            title: 'Methodology 1',
            slug: 'methodology-1',
            owningPublication: {
              id: 'owning-publication-1-id',
              title: 'Owning publication 1',
            },
            permissions: methodologyPermissions,
            amendment: false,
            status: 'Draft',
          },
          owner: true,
          permissions: {
            canDropMethodology: false,
          },
        },
      ],
    };
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('No Methodologies available.'));
    });

    expect(
      screen.queryByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly and links to the previous approved version for draft amendments', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      methodologies: [
        {
          methodology: {
            id: 'methodology-1-id',
            methodologyId: 'methodologyId-1',
            title: 'Methodology 1',
            slug: 'methodology-1',
            owningPublication: {
              id: 'owning-publication-1-id',
              title: 'Owning publication 1',
            },
            permissions: methodologyPermissions,
            previousVersionId: 'methodology-1-previous-id',
            amendment: true,
            status: 'Draft',
          },
          owner: true,
          permissions: {
            canDropMethodology: false,
          },
        },
      ],
    };
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology 1 (Owned)'));
    });

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(1);

    expect(
      within(items[0]).getByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-1-previous-id',
    );

    expect(within(items[0]).getByText('Published'));
  });

  test('renders the correctly for approved amendments', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      methodologies: [
        {
          methodology: {
            id: 'methodology-1-id',
            methodologyId: 'methodologyId-1',
            title: 'Methodology 1',
            slug: 'methodology-1',
            owningPublication: {
              id: 'owning-publication-1-id',
              title: 'Owning publication 1',
            },
            permissions: methodologyPermissions,
            previousVersionId: 'methodology-1-previous-id',
            amendment: true,
            status: 'Approved',
          },
          owner: true,
          permissions: {
            canDropMethodology: false,
          },
        },
        {
          methodology: {
            id: 'methodology-2-id',
            methodologyId: 'methodologyId-2',
            title: 'Methodology 2',
            slug: 'methodology-2',
            owningPublication: {
              id: 'owning-publication-2-id',
              title: 'Owning publication 2',
            },
            permissions: methodologyPermissions,
            previousVersionId: 'methodology-2-previous-id',
            published: '2018-03-22T00:00:00',
            amendment: true,
            status: 'Approved',
          },
          owner: false,
          permissions: {
            canDropMethodology: false,
          },
        },
      ],
    };
    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Methodology 1 (Owned)'));
    });

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(2);

    expect(
      within(items[0]).getByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-1-id',
    );
    expect(within(items[0]).getByText('Approved'));
    expect(within(items[0]).getByText('Amendment'));

    expect(
      within(items[1]).getByRole('link', { name: 'Methodology 2 (Adopted)' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/methodologies/methodology-2-id',
    );
    expect(within(items[1]).getByText('Published'));
    expect(within(items[1]).getByText('Amendment'));
  });

  test('renders when there is only a external methodology', async () => {
    const testPublication: MyPublication = {
      ...testPublicationNoMethodologies,
      externalMethodology: {
        title: 'An external methodology',
        url: 'http://hiveit.co.uk',
      },
    };

    publicationService.getMyPublication.mockResolvedValue(testPublication);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('An external methodology (External)'));
    });

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(1);

    expect(
      within(items[0]).getByRole('link', {
        name: 'An external methodology (External)',
      }),
    ).toHaveAttribute('href', 'http://hiveit.co.uk');
  });

  const renderPage = (
    initialEntries: string[] = [
      generatePath<ReleaseRouteParams>(preReleaseMethodologiesRoute.path, {
        publicationId: 'publication-1',
        releaseId: 'release-1',
      }),
    ],
  ) => {
    return render(
      <MemoryRouter initialEntries={initialEntries}>
        <Route
          component={PreReleaseMethodologiesPage}
          path={preReleaseMethodologiesRoute.path}
        />
      </MemoryRouter>,
    );
  };
});
