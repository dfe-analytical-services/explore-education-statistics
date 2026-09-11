import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import { releaseDataFileReplacementCompleteRoute } from '@admin/routes/releaseRoutes';
import _releaseDataFileService, {
  DataSetAccoutrements,
} from '@admin/services/releaseDataFileService';
import { act, render, screen } from '@testing-library/react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { generatePath } from 'react-router-dom';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';

jest.mock('@admin/services/releaseDataFileService');

const releaseDataFileService = _releaseDataFileService as jest.Mocked<
  typeof _releaseDataFileService
>;

describe('ReleaseDataFileReplacementCompletePage', () => {
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
      await screen.findByRole('heading', { name: 'Data replacement complete' }),
    ).toBeInTheDocument();
  });

  test('renders error message if there is an error loading replacement plan', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockRejectedValue(
      new Error('Something went wrong'),
    );

    await renderPage();

    expect(
      await screen.findByText('There was a problem with the data replacement.'),
    ).toBeInTheDocument();
  });

  async function renderPage(history: MemoryHistory = createMemoryHistory()) {
    history.push(
      generatePath(releaseDataFileReplacementCompleteRoute.path, {
        publicationId: 'publication-1',
        releaseVersionId: 'release-1',
        fileId: 'file-1',
      }),
    );

    await act(async () =>
      render(
        <TestRouterRenderer
          initialUrl={generatePath(
            releaseDataFileReplacementCompleteRoute.fullPath,
            {
              publicationId: 'publication-1',
              releaseVersionId: 'release-1',
              fileId: 'file-1',
            },
          )}
          route={releaseDataFileReplacementCompleteRoute.fullPath}
        >
          <ReleaseDataFileReplacementCompletePage />
        </TestRouterRenderer>,
      ),
    );
  }
});
