import baseRender from '@common-test/render';
import {
  testApiDataSetVersion,
  testApiDataSetVersions,
} from '@frontend/modules/data-catalogue/__data__/testDataSets';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import _apiDataSetService from '@frontend/services/apiDataSetService';
import { screen, waitFor, within } from '@testing-library/react';
import { times } from 'lodash';
import { MemoryRouterProvider } from 'next-router-mock/MemoryRouterProvider';
import React, { ReactElement } from 'react';

jest.mock('@frontend/services/apiDataSetService');

const apiDataSetService = jest.mocked(_apiDataSetService);

describe('DataSetFileApiVersionHistory', () => {
  test('renders correctly without pagination', async () => {
    apiDataSetService.listDataSetVersions.mockResolvedValue(
      testApiDataSetVersions,
    );

    render(
      <DataSetFileApiVersionHistory
        apiDataSetId="api-data-set-id"
        currentVersion="2.0"
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'API data set version history' }),
    ).toBeInTheDocument();

    const table = within(await screen.findByRole('table'));

    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('2.0 (current)');
    expect(within(row1Cells[0]).queryByRole('link')).not.toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('Published');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[0]).getByRole('link', { name: '1.2' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/TODO');
    expect(row2Cells[1]).toHaveTextContent('Deprecated');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[0]).getByRole('link', { name: '1.0' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/TODO');
    expect(row3Cells[1]).toHaveTextContent('Withdrawn');

    expect(
      screen.queryByRole('navigation', { name: 'Version history pagination' }),
    ).not.toBeInTheDocument();
  });

  test('renders pagination correctly', async () => {
    apiDataSetService.listDataSetVersions.mockResolvedValue({
      paging: {
        page: 1,
        pageSize: 3,
        totalPages: 2,
        totalResults: 5,
      },
      results: times(3, index => {
        return {
          ...testApiDataSetVersion,
          version: `2.${2 - index}`,
        };
      }),
    });

    render(
      <DataSetFileApiVersionHistory
        apiDataSetId="api-data-set-id"
        currentVersion="2.0"
      />,
    );

    const table = within(await screen.findByRole('table'));

    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const pagination = within(
      screen.getByRole('navigation', { name: 'Version history pagination' }),
    );

    const paginationLinks = pagination.getAllByRole('link');
    expect(paginationLinks).toHaveLength(3);

    expect(pagination.getByRole('link', { name: 'Page 1' })).toHaveAttribute(
      'href',
      '/?versionPage=1',
    );
    expect(pagination.getByRole('link', { name: 'Page 2' })).toHaveAttribute(
      'href',
      '/?versionPage=2',
    );
    expect(pagination.getByRole('link', { name: 'Next page' })).toHaveAttribute(
      'href',
      '/?versionPage=2',
    );
  });

  test('clicking `Next page` link loads next page of versions', async () => {
    apiDataSetService.listDataSetVersions.mockResolvedValue({
      paging: {
        page: 1,
        pageSize: 3,
        totalPages: 2,
        totalResults: 5,
      },
      results: times(3, index => {
        return {
          ...testApiDataSetVersion,
          version: `2.${2 - index}`,
        };
      }),
    });

    const { user } = render(
      <DataSetFileApiVersionHistory
        apiDataSetId="api-data-set-id"
        currentVersion="2.0"
      />,
    );

    let table = within(await screen.findByRole('table'));

    let rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    expect(within(rows[1]).getByRole('link', { name: '2.2' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/TODO',
    );

    expect(within(rows[2]).getByRole('link', { name: '2.1' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/TODO',
    );

    expect(within(rows[3]).getByText('2.0 (current)')).toBeInTheDocument();

    const pagination = within(
      await screen.findByRole('navigation', {
        name: 'Version history pagination',
      }),
    );

    apiDataSetService.listDataSetVersions.mockResolvedValue({
      paging: {
        page: 2,
        pageSize: 3,
        totalPages: 2,
        totalResults: 5,
      },
      results: times(2, index => {
        return {
          ...testApiDataSetVersion,
          version: `1.${1 - index}`,
        };
      }),
    });

    await user.click(pagination.getByRole('link', { name: 'Next page' }));

    await waitFor(() => {
      expect(screen.getByText('1.1')).toBeInTheDocument();
    });

    table = within(screen.getByRole('table'));

    rows = table.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(within(rows[1]).getByRole('link', { name: '1.1' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/TODO',
    );
    expect(within(rows[2]).getByRole('link', { name: '1.0' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/TODO',
    );
  });

  test('renders error message when data set versions cannot be loaded', async () => {
    apiDataSetService.listDataSetVersions.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <DataSetFileApiVersionHistory
        apiDataSetId="api-data-set-id"
        currentVersion="2.0"
      />,
    );

    expect(
      await screen.findByText('Could not load version history'),
    ).toBeInTheDocument();
  });

  function render(ui: ReactElement) {
    return baseRender(<MemoryRouterProvider>{ui}</MemoryRouterProvider>);
  }
});
