import render from '@common-test/render';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import TableToolSearchFinalResult from '@frontend/modules/table-tool/components/TableToolSearchFinalResult';
import { screen } from '@testing-library/react';
import React from 'react';
import { testFinalResult } from './__data__/tableData';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = jest.mocked(_tableBuilderService);

describe('TableToolSearchFinalResult', () => {
  const testTableDataResponse: TableDataResponse = {
    subjectMeta: {
      publicationName: 'Test publication',
      subjectName: 'Test subject',
      dataSetFileId: 'file-id',
      geoJsonAvailable: false,
      isCroppedTable: false,
      filters: {
        Filter1: {
          legend: 'Filter 1',
          name: 'filter1',
          options: {
            FilterGroup1: {
              id: 'filter-group-1',
              label: 'Filter group 1',
              options: [
                {
                  label: 'Filter 1',
                  value: 'filter-1',
                },
              ],
              order: 0,
            },
          },
          order: 0,
        },
      },
      locations: {
        country: [
          {
            id: 'england-id',
            label: 'England',
            value: 'england',
          },
        ],
      },
      timePeriodRange: [{ code: 'AY', label: '2020/21', year: 2020 }],
      indicators: [
        {
          label: 'Number of applications received',
          name: 'applications_received',
          unit: '',
          value: 'indicator-1',
        },
      ],
      boundaryLevels: [],
      footnotes: [],
    },
    results: [
      {
        filters: ['filter-1'],
        geographicLevel: 'country',
        locationId: 'england-id',
        timePeriod: '2020_AY',
        measures: {
          'indicator-1': '608180',
        },
      },
    ],
  };

  test('renders dataset details correctly', async () => {
    tableBuilderService.getTableData.mockResolvedValue(testTableDataResponse);

    render(
      <TableToolSearchFinalResult
        releaseVersionId="test-release-version-id"
        dataset={testFinalResult}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test dataset title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'View this data set' }),
    ).toHaveAttribute('href', '/data-catalogue/test-file-id');

    expect(
      screen.getByRole('heading', { name: 'Relevance' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Test AI relevance summary explanation.'),
    ).toBeInTheDocument();
  });

  test('renders table when table query resolves successfully', async () => {
    tableBuilderService.getTableData.mockResolvedValue(testTableDataResponse);

    render(
      <TableToolSearchFinalResult
        releaseVersionId="test-release-version-id"
        dataset={testFinalResult}
      />,
    );

    expect(await screen.findByRole('table')).toBeInTheDocument();
    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      /Indicator 1/,
    );
    expect(screen.getByText('Filter 1')).toBeInTheDocument();
    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(/England/);
  });

  test('renders error message when table query fails', async () => {
    tableBuilderService.getTableData.mockRejectedValue(new Error('API error'));

    render(
      <TableToolSearchFinalResult
        releaseVersionId="test-release-version-id"
        dataset={testFinalResult}
      />,
    );

    expect(await screen.findByText('Error loading table.')).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
});
