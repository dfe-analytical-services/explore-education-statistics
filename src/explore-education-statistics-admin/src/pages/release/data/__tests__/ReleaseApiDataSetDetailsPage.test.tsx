import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
  ApiDataSetDraftVersion,
  ApiDataSetLiveVersion,
} from '@admin/services/apiDataSetService';
import { Release } from '@admin/services/releaseService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');

const apiDataSetService = jest.mocked(_apiDataSetService);

describe('ReleaseApiDataSetDetailsPage', () => {
  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Published',
    previousReleaseIds: ['release-id'],
  };

  const testLiveVersion: ApiDataSetLiveVersion = {
    id: 'live-version-id',
    version: '1.0',
    type: 'Minor',
    status: 'Published',
    totalResults: 10_000,
    published: '2024-03-01T09:30:00+00:00',
    releaseVersion: {
      id: 'release-1-id',
      title: 'Test release 1',
    },
    file: {
      id: 'live-file-id',
      title: 'Test live file',
    },
    filters: ['Test live filter'],
    geographicLevels: ['National', 'Local authority'],
    indicators: ['Test live indicator'],
    timePeriods: {
      start: '2018',
      end: '2023',
    },
  };

  const testProcessingVersion: ApiDataSetDraftVersion = {
    id: 'processing-version-id',
    version: '2.0',
    status: 'Processing',
    type: 'Minor',
    totalResults: 0,
    releaseVersion: {
      id: 'release-2-id',
      title: 'Test release 2',
    },
    file: {
      id: 'processing-file-id',
      title: 'Test processing file',
    },
  };

  const testDraftVersion: ApiDataSetDraftVersion = {
    id: 'draft-version-id',
    version: '2.0',
    status: 'Draft',
    type: 'Major',
    totalResults: 20_000,
    releaseVersion: {
      id: 'release-2-id',
      title: 'Test release 2',
    },
    file: {
      id: 'draft-file-id',
      title: 'Test draft file',
    },
    filters: ['Test draft filter'],
    geographicLevels: ['National', 'Local authority'],
    indicators: ['Test draft indicator'],
    timePeriods: {
      start: '2018',
      end: '2024',
    },
  };

  test('renders correctly with data set summary', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testProcessingVersion,
    });

    renderPage();

    const summary = within(await screen.findByTestId('data-set-summary'));

    expect(summary.getByTestId('Status')).toHaveTextContent('Published');
    expect(summary.getByTestId('Summary')).toHaveTextContent(
      'Data set summary',
    );
  });

  test('renders correctly with processing draft version only', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testProcessingVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Draft version details' }),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('draft-version-summary'));

    expect(summary.getByTestId('Version')).toHaveTextContent('v2.0');
    expect(summary.getByTestId('Status')).toHaveTextContent('Processing');

    expect(summary.queryByTestId('Time periods')).not.toBeInTheDocument();
    expect(summary.queryByTestId('Geographic levels')).not.toBeInTheDocument();
    expect(summary.queryByTestId('Indicators')).not.toBeInTheDocument();
    expect(summary.queryByTestId('Filters')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Latest live version details' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with processed draft version only', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Draft version details' }),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('draft-version-summary'));

    expect(summary.getByTestId('Version')).toHaveTextContent('v2.0');
    expect(summary.getByTestId('Status')).toHaveTextContent('Ready');
    expect(summary.getByTestId('Time periods')).toHaveTextContent(
      '2018 to 2024',
    );
    expect(summary.getByTestId('Geographic levels')).toHaveTextContent(
      'National, Local authority',
    );
    expect(summary.getByTestId('Indicators')).toHaveTextContent(
      'Test draft indicator',
    );
    expect(summary.getByTestId('Filters')).toHaveTextContent(
      'Test draft filter',
    );
    expect(
      screen.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Latest live version details' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with latest live version only', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Latest live version details' }),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('live-version-summary'));

    expect(summary.getByTestId('Version')).toHaveTextContent('v1.0');
    expect(summary.getByTestId('Status')).toHaveTextContent('Published');
    expect(summary.getByTestId('Time periods')).toHaveTextContent(
      '2018 to 2023',
    );
    expect(summary.getByTestId('Indicators')).toHaveTextContent(
      'Test live indicator',
    );
    expect(summary.getByTestId('Filters')).toHaveTextContent(
      'Test live filter',
    );
    expect(
      within(summary.getByTestId('Actions')).getByRole('link', {
        name: /View live data set/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/live-file-id',
    );

    expect(
      screen.queryByRole('button', { name: 'Remove draft version' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Draft version details' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with draft and latest live version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
      latestLiveVersion: testLiveVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    // Draft version

    expect(
      screen.getByRole('heading', { name: 'Draft version details' }),
    ).toBeInTheDocument();

    const draftSummary = within(screen.getByTestId('draft-version-summary'));

    expect(draftSummary.getByTestId('Version')).toHaveTextContent('v2.0');
    expect(draftSummary.getByTestId('Status')).toHaveTextContent('Ready');
    expect(draftSummary.getByTestId('Time periods')).toHaveTextContent(
      '2018 to 2024',
    );
    expect(draftSummary.getByTestId('Geographic levels')).toHaveTextContent(
      'National, Local authority',
    );
    expect(draftSummary.getByTestId('Indicators')).toHaveTextContent(
      'Test draft indicator',
    );
    expect(draftSummary.getByTestId('Filters')).toHaveTextContent(
      'Test draft filter',
    );
    expect(
      draftSummary.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();

    // Latest live version

    expect(
      screen.getByRole('heading', { name: 'Latest live version details' }),
    ).toBeInTheDocument();

    const liveSummary = within(screen.getByTestId('live-version-summary'));

    expect(liveSummary.getByTestId('Version')).toHaveTextContent('v1.0');
    expect(liveSummary.getByTestId('Status')).toHaveTextContent('Published');
    expect(liveSummary.getByTestId('Time periods')).toHaveTextContent(
      '2018 to 2023',
    );
    expect(liveSummary.getByTestId('Indicators')).toHaveTextContent(
      'Test live indicator',
    );
    expect(liveSummary.getByTestId('Filters')).toHaveTextContent(
      'Test live filter',
    );
    expect(
      within(liveSummary.getByTestId('Actions')).getByRole('link', {
        name: /View live data set/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/live-file-id',
    );
  });

  test('does not render the Remove draft version button when cannot update the release', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({ release: { ...testRelease, approvalStatus: 'Approved' } });

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    const draftSummary = within(screen.getByTestId('draft-version-summary'));
    expect(
      draftSummary.queryByRole('button', { name: 'Remove draft version' }),
    ).not.toBeInTheDocument();
  });

  test('renders the create new version button when release can be updated and there is no draft version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Create a new version of this data set',
      }),
    ).toBeInTheDocument();
  });

  test('does not render the create new version button when cannot update the release', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({ release: { ...testRelease, approvalStatus: 'Approved' } });

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Create a new version of this data set',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not render the create new version button when there is a draft version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
      latestLiveVersion: testLiveVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Create a new version of this data set',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not render the create new version button when release series includes a previous version of this data set', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({
      release: {
        ...testRelease,
        releaseId: testDataSet.previousReleaseIds[0],
      },
    });

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Create a new version of this data set',
      }),
    ).not.toBeInTheDocument();
  });

  function renderPage(options?: { release?: Release; dataSetId?: string }) {
    const { release = testRelease, dataSetId = 'data-set-id' } = options ?? {};

    render(
      <TestConfigContextProvider>
        <ReleaseContextProvider release={release}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetDetailsRoute.path,
                {
                  publicationId: release.publicationId,
                  releaseId: release.id,
                  dataSetId,
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetDetailsPage}
              path={releaseApiDataSetDetailsRoute.path}
            />
          </MemoryRouter>
        </ReleaseContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
