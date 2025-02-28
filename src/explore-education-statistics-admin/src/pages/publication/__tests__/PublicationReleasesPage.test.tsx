import render from '@common-test/render';
import { testPublication } from '@admin/pages/publication/__data__/testPublication';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import PublicationReleasesPage from '@admin/pages/publication/PublicationReleasesPage';
import _publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import _releaseVersionService from '@admin/services/releaseVersionService';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';

jest.mock('@admin/services/releaseVersionService');
const releaseVersionService = _releaseVersionService as jest.Mocked<
  typeof _releaseVersionService
>;

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

describe('PublicationReleasesPage', () => {
  beforeEach(() => {
    releaseVersionService.getReleaseVersionStatus.mockResolvedValue({
      overallStage: 'Complete',
    });
    releaseVersionService.getReleaseVersionChecklist.mockResolvedValue({
      errors: [],
      valid: true,
      warnings: [],
    });
  });

  test('shows the create release button if you have permission', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);

    renderPage(testPublication);

    await waitFor(() => {
      expect(screen.getByText('Manage releases')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Create new release' }),
    ).toBeInTheDocument();
  });

  test('does not show the create release button if you do not have permission', async () => {
    const publication: PublicationWithPermissions = {
      ...testPublication,
      permissions: {
        ...testPublication.permissions,
        canCreateReleases: false,
      },
    };
    publicationService.getPublication.mockResolvedValue(publication);

    renderPage(publication);

    await waitFor(() => {
      expect(screen.getByText('Manage releases')).toBeInTheDocument();
    });

    expect(
      screen.queryByRole('link', { name: 'Create new release' }),
    ).not.toBeInTheDocument();
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
        <PublicationReleasesPage />
      </PublicationContextProvider>
    </MemoryRouter>,
  );
}
