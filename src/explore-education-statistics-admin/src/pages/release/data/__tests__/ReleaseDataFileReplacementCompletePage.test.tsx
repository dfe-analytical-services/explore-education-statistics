import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import {
  releaseDataFileReplacementCompleteRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import _releaseDataFileService, {
  DataSetAccoutrements,
} from '@admin/services/releaseDataFileService';
import { render, screen, waitFor } from '@testing-library/react';
import { createMemoryHistory, MemoryHistory } from 'history';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ReleaseDataFilePage', () => {
  const testAccoutrements: DataSetAccoutrements = {
    dataBlocks: [],
    footnotes: [],
  };

  test('renders the page when successfully loads the replacement plan', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockResolvedValue(
      testAccoutrements,
    );

    await renderPage();

    expect(
      screen.getByRole('heading', { name: 'Data replacement complete' }),
    ).toBeInTheDocument();
  });

  test('renders error message if there is an error loading replacement plan', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockRejectedValue(
      new Error('Something went wrong'),
    );

    await renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('There was a problem with the data replacement.'),
      ).toBeInTheDocument();
    });
  });

  async function renderPage(history: MemoryHistory = createMemoryHistory()) {
    history.push(
      generatePath<ReleaseDataFileRouteParams>(
        releaseDataFileReplacementCompleteRoute.path,
        {
          publicationId: 'publication-1',
          releaseVersionId: 'release-1',
          fileId: 'file-1',
        },
      ),
    );

    render(
      <Router history={history}>
        <Route
          path={releaseDataFileReplacementCompleteRoute.path}
          component={ReleaseDataFileReplacementCompletePage}
        />
      </Router>,
    );
  }
});
