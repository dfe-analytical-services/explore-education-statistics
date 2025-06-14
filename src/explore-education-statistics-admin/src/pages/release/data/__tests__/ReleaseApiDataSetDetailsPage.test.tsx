import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease as testBaseRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
  ApiDataSetDraftVersion,
  ApiDataSetLiveVersion,
} from '@admin/services/apiDataSetService';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { ReleaseVersion } from '@admin/services/releaseVersionService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetDetailsPage', () => {
  const testLiveRelease: ReleaseVersion = {
    ...testBaseRelease,
    id: 'release-1-id',
    title: 'ReleaseVersion 1',
  };

  const testDraftRelease: ReleaseVersion = {
    ...testBaseRelease,
    id: 'release-2-id',
    title: 'ReleaseVersion 2',
  };

  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Published',
    previousReleaseIds: ['release-id'],
  };

  const testLiveVersion: ApiDataSetLiveVersion = {
    id: 'live-version-id',
    version: '1.1',
    type: 'Minor',
    status: 'Published',
    totalResults: 10_000,
    published: '2024-03-01T09:30:00+00:00',
    releaseVersion: {
      id: testLiveRelease.id,
      title: testLiveRelease.title,
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
      id: testDraftRelease.id,
      title: testDraftRelease.title,
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
      id: testDraftRelease.id,
      title: testDraftRelease.title,
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

  const defaultTestConfig = {
    appInsightsKey: '',
    publicAppUrl: 'http://localhost',
    publicApiUrl: 'http://public-api',
    publicApiDocsUrl: 'http://public-api-docs',
    permittedEmbedUrlDomains: ['https://department-for-education.shinyapps.io'],
    oidc: {
      clientId: '',
      authority: '',
      knownAuthorities: [''],
      adminApiScope: '',
      authorityMetadata: {
        authorizationEndpoint: '',
        tokenEndpoint: '',
        issuer: '',
        userInfoEndpoint: '',
        endSessionEndpoint: '',
      },
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

    expect(screen.queryByTestId('draft-version-tasks')).not.toBeInTheDocument();

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
    expect(summary.queryByTestId('Actions')).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('live-version-summary'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with processed draft version only', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
    });

    renderPage();

    expect(screen.queryByTestId('draft-version-tasks')).not.toBeInTheDocument();

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
      summary.getByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/changelog/draft-version-id',
    );
    expect(
      summary.getByRole('link', { name: 'Preview API data set' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview',
    );
    expect(
      summary.getByRole('link', { name: 'View preview token log' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview-tokens',
    );
    expect(
      summary.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();

    // Latest live version sections not rendered

    expect(
      screen.queryByTestId('live-version-summary'),
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

    expect(summary.getByTestId('Version')).toHaveTextContent('v1.1');
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
      summary.getByRole('link', { name: /View live data set/ }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/live-file-id',
    );
    expect(
      summary.getByRole('link', { name: 'View changelog and guidance notes' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/changelog/live-version-id',
    );
    expect(
      summary.getByRole('link', { name: 'View version history' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/versions',
    );

    // Draft version sections not rendered

    expect(screen.queryByTestId('draft-version-tasks')).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Draft version details' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('draft-version-summary'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly with draft and latest live version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        mappingStatus: {
          filtersComplete: false,
          locationsComplete: false,
          filtersHaveMajorChange: false,
          locationsHaveMajorChange: false,
          hasMajorVersionUpdate: false,
        },
      },
      latestLiveVersion: testLiveVersion,
    });

    renderPage();

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    // Tasks

    expect(
      screen.getByRole('heading', { name: 'Draft version tasks' }),
    ).toBeInTheDocument();

    const mapLocationsTask = within(screen.getByTestId('map-locations-task'));

    expect(
      mapLocationsTask.getByRole('link', { name: 'Map locations' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/locations-mapping',
    );
    expect(mapLocationsTask.getByText('Incomplete')).toBeInTheDocument();

    const mapFiltersTask = within(screen.getByTestId('map-filters-task'));

    expect(
      mapFiltersTask.getByRole('link', { name: 'Map filters' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/filters-mapping',
    );
    expect(mapFiltersTask.getByText('Incomplete')).toBeInTheDocument();

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
      draftSummary.getByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/changelog/draft-version-id',
    );
    expect(
      draftSummary.getByRole('link', { name: 'Preview API data set' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview',
    );
    expect(
      draftSummary.getByRole('link', { name: 'View preview token log' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview-tokens',
    );
    expect(
      draftSummary.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();

    // Latest live version

    expect(
      screen.getByRole('heading', { name: 'Latest live version details' }),
    ).toBeInTheDocument();

    const liveSummary = within(screen.getByTestId('live-version-summary'));

    expect(liveSummary.getByTestId('Version')).toHaveTextContent('v1.1');
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
      liveSummary.getByRole('link', { name: /View live data set/ }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/live-file-id',
    );
    expect(
      liveSummary.getByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/changelog/live-version-id',
    );
    expect(
      liveSummary.getByRole('link', { name: 'View version history' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/versions',
    );
  });

  test('renders the correct draft version (v1) actions', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '1.0',
      },
    });

    renderPage();

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('draft-version-summary'));

    expect(
      summary.getByRole('link', { name: 'Preview API data set' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview',
    );
    expect(
      summary.getByRole('link', { name: 'View preview token log' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-2-id/api-data-sets/data-set-id/preview-tokens',
    );
    expect(
      summary.getByRole('button', { name: 'Remove draft version' }),
    ).toBeInTheDocument();
    expect(
      summary.queryByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not render the `Remove draft version` button when release cannot be updated', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: testDraftVersion,
    });

    renderPage({
      releaseVersion: { ...testDraftRelease, approvalStatus: 'Approved' },
    });

    expect(
      await screen.findByText('Draft version details'),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('draft-version-summary'));

    expect(
      summary.queryByRole('button', { name: 'Remove draft version' }),
    ).not.toBeInTheDocument();
  });

  test('renders correct latest live version (v1) actions', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: {
        ...testLiveVersion,
        version: '1.0',
      },
    });

    renderPage();

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Latest live version details' }),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('live-version-summary'));

    // Not rendered for v1
    expect(
      summary.queryByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).not.toBeInTheDocument();
    expect(
      summary.queryByRole('link', { name: 'View version history' }),
    ).not.toBeInTheDocument();
  });

  test('renders correct latest live version (non-v1) actions when release cannot be updated', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({
      releaseVersion: { ...testDraftRelease, approvalStatus: 'Approved' },
    });

    expect(
      await screen.findByText('Latest live version details'),
    ).toBeInTheDocument();

    const summary = within(screen.getByTestId('live-version-summary'));

    expect(
      summary.getByRole('link', { name: /View live data set/ }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/live-file-id',
    );
    expect(
      summary.getByRole('link', {
        name: 'View changelog and guidance notes',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/changelog/live-version-id',
    );
    expect(
      summary.getByRole('link', { name: 'View version history' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1-id/api-data-sets/data-set-id/versions',
    );
  });

  test('renders the `Create new version` button when release can be updated and no draft version', async () => {
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

  test('does not render the `Create new version` button when release cannot be updated', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({
      releaseVersion: { ...testDraftRelease, approvalStatus: 'Approved' },
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

  test('does not render the `Create new version` button when there is a draft version', async () => {
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

  test('does not render the `Create new version` button when release series includes a previous version of this data set', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      latestLiveVersion: testLiveVersion,
    });

    renderPage({
      releaseVersion: {
        ...testDraftRelease,
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

  describe('finalising', () => {
    beforeEach(() => {
      jest.useFakeTimers({ advanceTimers: true });
    });
    afterEach(() => {
      jest.useRealTimers();
    });

    test('shows the finalise banner when mapping is complete', async () => {
      apiDataSetService.getDataSet.mockResolvedValue({
        ...testDataSet,
        draftVersion: {
          ...testDraftVersion,
          status: 'Mapping',
          mappingStatus: {
            filtersComplete: true,
            locationsComplete: true,
            hasMajorVersionUpdate: null,
            filtersHaveMajorChange: false,
            locationsHaveMajorChange: false,
          },
        },
        latestLiveVersion: testLiveVersion,
      });

      renderPage();

      expect(
        await screen.findByText('Draft version details'),
      ).toBeInTheDocument();

      const banner = within(screen.getByTestId('notificationBanner'));
      expect(
        banner.getByRole('heading', { name: 'Action required' }),
      ).toBeInTheDocument();
      expect(
        banner.getByText('Draft API data set version is ready to be finalised'),
      ).toBeInTheDocument();
      expect(
        banner.getByRole('button', { name: 'Finalise this data set version' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', { name: 'Draft version tasks' }),
      ).toBeInTheDocument();
    });

    test('successfully finalised', async () => {
      apiDataSetService.getDataSet.mockResolvedValueOnce({
        ...testDataSet,
        draftVersion: {
          ...testDraftVersion,
          status: 'Mapping',
          mappingStatus: {
            filtersComplete: true,
            locationsComplete: true,
            hasMajorVersionUpdate: null,
            filtersHaveMajorChange: false,
            locationsHaveMajorChange: false,
          },
        },
        latestLiveVersion: testLiveVersion,
      });
      apiDataSetService.getDataSet.mockResolvedValueOnce({
        ...testDataSet,
        draftVersion: {
          ...testDraftVersion,
          status: 'Draft',
          mappingStatus: {
            filtersComplete: true,
            locationsComplete: true,
            hasMajorVersionUpdate: null,
            filtersHaveMajorChange: false,
            locationsHaveMajorChange: false,
          },
        },
        latestLiveVersion: testLiveVersion,
      });

      const { user } = renderPage();

      expect(
        await screen.findByText('Draft version details'),
      ).toBeInTheDocument();

      expect(apiDataSetVersionService.completeVersion).not.toHaveBeenCalled();

      const banner = within(screen.getByTestId('notificationBanner'));
      expect(
        banner.getByRole('heading', { name: 'Action required' }),
      ).toBeInTheDocument();

      await user.click(
        banner.getByRole('button', { name: 'Finalise this data set version' }),
      );

      await waitFor(() => {
        expect(apiDataSetVersionService.completeVersion).toHaveBeenCalledTimes(
          1,
        );
        expect(apiDataSetVersionService.completeVersion).toHaveBeenCalledWith({
          dataSetVersionId: 'draft-version-id',
        });
      });

      expect(
        banner.getByRole('heading', { name: 'Finalising' }),
      ).toBeInTheDocument();
      expect(
        banner.getByText('Finalising draft API data set version'),
      ).toBeInTheDocument();
      expect(
        banner.queryByRole('button', {
          name: 'Finalise this data set version',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'Draft version tasks' }),
      ).not.toBeInTheDocument();

      jest.runOnlyPendingTimers();

      await waitFor(() => {
        expect(
          banner.getByText(
            'Draft API data set version is ready to be published',
          ),
        ).toBeInTheDocument();
      });

      expect(
        banner.getByRole('heading', {
          name: 'Mappings finalised',
        }),
      ).toBeInTheDocument();

      expect(
        banner.queryByRole('button', {
          name: 'Finalise this data set version',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('heading', { name: 'Draft version tasks' }),
      ).toBeInTheDocument();
    });

    test('finalising failed', async () => {
      apiDataSetService.getDataSet.mockResolvedValueOnce({
        ...testDataSet,
        draftVersion: {
          ...testDraftVersion,
          status: 'Mapping',
          mappingStatus: {
            filtersComplete: true,
            locationsComplete: true,
            hasMajorVersionUpdate: null,
            filtersHaveMajorChange: false,
            locationsHaveMajorChange: false,
          },
        },
        latestLiveVersion: testLiveVersion,
      });
      apiDataSetService.getDataSet.mockResolvedValueOnce({
        ...testDataSet,
        draftVersion: {
          ...testDraftVersion,
          status: 'Failed',
          mappingStatus: {
            filtersComplete: true,
            locationsComplete: true,
            filtersHaveMajorChange: false,
            locationsHaveMajorChange: false,
            hasMajorVersionUpdate: null,
          },
        },
        latestLiveVersion: testLiveVersion,
      });

      const { user } = renderPage();

      expect(
        await screen.findByText('Draft version details'),
      ).toBeInTheDocument();

      expect(apiDataSetVersionService.completeVersion).not.toHaveBeenCalled();

      const banner = within(screen.getByTestId('notificationBanner'));
      expect(
        banner.getByRole('heading', { name: 'Action required' }),
      ).toBeInTheDocument();

      await user.click(
        banner.getByRole('button', { name: 'Finalise this data set version' }),
      );

      await waitFor(() => {
        expect(apiDataSetVersionService.completeVersion).toHaveBeenCalledTimes(
          1,
        );
        expect(apiDataSetVersionService.completeVersion).toHaveBeenCalledWith({
          dataSetVersionId: 'draft-version-id',
        });
      });

      expect(
        banner.getByRole('heading', { name: 'Finalising' }),
      ).toBeInTheDocument();
      expect(
        banner.getByText('Finalising draft API data set version'),
      ).toBeInTheDocument();
      expect(
        banner.queryByRole('button', {
          name: 'Finalise this data set version',
        }),
      ).not.toBeInTheDocument();

      jest.runOnlyPendingTimers();

      await waitFor(() => {
        expect(banner.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        banner.getByText('Data set version finalisation failed'),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('heading', { name: 'Draft version tasks' }),
      ).not.toBeInTheDocument();
    });
  });

  test('renders error summary when draft version has a major version update and feature flag for replacement is turned on', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: true,
          filtersComplete: true,
          filtersHaveMajorChange: false,
          locationsHaveMajorChange: false,
        },
      },
    });

    const options = { enableReplacementOfPublicApiDataSets: true };
    renderPage(options);

    await waitFor(() => {
      expect(
        screen.queryByText(
          'This API data set can not be published because it is either incomplete or has a major version update.',
          {
            selector: 'h2',
          },
        ),
      ).toBeInTheDocument();

      expect(() =>
        screen.getByText('Draft API data set version is ready to be published'),
      ).toThrow('Unable to find an element');
    });
  });

  test('renders "Major Change" when user has selected a major change for the filters and feature flag for replacement is turned on', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: false,
          filtersComplete: true,
          filtersHaveMajorChange: true,
          locationsHaveMajorChange: false,
        },
      },
    });

    const options = { enableReplacementOfPublicApiDataSets: true };
    renderPage(options);

    await waitFor(() => {
      expect(
        screen.queryByText(
          'This API data set can not be published because it is either incomplete or has a major version update.',
          {
            selector: 'h2',
          },
        ),
      ).toBeInTheDocument();

      expect(() =>
        screen.getByText('Draft API data set version is ready to be published'),
      ).toThrow('Unable to find an element');

      const mapLocationsTask = within(screen.getByTestId('map-locations-task'));
      const mapFiltersTask = within(screen.getByTestId('map-filters-task'));

      expect(
        mapFiltersTask.queryByText('Major Change', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
      expect(
        mapLocationsTask.queryByText('Incomplete', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
    });
  });

  test('renders "Major Change" when user has selected a major change for the locations and feature flag for replacement is turned on', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: false,
          filtersComplete: true,
          filtersHaveMajorChange: true,
          locationsHaveMajorChange: false,
        },
      },
    });

    const options = { enableReplacementOfPublicApiDataSets: true };
    renderPage(options);

    await waitFor(() => {
      expect(
        screen.queryByText(
          'This API data set can not be published because it is either incomplete or has a major version update.',
          {
            selector: 'h2',
          },
        ),
      ).toBeInTheDocument();

      expect(() =>
        screen.getByText('Draft API data set version is ready to be published'),
      ).toThrow('Unable to find an element');

      const mapLocationsTask = within(screen.getByTestId('map-locations-task'));
      const mapFiltersTask = within(screen.getByTestId('map-filters-task'));

      expect(
        mapFiltersTask.queryByText('Major Change', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
      expect(
        mapLocationsTask.queryByText('Incomplete', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
    });
  });

  test('Doesnt render "Major Change" when user has selected a major change for the filters and feature flag for replacement is turned off', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: false,
          filtersComplete: true,
          filtersHaveMajorChange: true,
          locationsHaveMajorChange: false,
        },
      },
    });

    const options = { enableReplacementOfPublicApiDataSets: false };
    renderPage(options);

    await waitFor(() => {
      expect(
        screen.queryByText(
          'This API data set can not be published because it is either incomplete or has a major version update.',
          {
            selector: 'h2',
          },
        ),
      ).not.toBeInTheDocument();

      expect(() =>
        screen.getByText('Draft API data set version is ready to be published'),
      ).toThrow('Unable to find an element');

      const mapLocationsTask = within(screen.getByTestId('map-locations-task'));
      const mapFiltersTask = within(screen.getByTestId('map-filters-task'));

      expect(
        mapFiltersTask.queryByText('Major Change', {
          selector: 'strong',
        }),
      ).not.toBeInTheDocument();
      expect(
        mapLocationsTask.queryByText('Incomplete', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
    });
  });

  test('Doesnt render "Major Change" when user has selected a major change for the locations and feature flag for replacement is turned off', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: false,
          filtersComplete: true,
          filtersHaveMajorChange: true,
          locationsHaveMajorChange: false,
        },
      },
    });

    const options = { enableReplacementOfPublicApiDataSets: false };
    renderPage(options);

    await waitFor(() => {
      expect(
        screen.queryByText(
          'This API data set can not be published because it is either incomplete or has a major version update.',
          {
            selector: 'h2',
          },
        ),
      ).not.toBeInTheDocument();

      expect(() =>
        screen.getByText('Draft API data set version is ready to be published'),
      ).toThrow('Unable to find an element');

      const mapLocationsTask = within(screen.getByTestId('map-locations-task'));
      const mapFiltersTask = within(screen.getByTestId('map-filters-task'));

      expect(
        mapFiltersTask.queryByText('Major Change', {
          selector: 'strong',
        }),
      ).not.toBeInTheDocument();
      expect(
        mapLocationsTask.queryByText('Incomplete', {
          selector: 'strong',
        }),
      ).toBeInTheDocument();
    });
  });

  test('Does not render error summary when draft version has a major version update and feature flag for replacement is turned off', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: true,
          filtersComplete: true,
          filtersHaveMajorChange: false,
          locationsHaveMajorChange: false,
        },
      },
    });

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText('Draft API data set version is ready to be published'),
      ).toBeInTheDocument();

      expect(() =>
        screen.getByText(
          'The data file uploaded has incomplete sections or has resulted in a major version update which is not allowed in release amendments.',
        ),
      ).toThrow('Unable to find an element');
    });
  });

  test('does not render error summary when draft version does not have a major version update', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '1.0.1',
        mappingStatus: {
          hasMajorVersionUpdate: false,
          locationsComplete: false,
          filtersComplete: false,
          filtersHaveMajorChange: false,
          locationsHaveMajorChange: false,
        },
      },
    });

    renderPage();

    await waitFor(() => {
      expect(() =>
        screen.getByText(
          'The data file uploaded has incomplete sections or has resulted in a major version update which is not allowed in release amendments.',
        ),
      ).toThrow('Unable to find an element');
    });
  });

  test('does not render error summary when draft version is not a patch version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue({
      ...testDataSet,
      draftVersion: {
        ...testDraftVersion,
        version: '2.0',
        mappingStatus: {
          hasMajorVersionUpdate: true,
          locationsComplete: true,
          filtersComplete: true,
          filtersHaveMajorChange: false,
          locationsHaveMajorChange: false,
        },
      },
    });

    renderPage();

    await waitFor(() => {
      expect(() =>
        screen.getByText(
          'The data file uploaded has incomplete sections or has resulted in a major version update which is not allowed in release amendments.',
        ),
      ).toThrow('Unable to find an element');
    });
  });

  function renderPage(options?: {
    releaseVersion?: ReleaseVersion;
    dataSetId?: string;
    enableReplacementOfPublicApiDataSets?: boolean;
  }) {
    const { releaseVersion = testDraftRelease, dataSetId = 'data-set-id' } =
      options ?? {};

    return render(
      <TestConfigContextProvider
        config={{
          ...defaultTestConfig,
          enableReplacementOfPublicApiDataSets:
            options?.enableReplacementOfPublicApiDataSets ?? false,
        }}
      >
        <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetDetailsRoute.path,
                {
                  publicationId: releaseVersion.publicationId,
                  releaseVersionId: releaseVersion.id,
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
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
