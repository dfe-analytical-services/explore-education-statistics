import PreReleaseMethodologiesPage from '@admin/pages/release/pre-release/PreReleaseMethodologiesPage';
import { preReleaseMethodologiesRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import _methodologyService from '@admin/services/methodologyService';
import _publicationService, {
  ExternalMethodology,
} from '@admin/services/publicationService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';
import React from 'react';

jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/publicationService');

const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PreReleaseMethodologiesPage', () => {
  const methodologyPermissions = {
    canApproveMethodology: false,
    canDeleteMethodology: false,
    canMakeAmendmentOfMethodology: false,
    canMarkMethodologyAsDraft: false,
    canUpdateMethodology: false,
    canRemoveMethodologyLink: false,
  };

  const testExternalMethodology: ExternalMethodology = {
    title: 'An external methodology',
    url: 'http://hiveit.co.uk',
  };

  test('renders correctly with no methodologies', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
    renderPage();

    expect(screen.getByRole('heading', { name: 'Methodologies' }));

    await waitFor(() => {
      expect(screen.getByText('No methodologies available.'));
    });
  });

  test('renders correctly with published owned and adopted methodologies and external methodologies', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([
      {
        id: 'methodology-1-id',
        methodologyId: 'methodologyId-1',
        title: 'Methodology 1',
        permissions: methodologyPermissions,
        amendment: false,
        owned: true,
        published: '2018-03-22T00:00:00',
        status: 'Approved',
      },
      {
        id: 'methodology-2-id',
        methodologyId: 'methodologyId-2',
        title: 'Methodology 2',
        permissions: methodologyPermissions,
        amendment: false,
        owned: false,
        published: '2018-03-22T00:00:00',
        status: 'Approved',
      },
    ]);
    publicationService.getExternalMethodology.mockResolvedValue({
      title: 'An external methodology',
      url: 'http://hiveit.co.uk',
    });
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
    methodologyService.listMethodologyVersions.mockResolvedValue([
      {
        id: 'methodology-1-id',
        methodologyId: 'methodologyId-1',
        title: 'Methodology 1',
        permissions: methodologyPermissions,
        amendment: false,
        owned: true,
        status: 'Approved',
      },
    ]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
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
    methodologyService.listMethodologyVersions.mockResolvedValue([
      {
        id: 'methodology-1-id',
        methodologyId: 'methodologyId-1',
        title: 'Methodology 1',
        permissions: methodologyPermissions,
        amendment: false,
        owned: true,
        published: '2018-03-22T00:00:00',
        status: 'Draft',
      },
    ]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('No methodologies available.'));
    });

    expect(
      screen.queryByRole('link', { name: 'Methodology 1 (Owned)' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly and links to the previous approved version for draft amendments', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([
      {
        id: 'methodology-1-id',
        methodologyId: 'methodologyId-1',
        title: 'Methodology 1',
        permissions: methodologyPermissions,
        amendment: true,
        owned: true,
        published: '2018-03-22T00:00:00',
        status: 'Draft',
        previousVersionId: 'methodology-1-previous-id',
      },
    ]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
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

  test('renders correctly for approved amendments', async () => {
    methodologyService.listMethodologyVersions.mockResolvedValue([
      {
        id: 'methodology-1-id',
        methodologyId: 'methodologyId-1',
        title: 'Methodology 1',
        permissions: methodologyPermissions,
        amendment: true,
        owned: true,
        status: 'Approved',
        previousVersionId: 'methodology-1-previous-id',
      },
      {
        id: 'methodology-2-id',
        methodologyId: 'methodologyId-2',
        title: 'Methodology 2',
        permissions: methodologyPermissions,
        amendment: true,
        owned: false,
        published: '2018-03-22T00:00:00',
        status: 'Approved',
        previousVersionId: 'methodology-2-previous-id',
      },
    ]);
    publicationService.getExternalMethodology.mockResolvedValue(undefined);
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
    methodologyService.listMethodologyVersions.mockResolvedValue([]);
    publicationService.getExternalMethodology.mockResolvedValue(
      testExternalMethodology,
    );
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
