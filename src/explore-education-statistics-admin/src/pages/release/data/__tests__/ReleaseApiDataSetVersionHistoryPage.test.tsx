import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetVersionHistoryPage from '@admin/pages/release/data/ReleaseApiDataSetVersionHistoryPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetVersionHistoryRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
  ApiDataSetLiveVersionSummary,
} from '@admin/services/apiDataSetService';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetVersionHistoryPage', () => {
  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Published',
    previousReleaseIds: [],
  };

  const testVersions: ApiDataSetLiveVersionSummary[] = [
    {
      id: 'version-3-1-id',
      file: {
        id: 'version-3-1-file-id',
        title: 'version-3-1-file-title',
      },
      published: '2024-04-01',
      releaseVersion: {
        id: 'release-v3-1-id',
        title: 'Release version 3.1',
      },
      status: 'Published',
      type: 'Minor',
      version: '3.1',
    },
    {
      id: 'version-3-id',
      file: {
        id: 'version-3-file-id',
        title: 'version-3-file-title',
      },
      published: '2024-03-01',
      releaseVersion: {
        id: 'release-v3-id',
        title: 'Release version 3',
      },
      status: 'Published',
      type: 'Major',
      version: '3.0',
    },
    {
      id: 'version-2-id',
      file: {
        id: 'version-2-file-id',
        title: 'version-2-file-title',
      },
      published: '2024-02-01',
      releaseVersion: {
        id: 'release-v2-id',
        title: 'Release version 2',
      },
      status: 'Published',
      type: 'Major',
      version: '2.0',
    },
    {
      id: 'version-1-id',
      file: {
        id: 'version-1-file-id',
        title: 'version-1-file-title',
      },
      published: '2024-01-01',
      releaseVersion: {
        id: 'release-v1-id',
        title: 'Release version 1',
      },
      status: 'Published',
      type: 'Major',
      version: '1.0',
    },
  ];

  test('renders the table correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.listVersions.mockResolvedValueOnce({
      results: testVersions,
      paging: { page: 1, pageSize: 10, totalPages: 1, totalResults: 4 },
    });
    apiDataSetVersionService.listVersions.mockResolvedValueOnce({
      results: testVersions,
      paging: { page: 2, pageSize: 10, totalPages: 1, totalResults: 4 },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const { publicationId } = testRelease;

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(5);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('3.1');
    expect(
      within(row1Cells[1]).getByRole('link', { name: 'Release version 3.1' }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v3-1-id/summary`,
    );
    expect(row1Cells[2]).toHaveTextContent('Published');
    expect(row1Cells[2]).toHaveTextContent('Latest version');
    expect(
      within(row1Cells[3]).getByRole('link', {
        name: 'View changelog for version 3.1',
      }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v3-1-id/api-data-sets/data-set-id/changelog/version-3-1-id`,
    );
    expect(
      within(row1Cells[3]).getByRole('link', {
        name: /View live data set for version 3.1/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/version-3-1-file-id',
    );

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('3.0');
    expect(
      within(row2Cells[1]).getByRole('link', { name: 'Release version 3' }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v3-id/summary`,
    );
    expect(row2Cells[2]).toHaveTextContent('Published');
    expect(
      within(row2Cells[3]).getByRole('link', {
        name: 'View changelog for version 3.0',
      }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v3-id/api-data-sets/data-set-id/changelog/version-3-id`,
    );
    expect(
      within(row2Cells[3]).getByRole('link', {
        name: /View live data set for version 3.0/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/version-3-file-id',
    );

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('2.0');
    expect(
      within(row3Cells[1]).getByRole('link', { name: 'Release version 2' }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v2-id/summary`,
    );
    expect(row3Cells[2]).toHaveTextContent('Published');
    expect(
      within(row3Cells[3]).getByRole('link', {
        name: 'View changelog for version 2.0',
      }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v2-id/api-data-sets/data-set-id/changelog/version-2-id`,
    );
    expect(
      within(row3Cells[3]).getByRole('link', {
        name: /View live data set for version 2.0/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/version-2-file-id',
    );

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('1.0');
    expect(
      within(row4Cells[1]).getByRole('link', { name: 'Release version 1' }),
    ).toHaveAttribute(
      'href',
      `/publication/${publicationId}/release/release-v1-id/summary`,
    );
    expect(row4Cells[2]).toHaveTextContent('Published');
    expect(
      within(row4Cells[3]).queryByRole('link', {
        name: 'View changelog for version 1.0',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('link', {
        name: /View live data set for version 1.0/,
      }),
    ).toHaveAttribute(
      'href',
      'http://localhost/data-catalogue/data-set/version-1-file-id',
    );
  });

  test('pagination works correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.listVersions.mockResolvedValueOnce({
      results: [testVersions[0], testVersions[1]],
      paging: { page: 1, pageSize: 2, totalPages: 2, totalResults: 4 },
    });

    const { user } = renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(3);
    expect(within(rows[1]).getAllByRole('cell')[0]).toHaveTextContent('3.1');
    expect(within(rows[2]).getAllByRole('cell')[0]).toHaveTextContent('3.0');

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();

    apiDataSetVersionService.listVersions.mockResolvedValueOnce({
      results: [testVersions[2], testVersions[3]],
      paging: { page: 2, pageSize: 2, totalPages: 2, totalResults: 4 },
    });

    await user.click(screen.getByRole('link', { name: 'Page 2' }));

    await waitFor(() => {
      expect(apiDataSetVersionService.listVersions).toHaveBeenCalledWith({
        dataSetId: 'data-set-id',
        page: 2,
      });
      expect(screen.queryByText('3.1')).not.toBeInTheDocument();
    });

    const updatedRows = within(screen.getByRole('table')).getAllByRole('row');
    expect(updatedRows).toHaveLength(3);

    expect(within(updatedRows[1]).getAllByRole('cell')[0]).toHaveTextContent(
      '2.0',
    );
    expect(within(updatedRows[2]).getAllByRole('cell')[0]).toHaveTextContent(
      '1.0',
    );
  });

  function renderPage() {
    return render(
      <TestConfigContextProvider>
        <ReleaseVersionContextProvider releaseVersion={testRelease}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetVersionHistoryRoute.path,
                {
                  publicationId: testRelease.publicationId,
                  releaseVersionId: testRelease.id,
                  dataSetId: 'data-set-id',
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetVersionHistoryPage}
              path={releaseApiDataSetVersionHistoryRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
