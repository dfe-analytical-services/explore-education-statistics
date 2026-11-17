import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import { releaseDataFileReplacementCompleteRoute } from '@admin/routes/releaseRoutes';
import _releaseDataFileService, {
  DataSetAccoutrements,
} from '@admin/services/releaseDataFileService';
import { DataReplacementResult } from '@admin/pages/release/data/components/DataFileReplacementPlan';
import { act, render, screen, within } from '@testing-library/react';
import { createMemoryHistory, MemoryHistory } from 'history';
import { generatePath, MemoryRouter, Route, Routes } from 'react-router';
import TestRouterRenderer from '@admin/components/testing/TestRouterRenderer';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

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

  test('renders the maintained and dropped paired indicators passed from the replacement', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockResolvedValue(
      testAccoutrements,
    );

    renderPageWithReplacementResult({
      replacementSubjectId: 'subject-2',
      pairedIndicators: [
        {
          parentIndicatorId: 'indicator-1',
          parentLabel: 'Indicator 1',
          childIndicatorId: 'indicator-2',
          childLabel: 'Indicator 2',
          maintained: true,
        },
        {
          parentIndicatorId: 'indicator-3',
          parentLabel: 'Indicator 3',
          childIndicatorId: 'indicator-4',
          childLabel: 'Indicator 4',
          maintained: false,
        },
      ],
    });

    expect(
      await screen.findByRole('heading', { name: 'Paired indicators' }),
    ).toBeInTheDocument();

    const maintained = within(
      screen.getByTestId('maintained-paired-indicators'),
    ).getAllByRole('listitem');
    expect(maintained).toHaveLength(1);
    expect(maintained[0]).toHaveTextContent(
      'Indicator 1, paired with Indicator 2',
    );

    const dropped = within(
      screen.getByTestId('dropped-paired-indicators'),
    ).getAllByRole('listitem');
    expect(dropped).toHaveLength(1);
    expect(dropped[0]).toHaveTextContent(
      'Indicator 3, paired with Indicator 4',
    );

    expect(
      screen.getByRole('link', { name: 'Manage paired indicators' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data?manageIndicators=subject-2#reordering',
    );
  });

  test('renders that there are no paired indicators to check when the replacement had none', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockResolvedValue(
      testAccoutrements,
    );

    renderPageWithReplacementResult({
      replacementSubjectId: 'subject-2',
      pairedIndicators: [],
    });

    expect(
      await screen.findByText('There are no paired indicators to check.'),
    ).toBeInTheDocument();
    expect(
      screen.queryByTestId('maintained-paired-indicators'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('dropped-paired-indicators'),
    ).not.toBeInTheDocument();
  });

  test('asks to check the paired indicators when the replacement result is not available', async () => {
    releaseDataFileService.getDataSetAccoutrementsSummary.mockResolvedValue(
      testAccoutrements,
    );

    await renderPage();

    expect(
      await screen.findByText(
        'Please check the paired indicators for this data set.',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Manage paired indicators' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data#reordering',
    );
  });

  function renderPageWithReplacementResult(result: DataReplacementResult) {
    render(
      <MemoryRouter
        initialEntries={[
          {
            pathname: generatePath(
              releaseDataFileReplacementCompleteRoute.fullPath,
              {
                publicationId: 'publication-1',
                releaseVersionId: 'release-1',
                fileId: 'file-1',
              },
            ),
            state: result,
          },
        ]}
      >
        <Routes>
          <Route
            path={releaseDataFileReplacementCompleteRoute.fullPath}
            element={
              <TestConfigContextProvider>
                <ReleaseDataFileReplacementCompletePage />
              </TestConfigContextProvider>
            }
          />
        </Routes>
      </MemoryRouter>,
    );
  }

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
