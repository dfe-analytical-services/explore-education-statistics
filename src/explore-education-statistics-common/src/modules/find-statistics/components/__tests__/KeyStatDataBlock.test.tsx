import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import React from 'react';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('KeyStatDataBlock', () => {
  const testTableDataResponse: TableDataResponse = {
    subjectMeta: {
      publicationName: 'Test publication',
      subjectName: 'Test subject',
      geoJsonAvailable: false,
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

  test('renders correctly with all props provided', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <KeyStatDataBlock
        releaseId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle="What is the number of applications received?"
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );
    });

    expect(
      screen.getByRole('button', {
        name: 'What is the number of applications received?',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Total number of applications received for places at primary and secondary schools.',
    );
  });

  test('renders correctly with a blank guidanceTitle', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <KeyStatDataBlock
        releaseId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle={undefined}
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );
    });

    expect(
      screen.getByRole('button', {
        name: 'Help for Number of applications received',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'Total number of applications received for places at primary and secondary schools.',
    );
  });

  test('renders correctly without trend or guidance text', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <KeyStatDataBlock
        releaseId="release-1"
        dataBlockId="block-1"
        guidanceTitle="This shouldn't appear"
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();
    expect(screen.queryByText("This shouldn't appear")).not.toBeInTheDocument();
  });

  test('does not render if there was an error fetching the table data', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <KeyStatDataBlock
        releaseId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle="What is the number of applications received?"
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-statistic')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();
  });

  test('does not render if there is no matching result in the response', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue({
      ...testTableDataResponse,
      subjectMeta: {
        ...testTableDataResponse.subjectMeta,
        indicators: [
          {
            label: 'Number of applications received',
            name: 'applications_received',
            unit: '',
            value: 'indicator-1',
          },
        ],
      },
      results: [],
    });

    render(
      <KeyStatDataBlock
        releaseId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle="What is the number of applications received?"
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-statistic')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();
  });
});
