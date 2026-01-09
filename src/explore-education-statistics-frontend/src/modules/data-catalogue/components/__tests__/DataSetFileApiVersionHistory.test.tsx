import baseRender from '@common-test/render';
import {
  testApiDataSetVersion,
  testApiDataSetVersions,
  testPatchApiDataSetVersions,
} from '@frontend/modules/data-catalogue/__data__/testDataSets';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import _apiDataSetService from '@frontend/services/apiDataSetService';
import { screen, within } from '@testing-library/react';
import { times } from 'lodash';
import { MemoryRouterProvider } from 'next-router-mock/MemoryRouterProvider';
import React, { ReactNode } from 'react';

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

    expect(await screen.findByText('Version')).toBeInTheDocument();

    const table = within(screen.getByRole('table'));

    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('2.0 (current)');
    expect(within(row1Cells[0]).queryByRole('link')).not.toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('Release 1 title');
    expect(row1Cells[2]).toHaveTextContent('Published');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[0]).getByRole('link', { name: '1.2' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/file-2-id');
    expect(row2Cells[1]).toHaveTextContent('Release 2 title');
    expect(row2Cells[2]).toHaveTextContent('Deprecated');

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[0]).getByRole('link', { name: '1.0' }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/file-3-id');
    expect(row3Cells[1]).toHaveTextContent('Release 3 title');
    expect(row3Cells[2]).toHaveTextContent('Withdrawn');

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

    expect(await screen.findByText('Version')).toBeInTheDocument();

    const table = within(screen.getByRole('table'));

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
        const i = index + 1;

        return {
          ...testApiDataSetVersion,
          version: `2.${2 - index}`,
          file: {
            id: `file-${i}-id`,
          },
          release: {
            title: `Release ${i} title`,
            slug: `release-${i}-slug`,
          },
        };
      }),
    });

    const { user } = render(
      <DataSetFileApiVersionHistory
        apiDataSetId="api-data-set-id"
        currentVersion="2.0"
      />,
    );

    expect(await screen.findByText('Version')).toBeInTheDocument();

    let table = within(screen.getByRole('table'));

    let rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    expect(within(rows[1]).getByRole('link', { name: '2.2' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/file-1-id',
    );
    expect(within(rows[1]).getByText('Release 1 title')).toBeInTheDocument();

    expect(within(rows[2]).getByRole('link', { name: '2.1' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/file-2-id',
    );
    expect(within(rows[2]).getByText('Release 2 title')).toBeInTheDocument();

    expect(within(rows[3]).getByText('2.0 (current)')).toBeInTheDocument();
    expect(within(rows[3]).getByText('Release 3 title')).toBeInTheDocument();

    const pagination = within(
      screen.getByRole('navigation', {
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
        const i = index + 3;

        return {
          ...testApiDataSetVersion,
          version: `1.${1 - index}`,
          file: {
            id: `file-${i}-id`,
          },
          release: {
            title: `Release ${i} title`,
            slug: `release-${i}-slug`,
          },
        };
      }),
    });

    await user.click(pagination.getByRole('link', { name: 'Next page' }));

    expect(await screen.findByText('1.1')).toBeInTheDocument();

    table = within(screen.getByRole('table'));

    rows = table.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(within(rows[1]).getByRole('link', { name: '1.1' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/file-3-id',
    );
    expect(within(rows[1]).getByText('Release 3 title')).toBeInTheDocument();

    expect(within(rows[2]).getByRole('link', { name: '1.0' })).toHaveAttribute(
      'href',
      '/data-catalogue/data-set/file-4-id',
    );
    expect(within(rows[2]).getByText('Release 4 title')).toBeInTheDocument();
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

  test('renders latest patch version as the current', async () => {
    apiDataSetService.listDataSetVersions.mockResolvedValue(
      testPatchApiDataSetVersions,
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

    expect(await screen.findByText('Version')).toBeInTheDocument();

    const table = within(screen.getByRole('table'));

    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    const row2Cells = within(rows[2]).getAllByRole('cell');
    const row3Cells = within(rows[3]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('2.0.2 (current)');
    expect(within(row1Cells[0]).queryByRole('link')).not.toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('Release 1 title');
    expect(row1Cells[2]).toHaveTextContent('Published');

    expect(row2Cells[0]).toHaveTextContent('1.1');
    expect(within(row2Cells[0]).queryByRole('link')).toBeInTheDocument();
    expect(row2Cells[1]).toHaveTextContent('Release 2 title');
    expect(row2Cells[2]).toHaveTextContent('Deprecated');

    expect(row3Cells[0]).toHaveTextContent('1.0');
    expect(within(row3Cells[0]).queryByRole('link')).toBeInTheDocument();
    expect(row3Cells[1]).toHaveTextContent('Release 3 title');
    expect(row3Cells[2]).toHaveTextContent('Withdrawn');
  });

  function render(ui: ReactNode) {
    return baseRender(<MemoryRouterProvider>{ui}</MemoryRouterProvider>);
  }
});
