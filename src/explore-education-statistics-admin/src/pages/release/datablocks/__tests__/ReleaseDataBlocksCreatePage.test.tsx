import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import _permissionService from '@admin/services/permissionService';
import ReleaseDataBlockCreatePage from '@admin/pages/release/datablocks/ReleaseDataBlockCreatePage';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter } from 'react-router';
import { Route } from 'react-router-dom';

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
      <TestConfigContextProvider>
        <MemoryRouter
          initialEntries={[
            generatePath<ReleaseDataBlockRouteParams>(
              releaseDataBlockEditRoute.path,
              {
                publicationId: 'publication-1',
                releaseId: 'release-1',
                dataBlockId: 'block-1',
              },
            ),
          ]}
        >
          <Route
            path={releaseDataBlockEditRoute.path}
            component={ReleaseDataBlockCreatePage}
          />
        </MemoryRouter>
      </TestConfigContextProvider>,
    );
  };
});
