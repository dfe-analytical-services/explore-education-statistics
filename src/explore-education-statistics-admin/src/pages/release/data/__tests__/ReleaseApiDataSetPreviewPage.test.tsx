import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetPreviewPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetPreviewRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import { ReleaseVersion } from '@admin/services/releaseVersionService';
import _previewTokenService, {
  PreviewToken,
} from '@admin/services/previewTokenService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, Route, Router } from 'react-router-dom';
import { createMemoryHistory, MemoryHistory } from 'history';
import { addDays, addHours } from 'date-fns';
import { formatInTimeZone, fromZonedTime } from 'date-fns-tz';

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
    activates: now.toISOString(),
    createdByEmail: 'test@gov.uk',
    updated: '',
    expires: addHours(now, 24).toISOString(),
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
      activates: expect.any(Date),
      expires: expect.any(Date),
      label: 'Test label',
    });
    const fixedDate = new Date();
    const TZ = 'Europe/London';

    // Arrange expected end: end of *tomorrow* in London
    // 1) London calendar for "today"
    const todayYmdLondon = formatInTimeZone(fixedDate, TZ, 'yyyy-MM-dd');
    // 2) Build "today at 00:00 London" → get UTC instant, add 1 day in absolute time, then get "tomorrow" YMD in London
    const todayMidnightUtc = fromZonedTime(`${todayYmdLondon}T00:00:00`, TZ);
    const plusOneDayUtc = addDays(todayMidnightUtc, 1);
    const tomorrowYmdLondon = formatInTimeZone(plusOneDayUtc, TZ, 'yyyy-MM-dd');
    // 3) End of tomorrow (London wall time) → exact UTC instant
    const expectedEndUtc = fromZonedTime(
      `${tomorrowYmdLondon}T23:59:59.999`,
      TZ,
    );

    const [[args]] = (previewTokenService.createPreviewToken as jest.Mock).mock
      .calls;
    // Assert: start is "now" (tight tolerance to avoid flakiness on CI)
    expect(
      Math.abs(args.activates.getTime() - fixedDate.getTime()),
    ).toBeLessThanOrEqual(100);

    // Assert: end equals exact end-of-tomorrow instant
    expect(args.expires.toISOString()).toBe(expectedEndUtc.toISOString());

    await waitFor(() => {
      expect(history.location.pathname).toBe(
        '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview-tokens/token-id',
      );
    });
  });

  function renderPage(options?: {
    releaseVersion?: ReleaseVersion;
    dataSetId?: string;
    previewTokenId?: string;
    history?: MemoryHistory;
  }) {
    const {
      releaseVersion = testRelease,
      dataSetId = 'data-set-id',
      history = createMemoryHistory(),
    } = options ?? {};

    history.push(
      generatePath<ReleaseDataSetRouteParams>(
        releaseApiDataSetPreviewRoute.path,
        {
          publicationId: releaseVersion.publicationId,
          releaseVersionId: releaseVersion.id,
          dataSetId,
        },
      ),
    );

    return render(
      <TestConfigContextProvider>
        <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
          <Router history={history}>
            <Route
              component={ReleaseApiDataSetPreviewPage}
              path={releaseApiDataSetPreviewRoute.path}
            />
          </Router>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
