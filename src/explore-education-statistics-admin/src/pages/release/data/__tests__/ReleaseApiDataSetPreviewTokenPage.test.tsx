import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import ReleaseApiDataSetPreviewTokenPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenPage';
import {
  releaseApiDataSetPreviewTokenRoute,
  ReleaseDataSetPreviewTokenRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import _previewTokenService, {
  PreviewToken,
} from '@admin/services/previewTokenService';
import { Release } from '@admin/services/releaseService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import addHours from 'date-fns/addHours';
import { createMemoryHistory, MemoryHistory } from 'history';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/previewTokenService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const previewTokenService = jest.mocked(_previewTokenService);

describe('ReleaseApiDataSetPreviewTokenPage', () => {
  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Draft',
    draftVersion: {
      id: 'draft-version-id',
      version: '2.0',
      status: 'Draft',
      type: 'Minor',
      totalResults: 0,
      releaseVersion: {
        id: 'release-2-id',
        title: 'Test release 2',
      },
      file: {
        id: 'draft-file-id',
        title: 'Test draft file',
      },
    },
    previousReleaseIds: [],
  };
  const now = new Date();
  const testToken: PreviewToken = {
    id: 'token-id',
    label: 'Test label',
    status: 'Active',
    created: now.toISOString(),
    createdByEmail: 'test@gov.uk',
    updated: '',
    expiry: addHours(now, 24).toISOString(),
  };

  test('renders correctly with an active token', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.getPreviewToken.mockResolvedValue(testToken);

    renderPage();

    expect(
      await screen.findByText('API data set preview token'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Data set title' }),
    ).toBeInTheDocument();

    const expiry = `tomorrow at ${now.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
    })}`;

    expect(screen.getByText('The token expires:')).toBeInTheDocument();
    expect(screen.getByText(expiry)).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Using the preview token' }),
    ).toBeInTheDocument();

    expect(screen.getByLabelText('Preview token')).toHaveValue('token-id');
    expect(
      screen.getByRole('button', { name: 'Copy preview token' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Revoke preview token' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'View preview token log' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'API data set endpoints quick start',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'API data set details',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Get data set summary',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Get data set metadata',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Query data set (GET)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Query data set (POST)',
      }),
    ).toBeInTheDocument();
  });

  test('shows a modal and calls the `onRevoke` handler on confirm when the revoke button is clicked', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.getPreviewToken.mockResolvedValue(testToken);

    const history = createMemoryHistory();

    const { user } = renderPage({ history });

    expect(
      await screen.findByText('API data set preview token'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Revoke preview token' }),
    );

    expect(
      await screen.findByText(
        'Are you sure you want to revoke the preview token?',
      ),
    ).toBeInTheDocument();

    expect(previewTokenService.revokePreviewToken).not.toHaveBeenCalled();

    await user.click(
      within(screen.getByRole('dialog')).getByRole('button', {
        name: 'Confirm',
      }),
    );

    await waitFor(() => {
      expect(previewTokenService.revokePreviewToken).toHaveBeenCalledTimes(1);
    });

    expect(previewTokenService.revokePreviewToken).toHaveBeenCalledWith(
      'token-id',
    );

    await waitFor(() => {
      expect(history.location.pathname).toBe(
        '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview-tokens',
      );
    });
  });

  test('renders a message when the token has expired', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.getPreviewToken.mockResolvedValue({
      ...testToken,
      status: 'Expired',
    });

    renderPage();

    expect(
      await screen.findByText('API data set preview token'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Data set title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('This preview token has expired.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Using this token' }),
    ).not.toBeInTheDocument();
  });

  function renderPage(options?: {
    release?: Release;
    dataSetId?: string;
    previewTokenId?: string;
    history?: MemoryHistory;
  }) {
    const {
      release = testRelease,
      dataSetId = 'data-set-id',
      previewTokenId = 'token-id',
      history = createMemoryHistory(),
    } = options ?? {};

    history.push(
      generatePath<ReleaseDataSetPreviewTokenRouteParams>(
        releaseApiDataSetPreviewTokenRoute.path,
        {
          publicationId: release.publicationId,
          releaseId: release.id,
          dataSetId,
          previewTokenId,
        },
      ),
    );

    return render(
      <TestConfigContextProvider>
        <ReleaseContextProvider release={release}>
          <Router history={history}>
            <Route
              component={ReleaseApiDataSetPreviewTokenPage}
              path={releaseApiDataSetPreviewTokenRoute.path}
            />
          </Router>
        </ReleaseContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
