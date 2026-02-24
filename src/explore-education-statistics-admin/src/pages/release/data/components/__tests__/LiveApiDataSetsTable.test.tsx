import LiveApiDataSetsTable, {
  LiveApiDataSetSummary,
} from '@admin/pages/release/data/components/LiveApiDataSetsTable';
import _apiDataSetCandidateService, {
  ApiDataSetCandidate,
} from '@admin/services/apiDataSetCandidateService';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import baseRender from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import { createMemoryHistory, History } from 'history';
import { ReactNode } from 'react';
import { Router } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetVersionService');
jest.mock('@admin/services/apiDataSetCandidateService');

const apiDataSetCandidateService = jest.mocked(_apiDataSetCandidateService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('LiveApiDataSetsTable', () => {
  const testDataSets: LiveApiDataSetSummary[] = [
    {
      id: 'data-set-2',
      title: 'Data set 2 title',
      summary: 'Data set 2 summary',
      status: 'Published',
      latestLiveVersion: {
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-2',
        version: '1.0',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Major',
      },
      previousReleaseIds: ['previous-release-id'],
    },
    {
      id: 'data-set-1',
      title: 'Data set 1 title',
      summary: 'Data set 1 summary',
      status: 'Published',
      latestLiveVersion: {
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-1',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        version: '2.0',
        status: 'Published',
        type: 'Major',
      },
      previousReleaseIds: ['previous-release-id'],
    },
    {
      id: 'data-set-3',
      title: 'Data set 3 title',
      summary: 'Data set 3 summary',
      status: 'Published',
      latestLiveVersion: {
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-3',
        version: '1.2',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Minor',
      },
      previousReleaseIds: ['previous-release-id'],
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

  test('renders live data set rows correctly', () => {
    render(
      <LiveApiDataSetsTable
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-version-1"
        releaseId="release-1"
      />,
    );

    const baseDataSetUrl =
      '/publication/publication-1/release/release-version-1/api-data-sets';

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(4);

    // Row 1

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0');
    expect(row1Cells[1]).toHaveTextContent('Data set 1 title');

    expect(
      within(row1Cells[2]).getByRole('link', {
        name: 'View details for Data set 1 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-1`);

    expect(
      within(row1Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 1 title',
      }),
    ).toBeInTheDocument();

    // Row 2

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(row2Cells[0]).toHaveTextContent('v1.0');
    expect(row2Cells[1]).toHaveTextContent('Data set 2 title');

    expect(
      within(row2Cells[2]).getByRole('link', {
        name: 'View details for Data set 2 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-2`);

    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 2 title',
      }),
    ).toBeInTheDocument();

    // Row 3

    const row3Cells = within(rows[3]).getAllByRole('cell');

    expect(row3Cells[0]).toHaveTextContent('v1.2');
    expect(row3Cells[1]).toHaveTextContent('Data set 3 title');

    expect(
      within(row3Cells[2]).getByRole('link', {
        name: 'View details for Data set 3 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-3`);

    expect(
      within(row3Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 3 title',
      }),
    ).toBeInTheDocument();
  });

  test("does not render 'Create new version' buttons when release cannot be updated", () => {
    render(
      <LiveApiDataSetsTable
        canUpdateRelease={false}
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-1"
        releaseId="release-id"
      />,
    );

    expect(
      screen.queryAllByRole('button', { name: /Create new version/ }),
    ).toHaveLength(0);
  });

  test("does not render 'Create new version' buttons when release series contains previous version of this data set", () => {
    render(
      <LiveApiDataSetsTable
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-1"
        releaseId="previous-release-id"
      />,
    );

    expect(
      screen.queryAllByRole('button', { name: /Create new version/ }),
    ).toHaveLength(0);
  });

  test('renders message when no data sets', () => {
    render(
      <LiveApiDataSetsTable
        dataSets={[]}
        publicationId="publication-1"
        releaseVersionId="release-1"
        releaseId="release-id"
      />,
    );

    expect(screen.getByText(/No live API data sets/)).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('clicking the create button opens modal form to create API data set version', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);

    const { user } = render(
      <LiveApiDataSetsTable
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-1"
        releaseId="release-id"
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    await user.click(
      within(rows[1]).getByRole('button', {
        name: 'Create new version for Data set 1 title',
      }),
    );

    expect(
      await screen.findByText('Create a new API data set version'),
    ).toBeInTheDocument();

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'Create a new API data set version' }),
    ).toBeInTheDocument();

    expect(modal.getByLabelText('Data set')).toBeInTheDocument();

    expect(
      modal.getByRole('button', { name: 'Confirm new data set version' }),
    ).toBeInTheDocument();
  });

  test('submitting the create version form calls the correct service and redirects to next page', async () => {
    apiDataSetCandidateService.listCandidates.mockResolvedValue(testCandidates);
    apiDataSetVersionService.createVersion.mockResolvedValue({
      id: 'data-set-id',
      title: 'Test title',
      summary: 'Test summary',
      status: 'Draft',
      previousReleaseIds: [],
    });

    const history = createMemoryHistory();

    const { user } = render(
      <LiveApiDataSetsTable
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-version-1"
        releaseId="release-1"
      />,
      { history },
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    await user.click(
      within(rows[1]).getByRole('button', {
        name: 'Create new version for Data set 1 title',
      }),
    );

    expect(
      await screen.findByText('Create a new API data set version'),
    ).toBeInTheDocument();

    await user.selectOptions(
      await screen.findByLabelText('Data set'),
      testCandidates[0].releaseFileId,
    );

    expect(apiDataSetVersionService.createVersion).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Confirm new data set version' }),
    );

    await waitFor(() => {
      expect(apiDataSetVersionService.createVersion).toHaveBeenCalledTimes(1);
      expect(apiDataSetVersionService.createVersion).toHaveBeenCalledWith<
        Parameters<typeof apiDataSetVersionService.createVersion>
      >({
        dataSetId: testDataSets[1].id,
        releaseFileId: testCandidates[0].releaseFileId,
      });
    });

    expect(history.location.pathname).toBe(
      '/publication/publication-1/release/release-version-1/api-data-sets/data-set-1',
    );
  });

  function render(
    ui: ReactNode,
    options?: {
      history: History;
    },
  ) {
    const { history = createMemoryHistory() } = options ?? {};

    return baseRender(<Router history={history}>{ui}</Router>);
  }
});
