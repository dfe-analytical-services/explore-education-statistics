import flushPromises from '@common-test/flushPromises';
import render from '@common-test/render';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = jest.mocked(_tableBuilderService);

describe('KeyStatDataBlock', () => {
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

  test('renders correctly with all props provided', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <KeyStatDataBlock
        releaseVersionId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle="What is the number of applications received?"
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    await screen.findByRole('button', {
      name: 'What is the number of applications received?',
    });

    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(1);

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'Number of applications received',
    );

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );

    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
      'Down from 620,330 in 2017',
    );

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
        releaseVersionId="release-1"
        dataBlockId="block-1"
        trend="Down from 620,330 in 2017"
        guidanceTitle={undefined}
        guidanceText="Total number of applications received for places at primary and secondary schools."
      />,
    );

    expect(
      await screen.findByRole('button', {
        name: 'Help for Number of applications received',
      }),
    ).toBeInTheDocument();

    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(1);

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'Number of applications received',
    );

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );

    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
      'Down from 620,330 in 2017',
    );

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
        releaseVersionId="release-1"
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
        releaseVersionId="release-1"
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
        releaseVersionId="release-1"
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

  // As the frontend does when the key stat was fetched server-side.
  describe('with a pre-populated query cache', () => {
    function createPopulatedQueryClient() {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      queryClient.setQueryData(
        tableBuilderQueries.getKeyStat('release-1', 'block-1').queryKey,
        { title: 'Number of applications received', value: '608,180' },
      );

      return queryClient;
    }

    test('server-renders the key stat', () => {
      const container = document.createElement('div');
      container.innerHTML = renderToStaticMarkup(
        <QueryClientProvider client={createPopulatedQueryClient()}>
          <KeyStatDataBlock
            releaseVersionId="release-1"
            dataBlockId="block-1"
            guidanceTitle="What is the number of applications received?"
            guidanceText="Total number of applications received."
          />
        </QueryClientProvider>,
      );

      expect(within(container).getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );
      expect(
        within(container).getByTestId('keyStat-statistic'),
      ).toHaveTextContent('608,180');
      expect(
        within(container).getByTestId('keyStat-guidanceText'),
      ).toHaveTextContent('Total number of applications received.');

      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    });

    test('does not refetch when `dataBlockStaleTime` is Infinity', async () => {
      baseRender(
        <QueryClientProvider client={createPopulatedQueryClient()}>
          <KeyStatDataBlock
            releaseVersionId="release-1"
            dataBlockId="block-1"
            dataBlockStaleTime={Infinity}
          />
        </QueryClientProvider>,
      );

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      await flushPromises();

      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    });

    test('refetches on mount by default', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      baseRender(
        <QueryClientProvider client={createPopulatedQueryClient()}>
          <KeyStatDataBlock
            releaseVersionId="release-1"
            dataBlockId="block-1"
          />
        </QueryClientProvider>,
      );

      await waitFor(() => {
        expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
          1,
        );
      });
    });
  });
});
