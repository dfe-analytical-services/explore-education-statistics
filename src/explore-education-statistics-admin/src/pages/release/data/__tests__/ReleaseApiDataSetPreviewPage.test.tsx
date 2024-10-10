import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetPreviewPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewPage';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import {
  releaseApiDataSetPreviewRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import { Release } from '@admin/services/releaseService';
import _previewTokenService, {
  PreviewToken,
} from '@admin/services/previewTokenService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';
import { createMemoryHistory, MemoryHistory } from 'history';
import addHours from 'date-fns/addHours';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/previewTokenService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const previewTokenService = jest.mocked(_previewTokenService);

describe('ReleaseApiDataSetPreviewPage', () => {
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

  test('renders correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);

    renderPage();

    expect(
      await screen.findByText('Generate API data set preview token'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Data set title' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Generate preview token' }),
    ).toBeInTheDocument();
  });

  test('generating a preview token', async () => {
    const history = createMemoryHistory();
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.createPreviewToken.mockResolvedValue(testToken);

    const { user } = renderPage({ history });

    expect(
      await screen.findByText('Generate API data set preview token'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Generate preview token' }),
    );

    expect(await screen.findByText('Token name')).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    await user.type(modal.getByLabelText('Token name'), 'Test label');
    await user.click(modal.getByLabelText(/I agree/));

    expect(previewTokenService.createPreviewToken).not.toHaveBeenCalled();

    await user.click(modal.getByRole('button', { name: 'Continue' }));

    await waitFor(() => {
      expect(previewTokenService.createPreviewToken).toHaveBeenCalledTimes(1);
    });

    expect(previewTokenService.createPreviewToken).toHaveBeenCalledWith({
      dataSetVersionId: 'draft-version-id',
      label: 'Test label',
    });

    await waitFor(() => {
      expect(history.location.pathname).toBe(
        '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview-tokens/token-id',
      );
    });
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
      history = createMemoryHistory(),
    } = options ?? {};

    history.push(
      generatePath<ReleaseDataSetRouteParams>(
        releaseApiDataSetPreviewRoute.path,
        {
          publicationId: release.publicationId,
          releaseId: release.id,
          dataSetId,
        },
      ),
    );

    return render(
      <TestConfigContextProvider>
        <ReleaseContextProvider release={release}>
          <Router history={history}>
            <Route
              component={ReleaseApiDataSetPreviewPage}
              path={releaseApiDataSetPreviewRoute.path}
            />
          </Router>
        </ReleaseContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
