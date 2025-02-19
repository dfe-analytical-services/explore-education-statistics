import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetPreviewTokenLogPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenLogPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetPreviewTokenLogRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import _previewTokenService, {
  PreviewToken,
} from '@admin/services/previewTokenService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/previewTokenService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const previewTokenService = jest.mocked(_previewTokenService);

describe('ReleaseApiDataSetPreviewTokenLogPage', () => {
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
  const testTokens: PreviewToken[] = [
    {
      id: 'token-id-1',
      label: 'Test label 1',
      status: 'Active',
      created: '2024-01-01T14:00Z',
      createdByEmail: 'test-1@hiveit.co.uk',
      updated: '',
      expiry: '2024-01-02T14:00Z',
    },
    {
      id: 'token-id-2',
      label: 'Test label 2',
      status: 'Expired',
      created: '2024-02-01T10:00Z',
      createdByEmail: 'test-2@hiveit.co.uk',
      updated: '',
      expiry: '2024-02-02T10:00Z',
    },
    {
      id: 'token-id-3',
      label: 'Test label 3',
      status: 'Active',
      created: '2024-03-01T10:00Z',
      createdByEmail: 'test-3@hiveit.co.uk',
      updated: '',
      expiry: '2024-03-02T10:00Z',
    },
  ];

  test('renders correctly with tokens', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.listPreviewTokens.mockResolvedValue(testTokens);

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Test label 1');
    expect(row1Cells[1]).toHaveTextContent('test-1@hiveit.co.uk');
    expect(row1Cells[2]).toHaveTextContent('1 January 2024, 14:00');
    expect(row1Cells[3]).toHaveTextContent('Active');
    expect(row1Cells[4]).toHaveTextContent('2 January 2024, 14:00');
    expect(
      within(row1Cells[5]).getByRole('link', {
        name: 'View details for Test label 1',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview-tokens/token-id-1',
    );
    expect(
      within(row1Cells[5]).getByRole('button', { name: 'Revoke Test label 1' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Test label 2');
    expect(row2Cells[1]).toHaveTextContent('test-2@hiveit.co.uk');
    expect(row2Cells[2]).toHaveTextContent('1 February 2024, 10:00');
    expect(row2Cells[3]).toHaveTextContent('Expired');
    expect(row2Cells[4]).toHaveTextContent('2 February 2024, 10:00');
    expect(
      within(row2Cells[5]).queryByRole('link', {
        name: 'View details for Test label 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row2Cells[5]).queryByRole('button', {
        name: 'Revoke Test label 2',
      }),
    ).not.toBeInTheDocument();

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Test label 3');
    expect(row3Cells[1]).toHaveTextContent('test-3@hiveit.co.uk');
    expect(row3Cells[2]).toHaveTextContent('1 March 2024, 10:00');
    expect(row3Cells[3]).toHaveTextContent('Active');
    expect(row3Cells[4]).toHaveTextContent('2 March 2024, 10:00');
    expect(
      within(row3Cells[5]).getByRole('link', {
        name: 'View details for Test label 3',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview-tokens/token-id-3',
    );
    expect(
      within(row3Cells[5]).getByRole('button', { name: 'Revoke Test label 3' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('No preview tokens have been created.'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Generate preview token' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview',
    );
  });

  test('renders correctly with no tokens', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.listPreviewTokens.mockResolvedValue([]);

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(screen.queryByRole('table')).not.toBeInTheDocument();

    expect(
      screen.getByText('No preview tokens have been created.'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Generate preview token' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/api-data-sets/data-set-id/preview',
    );
  });

  test('revoking a token', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    previewTokenService.listPreviewTokens.mockResolvedValueOnce(testTokens);
    previewTokenService.listPreviewTokens.mockResolvedValueOnce(
      testTokens.map(token => {
        return token.id === 'token-id-1'
          ? { ...token, status: 'Expired' }
          : token;
      }),
    );

    const { user } = renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(4);

    await user.click(
      within(rows[1]).getByRole('button', { name: 'Revoke Test label 1' }),
    );

    expect(
      await screen.findByText(
        'Are you sure you want to revoke this preview token?',
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
      'token-id-1',
    );

    expect(
      within(rows[1]).queryByRole('button', { name: 'Revoke Test label 1' }),
    ).not.toBeInTheDocument();
    expect(within(rows[1]).getAllByRole('cell')[3]).toHaveTextContent(
      'Expired',
    );
  });

  function renderPage() {
    return render(
      <TestConfigContextProvider>
        <ReleaseVersionContextProvider releaseVersion={testRelease}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetPreviewTokenLogRoute.path,
                {
                  publicationId: testRelease.publicationId,
                  releaseVersionId: testRelease.id,
                  dataSetId: 'data-set-id',
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetPreviewTokenLogPage}
              path={releaseApiDataSetPreviewTokenLogRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
