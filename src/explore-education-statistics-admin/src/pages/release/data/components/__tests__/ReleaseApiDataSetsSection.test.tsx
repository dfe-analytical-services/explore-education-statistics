import { AuthContextTestProvider, User } from '@admin/contexts/AuthContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseApiDataSetsSection from '@admin/pages/release/data/components/ReleaseApiDataSetsSection';
import _apiDataSetCandidateService, {
  ApiDataSetCandidate,
} from '@admin/services/apiDataSetCandidateService';
import _apiDataSetService, {
  ApiDataSetSummary,
} from '@admin/services/apiDataSetService';
import { GlobalPermissions } from '@admin/services/authService';
import { ReleaseVersion } from '@admin/services/releaseVersionService';
import render, { CustomRenderResult } from '@common-test/render';
import { createMemoryHistory, History } from 'history';
import { screen, waitFor, within } from '@testing-library/react';
import { Router } from 'react-router-dom';
import React from 'react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetCandidateService');

const apiDataSetCandidateService = jest.mocked(_apiDataSetCandidateService);
const apiDataSetService = jest.mocked(_apiDataSetService);

describe('ReleaseApiDataSetsSection', () => {
  const testBauUser: User = {
    id: 'user-id-1',
    name: 'BAU user',
    permissions: {
      isBauUser: true,
    } as GlobalPermissions,
  };

  const testAnalystUser: User = {
    id: 'user-id-1',
    name: 'Analyst user',
    permissions: {
      isBauUser: false,
    } as GlobalPermissions,
  };

  const testDataSets: ApiDataSetSummary[] = [
    {
      id: 'data-set-1',
      title: 'Data set 1 title',
      summary: 'Data set 1 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-1',
        version: '1.0',
        status: 'Draft',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-2',
      title: 'Data set 2 title',
      summary: 'Data set 2 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-2',
        version: '1.1',
        status: 'Draft',
        type: 'Minor',
      },
      latestLiveVersion: {
        id: 'version-3',
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        version: '1.0',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Major',
        published: '2024-05-01T09:30:00+00:00',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-3',
      title: 'Data set 3 title',
      summary: 'Data set 3 summary',
      status: 'Published',
      latestLiveVersion: {
        id: 'version-4',
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        version: '1.0',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Major',
        published: '2024-05-01T09:30:00+00:00',
      },
      previousReleaseIds: [],
    },
  ];

  const testCandidates: ApiDataSetCandidate[] = [
    {
      releaseFileId: 'release-file-1',
      title: 'Test data set 1',
    },
    {
      releaseFileId: 'release-file-2',
      title: 'Test data set 2',
    },
  ];

  test('renders draft and live data set tables correctly', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);
    apiDataSetService.listDataSets.mockResolvedValue(testDataSets);

    renderWithTestConfig();

    expect(await screen.findByText('Draft API data sets')).toBeInTheDocument();

    const draftsTable = within(screen.getByTestId('draft-api-data-sets'));

    const draftRows = draftsTable.getAllByRole('row');
    expect(draftRows).toHaveLength(3);

    const draftRow1Cells = within(draftRows[1]).getAllByRole('cell');
    expect(draftRow1Cells[0]).toHaveTextContent('v1.0');
    expect(draftRow1Cells[1]).toHaveTextContent('N/A');
    expect(draftRow1Cells[2]).toHaveTextContent('Data set 1 title');

    const draftRow2Cells = within(draftRows[2]).getAllByRole('cell');
    expect(draftRow2Cells[0]).toHaveTextContent('v1.1');
    expect(draftRow2Cells[1]).toHaveTextContent('v1.0');
    expect(draftRow2Cells[2]).toHaveTextContent('Data set 2 title');

    expect(screen.getByText('Current live API data sets')).toBeInTheDocument();

    const liveTable = within(screen.getByTestId('live-api-data-sets'));

    const liveRows = liveTable.getAllByRole('row');
    expect(liveRows).toHaveLength(2);

    const liveRows1Cells = within(liveRows[1]).getAllByRole('cell');
    expect(liveRows1Cells[0]).toHaveTextContent('v1.0');
    expect(liveRows1Cells[1]).toHaveTextContent('Data set 3 title');
  });

  test('renders correctly when no data sets exist', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);
    apiDataSetService.listDataSets.mockResolvedValue(testDataSets);

    renderWithTestConfig();

    expect(await screen.findByText('Draft API data sets')).toBeInTheDocument();
    expect(screen.getByText('Current live API data sets')).toBeInTheDocument();

    expect(
      screen.queryByText(
        'No API data sets have been created for this publication.',
      ),
    ).not.toBeInTheDocument();
  });

  test('renders info message when release has not been published', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);
    apiDataSetService.listDataSets.mockResolvedValue([]);

    renderPage();

    expect(
      await screen.findByText(
        'Changes will not be made in the public API until this release has been published.',
      ),
    ).toBeInTheDocument();
  });

  test('renders warning message when release has been approved and data sets cannot be created', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);
    apiDataSetService.listDataSets.mockResolvedValue([]);

    renderPage({
      releaseVersion: {
        ...testRelease,
        approvalStatus: 'Approved',
      },
    });

    expect(
      await screen.findByText(
        'This release has been approved and API data sets can no longer be created for it.',
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Create API data set' }),
    ).not.toBeInTheDocument();
  });

  test('does not render create button when user is not BAU', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue([]);
    apiDataSetService.listDataSets.mockResolvedValue([]);

    renderPage({ user: testAnalystUser });

    expect(
      await screen.findByText(
        'No API data sets have been created for this publication.',
      ),
    ).toBeInTheDocument();

    expect(screen.queryByText('Create API data set')).not.toBeInTheDocument();
  });

  test('clicking the create button opens modal form to create API data set', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);
    apiDataSetService.listDataSets.mockResolvedValue([]);

    const { user } = renderPage();

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    expect(
      await screen.findByText('Create a new API data set'),
    ).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'Create a new API data set' }),
    ).toBeInTheDocument();

    expect(modal.getByLabelText('Data set')).toBeInTheDocument();

    expect(
      modal.getByRole('button', { name: 'Confirm new API data set' }),
    ).toBeInTheDocument();
  });

  test('submitting the create version form calls the correct service and updates the modal', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);
    apiDataSetService.listDataSets.mockResolvedValue([]);
    apiDataSetService.createDataSet.mockResolvedValue({
      id: 'data-set-id',
      title: 'Test title',
      summary: 'Test summary',
      status: 'Draft',
      previousReleaseIds: [],
    });

    const history = createMemoryHistory();

    const { user } = renderPage({ history });

    expect(await screen.findByText('Create API data set')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Create API data set' }),
    );

    expect(
      await screen.findByText('Create a new API data set'),
    ).toBeInTheDocument();

    await user.selectOptions(
      await screen.findByLabelText('Data set'),
      testCandidates[0].releaseFileId,
    );

    expect(apiDataSetService.createDataSet).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new API data set' }),
    );

    await waitFor(() => {
      expect(apiDataSetService.createDataSet).toHaveBeenCalledTimes(1);
      expect(apiDataSetService.createDataSet).toHaveBeenCalledWith<
        Parameters<typeof apiDataSetService.createDataSet>
      >({
        releaseFileId: testCandidates[0].releaseFileId,
      });
    });

    expect(
      await screen.findByText('Creating API data set'),
    ).toBeInTheDocument();
  });

  function renderPage(options?: {
    releaseVersion?: ReleaseVersion;
    user?: User;
    history?: History;
  }): CustomRenderResult {
    const {
      releaseVersion = testRelease,
      user = testBauUser,
      history = createMemoryHistory(),
    } = options ?? {};

    return render(
      <AuthContextTestProvider user={user}>
        <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
          <Router history={history}>
            <ReleaseApiDataSetsSection />
          </Router>
        </ReleaseVersionContextProvider>
      </AuthContextTestProvider>,
    );
  }

  function renderWithTestConfig(options?: {
    releaseVersion?: ReleaseVersion;
    user?: User;
    history?: History;
  }): CustomRenderResult {
    const defaultTestConfig = {
      appInsightsKey: '',
      publicAppUrl: 'http://localhost',
      publicApiUrl: 'http://public-api',
      publicApiDocsUrl: 'http://public-api-docs',
      permittedEmbedUrlDomains: [
        'https://department-for-education.shinyapps.io',
      ],
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
    const {
      releaseVersion = testRelease,
      user = testBauUser,
      history = createMemoryHistory(),
    } = options ?? {};

    return render(
      <TestConfigContextProvider
        config={{
          ...defaultTestConfig,
          enableReplacementOfPublicApiDataSets: false,
        }}
      >
        <AuthContextTestProvider user={user}>
          <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
            <Router history={history}>
              <ReleaseApiDataSetsSection />
            </Router>
          </ReleaseVersionContextProvider>
        </AuthContextTestProvider>
      </TestConfigContextProvider>,
    );
  }
});
