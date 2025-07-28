import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetChangelogPage from '@admin/pages/release/data/ReleaseApiDataSetChangelogPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetChangelogRoute,
  ReleaseDataSetChangelogRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
  ApiDataSetVersionInfo,
} from '@admin/services/apiDataSetService';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetChangelogPage', () => {
  const draftDataSetVersion: ApiDataSetVersionInfo = {
    id: 'data-set-version-4-0',
    version: '4.0',
    status: 'Draft',
    type: 'Major',
    notes: '',
  };

  const latestLiveDataSetVersion: ApiDataSetVersionInfo = {
    id: 'data-set-version-3-0',
    version: '3.0',
    status: 'Published',
    type: 'Major',
    notes: 'Version 3.0 notes',
  };

  const dataSetVersionWithOnlyMajorChanges: ApiDataSetVersionInfo = {
    id: 'data-set-version-2-0',
    version: '2.0',
    status: 'Published',
    type: 'Major',
    notes: 'Version 2.0 notes',
  };

  const dataSetVersionWithOnlyMinorChanges: ApiDataSetVersionInfo = {
    id: 'data-set-version-1-1',
    version: '1.1',
    status: 'Published',
    type: 'Minor',
    notes: 'Version 1.1 notes',
  };

  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Draft',
    draftVersion: {
      id: draftDataSetVersion.id,
      version: draftDataSetVersion.version,
      status: 'Draft',
      type: draftDataSetVersion.type,
      totalResults: 0,
      releaseVersion: {
        id: 'release-3-id',
        title: 'Test release 3',
      },
      file: {
        id: 'draft-file-id',
        title: 'Test draft file',
      },
    },
    latestLiveVersion: {
      id: latestLiveDataSetVersion.id,
      version: latestLiveDataSetVersion.version,
      status: 'Published',
      type: latestLiveDataSetVersion.type,
      totalResults: 0,
      releaseVersion: {
        id: 'release-2-id',
        title: 'Test release 2',
      },
      file: {
        id: 'draft-file-id',
        title: 'Test draft file',
      },
      timePeriods: { start: '', end: '' },
      geographicLevels: [],
      filters: [],
      published: '',
      indicators: [],
      notes: latestLiveDataSetVersion.notes,
    },
    previousReleaseIds: [],
  };

  const majorChangeSet: ChangeSet = {
    filters: [
      {
        previousState: {
          id: 'filter-1',
          column: 'filter_1',
          label: 'Filter 1',
          hint: '',
        },
      },
    ],
  };

  const minorChangeSet: ChangeSet = {
    filters: [
      {
        currentState: {
          id: 'filter-2',
          column: 'filter_2',
          label: 'Filter 2',
          hint: '',
        },
      },
    ],
  };

  test('renders correctly for a LIVE API data set with MAJOR AND MINOR changes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockResolvedValue(
      latestLiveDataSetVersion,
    );
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-3-0');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Published v3.0')).toBeInTheDocument();
    expect(screen.getByText('Major update')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public guidance notes' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Version 3.0 notes')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Major changes for version 3.0' }),
    ).toBeInTheDocument();

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    expect(
      screen.getByRole('heading', { name: 'Minor changes for version 3.0' }),
    ).toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders correctly for a DRAFT API data set with MAJOR AND MINOR changes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockResolvedValue(draftDataSetVersion);
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-4-0');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Draft v4.0')).toBeInTheDocument();
    expect(screen.getByText('Major update')).toBeInTheDocument();

    expect(screen.getByLabelText('Public guidance notes')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Major changes for version 4.0' }),
    ).toBeInTheDocument();

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    expect(
      screen.getByRole('heading', { name: 'Minor changes for version 4.0' }),
    ).toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders correctly for a LIVE API data set with ONLY MAJOR changes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockResolvedValue(
      dataSetVersionWithOnlyMajorChanges,
    );
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: {},
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-2-0');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Published v2.0')).toBeInTheDocument();
    expect(screen.getByText('Major update')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public guidance notes' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Version 2.0 notes')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Major changes for version 2.0' }),
    ).toBeInTheDocument();

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    expect(
      screen.queryByRole('heading', { name: 'Minor changes for version 2.0' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly for a LIVE API data set with ONLY MINOR changes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockResolvedValue(
      dataSetVersionWithOnlyMinorChanges,
    );
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: {},
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-1-1');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Published v1.1')).toBeInTheDocument();
    expect(screen.getByText('Minor update')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public guidance notes' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Version 1.1 notes')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Major changes for version 1.1' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Minor changes for version 1.1' }),
    ).toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('updating draft data set notes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion
      .mockResolvedValueOnce(draftDataSetVersion)
      .mockResolvedValueOnce({ ...draftDataSetVersion, notes: 'Test notes' });
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    const { user } = renderPage('data-set-version-4-0');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    await user.type(
      screen.getByLabelText('Public guidance notes'),
      'Test notes',
    );

    expect(apiDataSetVersionService.updateNotes).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    );

    await waitFor(() => {
      expect(apiDataSetVersionService.updateNotes).toHaveBeenCalledTimes(1);
      expect(apiDataSetVersionService.updateNotes).toHaveBeenCalledWith(
        'data-set-version-4-0',
        { notes: 'Test notes' },
      );
    });

    expect(screen.getByTestId('public-guidance-notes')).toHaveTextContent(
      'Test notes',
    );
  });

  test('renders warning if unable to fetch data set', async () => {
    apiDataSetService.getDataSet.mockRejectedValue(
      new Error('Unable to fetch data set'),
    );
    apiDataSetVersionService.getVersion.mockResolvedValue(draftDataSetVersion);
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-4-0');

    expect(screen.queryByText('Data set title')).not.toBeInTheDocument();

    expect(
      await screen.findByText('Could not load API data set'),
    ).toBeInTheDocument();
  });

  test('renders warning if unable to fetch data set version', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockRejectedValue(
      new Error('Unable to fetch data set'),
    );
    apiDataSetVersionService.getChanges.mockResolvedValue({
      majorChanges: majorChangeSet,
      minorChanges: minorChangeSet,
      versionNumber: '1.0.0',
      notes: '',
      patchHistory: [],
    });

    renderPage('data-set-version-4-0');

    expect(screen.queryByText('Data set title')).not.toBeInTheDocument();

    expect(
      await screen.findByText('Could not load API data set'),
    ).toBeInTheDocument();
  });

  test('renders warning if unable to fetch changes', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getVersion.mockResolvedValue(draftDataSetVersion);
    apiDataSetVersionService.getChanges.mockRejectedValue(
      new Error('Unable to fetch changes'),
    );

    renderPage('data-set-version-4-0');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Could not load changelog')).toBeInTheDocument();
  });

  function renderPage(dataSetVersionId: string) {
    return render(
      <TestConfigContextProvider>
        <ReleaseVersionContextProvider releaseVersion={testRelease}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetChangelogRouteParams>(
                releaseApiDataSetChangelogRoute.path,
                {
                  publicationId: testRelease.publicationId,
                  releaseVersionId: testRelease.id,
                  dataSetId: 'data-set-id',
                  dataSetVersionId,
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetChangelogPage}
              path={releaseApiDataSetChangelogRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
