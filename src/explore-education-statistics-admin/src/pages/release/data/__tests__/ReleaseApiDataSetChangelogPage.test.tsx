import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetChangelogPage from '@admin/pages/release/data/ReleaseApiDataSetChangelogPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetChangelogRoute,
  ReleaseDataSetChangelogRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService from '@admin/services/apiDataSetService';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';
import {
  ApiDataSetVersionChanges,
  ApiDataSetVersionChanges2,
} from '@admin/services/types/apiDataSetChanges';

jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetChangelogPage', () => {
  const testChanges: ApiDataSetVersionChanges2 = {
    majorChanges: {
      filters: [
        {
          previousState: {
            id: 'filter-1',
            column: 'filter_1',
            label: 'Filter 1',
            hint: 'Filter 1 Hint',
          },
        },
      ],
    },
    minorChanges: {
      filters: [
        {
          currentState: {
            id: 'filter-2',
            column: 'filter_2',
            label: 'Filter 2',
            hint: 'Filter 2 Hint',
          },
        },
      ],
    },
  };

  const draftDataSetVersionChanges: ApiDataSetVersionChanges = {
    dataSet: {
      id: 'data-set-id',
      title: 'Data set title',
    },
    dataSetVersion: {
      id: 'draft-version-id',
      version: '2.0',
      status: 'Draft',
      type: 'Major',
      notes: '',
    },
    changes: testChanges,
  };

  const liveDataSetVersionChanges: ApiDataSetVersionChanges = {
    dataSet: {
      id: 'data-set-id',
      title: 'Data set title',
    },
    dataSetVersion: {
      id: 'live-version-id',
      version: '1.1',
      status: 'Published',
      type: 'Minor',
      notes: 'Live version notes',
    },
    changes: testChanges,
  };

  test('renders correctly for a live API data set', async () => {
    apiDataSetVersionService.getChanges.mockResolvedValue({
      ...liveDataSetVersionChanges,
      changes: {
        ...liveDataSetVersionChanges.changes!,
        majorChanges: {},
      },
    });

    renderPage('live-version-id');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Published v1.1')).toBeInTheDocument();
    expect(screen.getByText('Minor update')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public guidance notes' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Live version notes')).toBeInTheDocument();

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

  test('renders correctly for a draft API data set', async () => {
    apiDataSetVersionService.getChanges.mockResolvedValue(
      draftDataSetVersionChanges,
    );

    renderPage('draft-version-id');

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.getByText('Draft v2.0')).toBeInTheDocument();
    expect(screen.getByText('Major update')).toBeInTheDocument();

    expect(screen.getByLabelText('Public guidance notes')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Save public guidance notes' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Major changes for version 2.0' }),
    ).toBeInTheDocument();

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(majorChanges.getByTestId('deleted-filters')).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    expect(
      screen.getByRole('heading', { name: 'Minor changes for version 2.0' }),
    ).toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(minorChanges.getByTestId('added-filters')).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('updating draft data set notes', async () => {
    apiDataSetVersionService.getChanges.mockResolvedValue(
      draftDataSetVersionChanges,
    );

    const { user } = renderPage('draft-version-id');

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
        'draft-version-id',
        { notes: 'Test notes' },
      );
    });
  });

  test('renders warning if unable to fetch API data set details', async () => {
    apiDataSetVersionService.getChanges.mockRejectedValue(
      new Error('Unable to fetch changes'),
    );

    renderPage('draft-version-id');

    await waitFor(() =>
      expect(
        screen.getByText('Could not load API data set'),
      ).toBeInTheDocument(),
    );
  });

  test('renders warning if unable to fetch changes', async () => {
    apiDataSetVersionService.getChanges.mockResolvedValue({
      ...draftDataSetVersionChanges,
      changes: undefined,
    });

    renderPage('draft-version-id');

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
