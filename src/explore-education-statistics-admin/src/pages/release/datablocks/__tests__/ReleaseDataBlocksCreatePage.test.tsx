import { releaseDataBlockEditRoute } from '@admin/routes/releaseRoutes';
import _permissionService from '@admin/services/permissionService';
import ReleaseDataBlockCreatePage from '@admin/pages/release/datablocks/ReleaseDataBlockCreatePage';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { generatePath } from 'react-router';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';

jest.mock('@admin/services/permissionService');

const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('ReleaseDataBlockCreatePage', () => {
  test('renders correctly if release cannot be updated', async () => {
    permissionService.canUpdateRelease.mockResolvedValue(false);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText(
          /This release has been approved, and can no longer be updated/,
        ),
      ).toBeInTheDocument();
    });

    expect(screen.queryByRole('tablist')).not.toBeInTheDocument();
  });

  const renderPage = () => {
    return render(
      <TestRouterRenderer
        initialUrl={generatePath(releaseDataBlockEditRoute.fullPath, {
          publicationId: 'publication-1',
          releaseVersionId: 'release-1',
          dataBlockVersionId: 'block-1',
        })}
        route={releaseDataBlockEditRoute.fullPath}
      >
        <ReleaseDataBlockCreatePage />
      </TestRouterRenderer>,
    );
  };
});
