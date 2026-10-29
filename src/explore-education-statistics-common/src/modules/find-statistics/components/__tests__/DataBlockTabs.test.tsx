import render from '@common-test/render';
import TabsSection from '@common/components/TabsSection';
import {
  testChartConfiguration,
  testChartTableData,
} from '@common/modules/charts/components/__tests__/__data__/testChartData';
import {
  testMapConfiguration,
  testMapTableData,
  testMapTableDataLocationsLowRes,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import { Chart } from '@common/modules/charts/types/chart';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import KeyStatDataBlock from '@common/modules/find-statistics/components/KeyStatDataBlock';
import logger from '@common/services/logger';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { DataBlock } from '@common/services/types/blocks';
import { screen, waitFor, within } from '@testing-library/dom';
import userEvent from '@testing-library/user-event';
import {
  dehydrate,
  Hydrate,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query';
import { AxiosError } from 'axios';
import noop from 'lodash/noop';
import { forceVisible } from 'react-lazyload';
import React, { ReactElement, ReactNode } from 'react';
import { renderToStaticMarkup, renderToString } from 'react-dom/server';
import { act, render as baseRender } from '@testing-library/react';

jest.mock('@common/services/tableBuilderService');

jest.mock('recharts/lib/util/LogUtils');

// Responsive charts don't render in tests so have do
// mock the responsive container and apply a fixed width.
jest.mock('recharts', () => {
  const OriginalModule = jest.requireActual('recharts');
  return {
    ...OriginalModule,
    ResponsiveContainer: ({
      children,
      height,
    }: {
      children: ReactNode;
      height: string;
    }) => (
      <OriginalModule.ResponsiveContainer width={800} height={height}>
        {children}
      </OriginalModule.ResponsiveContainer>
    ),
  };
});

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataBlockTabs', () => {
  const testDataBlock: DataBlock = {
    id: 'block-1',
    dataBlockId: 'block-1',
    dataSetName: 'Test data set',
    dataSetId: 'test-data-set',
    type: 'DataBlockVersionLink',
    heading: '',
    order: 0,
    name: 'Test data block',
    source: '',
    query: {
      subjectId: '1',
      timePeriod: {
        startYear: 2012,
        startCode: 'AY',
        endYear: 2016,
        endCode: 'AY',
      },
      filters: ['characteristic-total', 'school-type-total'],
      indicators: [
        'authorised-absence-rate',
        'unauthorised-absence-rate',
        'overall-absence-rate',
      ],
      locationIds: ['england'],
    },
    charts: [],
    table: {
      tableHeaders: {
        columnGroups: [],
        columns: [
          { value: '2012_AY', type: 'TimePeriod' },
          { value: '2013_AY', type: 'TimePeriod' },
          { value: '2014_AY', type: 'TimePeriod' },
          { value: '2015_AY', type: 'TimePeriod' },
          { value: '2016_AY', type: 'TimePeriod' },
        ],
        rowGroups: [],
        rows: [
          {
            value: 'authorised-absence-rate',
            type: 'Indicator',
          },
          {
            value: 'unauthorised-absence-rate',
            type: 'Indicator',
          },
          {
            value: 'overall-absence-rate',
            type: 'Indicator',
          },
        ],
      },
    },
  };

  const testDataBlockMap: DataBlock = {
    id: 'block-1',
    dataBlockId: 'block-1',
    dataSetId: 'test-data-set',
    dataSetName: 'Test data set',
    type: 'DataBlockVersionLink',
    heading: '',
    order: 0,
    name: 'Test data block',
    source: '',
    query: {
      subjectId: '1',
      timePeriod: {
        startYear: 2016,
        startCode: 'AY',
        endYear: 2016,
        endCode: 'AY',
      },
      filters: ['characteristic-total', 'school-type-total'],
      indicators: [
        'authorised-absence-rate',
        'unauthorised-absence-rate',
        'overall-absence-rate',
      ],
      locationIds: ['england'],
    },
    charts: [testMapConfiguration],
    table: {
      tableHeaders: {
        columnGroups: [],
        columns: [{ value: '2016_AY', type: 'TimePeriod' }],
        rowGroups: [],
        rows: [
          {
            value: 'authorised-absence-rate',
            type: 'Indicator',
          },
          {
            value: 'unauthorised-absence-rate',
            type: 'Indicator',
          },
          {
            value: 'overall-absence-rate',
            type: 'Indicator',
          },
        ],
      },
    },
  };

  test('renders error message if table response is error', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue({
      isAxiosError: true,
      message: 'Something went wrong',
      response: {
        status: 500,
      },
    } as AxiosError);

    render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getByText('Could not load content')).toBeInTheDocument();
    });
  });

  test('renders nothing if table response is 403', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue({
      name: 'Error',
      isAxiosError: true,
      message: 'Forbidden',
      response: {
        status: 403,
      },
    } as AxiosError);

    render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.queryByText('Could not load content')).toBeInTheDocument();
      expect(screen.queryByRole('table')).not.toBeInTheDocument();
    });
  });

  test('renders line chart', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const { container } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getAllByRole('tab')).toHaveLength(2);

      expect(container.querySelectorAll('.recharts-line')).toHaveLength(3);
    });
  });

  test('renders horizontal chart', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const { container } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          charts: [
            {
              ...testChartConfiguration,
              type: 'horizontalbar',
            } as Chart,
          ],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getAllByRole('tab')).toHaveLength(2);
      expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    });
  });

  test('renders vertical chart', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const { container } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          charts: [
            {
              ...testChartConfiguration,
              type: 'verticalbar',
            } as Chart,
          ],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getAllByRole('tab')).toHaveLength(2);
      expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    });
  });

  test('renders table', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const fullTable = mapFullTable(testChartTableData);

    render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          table: {
            tableHeaders: mapUnmappedTableHeaders(
              getDefaultTableHeaderConfig(fullTable),
            ),
          },
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getByRole('table')).toBeInTheDocument();
      expect(screen.getAllByRole('row')).toHaveLength(4);
      expect(screen.getAllByRole('cell')).toHaveLength(16);
    });
  });

  test('renders map', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testMapTableData,
    );
    tableBuilderService.getDataBlockGeoJson.mockResolvedValue(
      testMapTableData.subjectMeta.locations,
    );

    const { container } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-block"
        dataBlock={testDataBlockMap}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
        'release-1',
        'block-1',
        1,
      );

      expect(container.querySelector('.leaflet-container')).toBeInTheDocument();
    });
    await waitFor(() => {
      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalled();
      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledTimes(1);
    });
  });

  test('renders chart and table export menus', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const { user } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getAllByRole('tab')).toHaveLength(2);
    });

    const exportButtons = screen.getAllByTestId(
      'Expand Details Section Export options',
    );
    expect(exportButtons).toHaveLength(2);

    expect(exportButtons[0].textContent).toMatch(
      /Export options\s+for Aggregated results chart/,
    );
    await user.click(screen.getByRole('tab', { name: /Table/ }));

    expect(exportButtons[1].textContent).toMatch(/Export options\s+for table/);
  });

  test('selecting data set with boundaryLevel retrieves and renders new map polygons', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testMapTableData,
    );
    tableBuilderService.getDataBlockGeoJson
      .mockResolvedValueOnce(testMapTableData.subjectMeta.locations)
      .mockResolvedValue(testMapTableDataLocationsLowRes);

    const { container } = render(
      <DataBlockTabs
        releaseVersionId="release-1"
        id="test-block"
        dataBlock={testDataBlockMap}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledWith(
        'release-1',
        'block-1',
      );

      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
        'release-1',
        'block-1',
        1,
      );

      expect(container.querySelector('.leaflet-container')).toBeInTheDocument();
    });

    expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledTimes(1);
    });

    const mapPathSelector =
      '.leaflet-container svg:not(.leaflet-attribution-flag) path';

    const initialPaths =
      container.querySelectorAll<HTMLElement>(mapPathSelector);
    expect(initialPaths.length).toEqual(4);

    const select = screen.getByLabelText('1. Select data to view');
    const options = within(select).getAllByRole('option');
    expect(options).toHaveLength(2);
    await userEvent.selectOptions(select, options[1]);

    expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
      'release-1',
      'block-1',
      2,
    );
    expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledTimes(2);

    const updatedPaths =
      container.querySelectorAll<HTMLElement>(mapPathSelector);
    expect(updatedPaths.length).toEqual(4);
  });

  test('re-rendering with new data block does not throw error', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const fullTable = mapFullTable(testChartTableData);

    const { rerender } = render(
      <DataBlockTabs
        id="test-block"
        releaseVersionId="release-1"
        dataBlock={{
          ...testDataBlock,
          table: {
            tableHeaders: mapUnmappedTableHeaders(
              getDefaultTableHeaderConfig(fullTable),
            ),
          },
        }}
      />,
    );

    await act(() => {
      forceVisible();
    });

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
      expect(screen.getAllByRole('row')).toHaveLength(4);
      expect(screen.getAllByRole('cell')).toHaveLength(16);
    });

    tableBuilderService.getDataBlockTableData.mockResolvedValue({
      subjectMeta: {
        ...testChartTableData.subjectMeta,
        indicators: [
          {
            label: 'Number of authorised absence sessions',
            unit: '',
            value: 'authorised-absence-sessions',
            name: 'absence_sess',
          },
        ],
        timePeriodRange: [{ code: 'AY', label: '2018/19', year: 2018 }],
      },
      results: [
        {
          filters: ['characteristic-total', 'school-type-total'],
          geographicLevel: 'country',
          locationId: 'england',
          measures: {
            'authorised-absence-sessions': '500000',
          },
          timePeriod: '2018_AY',
        },
      ],
    });

    expect(() => {
      rerender(
        <DataBlockTabs
          id="test-block"
          releaseVersionId="release-1"
          dataBlock={{
            ...testDataBlock,
            id: 'block-2-id',
            dataBlockId: 'block-2-id',
            query: {
              subjectId: '1',
              timePeriod: {
                startYear: 2018,
                startCode: 'AY',
                endYear: 2018,
                endCode: 'AY',
              },
              filters: ['characteristic-total', 'school-type-total'],
              indicators: ['authorised-absence-sessions'],
              locationIds: ['england'],
            },
            table: {
              tableHeaders: {
                columnGroups: [],
                rowGroups: [],
                columns: [
                  {
                    type: 'TimePeriod',
                    value: '2018_AY',
                  },
                ],
                rows: [
                  {
                    type: 'Indicator',
                    value: 'authorised-absence-sessions',
                  },
                ],
              },
            },
          }}
        />,
      );
    }).not.toThrow();

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
      expect(screen.getAllByRole('row')).toHaveLength(2);
      expect(screen.getAllByRole('cell')).toHaveLength(2);
    });
  });

  // As on the release page, when the data was fetched server-side.
  describe('with table data already in the query cache', () => {
    const testTableDataBlock: DataBlock = {
      ...testDataBlock,
      charts: [testChartConfiguration],
      table: {
        tableHeaders: mapUnmappedTableHeaders(
          getDefaultTableHeaderConfig(mapFullTable(testChartTableData)),
        ),
      },
    };

    function createQueryClient(tableData?: TableDataResponse) {
      const queryClient = new QueryClient({
        logger: { log: noop, warn: noop, error: noop },
        defaultOptions: { queries: { retry: false } },
      });

      if (tableData) {
        queryClient.setQueryData(
          tableBuilderQueries.getDataBlockTable('release-1', 'block-1')
            .queryKey,
          tableData,
        );
      }

      return queryClient;
    }

    function renderServerMarkup(ui: ReactElement, queryClient: QueryClient) {
      const container = document.createElement('div');
      container.innerHTML = renderToStaticMarkup(
        <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
      );
      return container;
    }

    function hydrateServerMarkup(ui: ReactElement, queryClient: QueryClient) {
      const state = dehydrate(queryClient);
      const container = document.createElement('div');
      container.innerHTML = renderToString(
        <QueryClientProvider client={queryClient}>
          <Hydrate state={state}>{ui}</Hydrate>
        </QueryClientProvider>,
      );
      document.body.appendChild(container);
      const serverTable = container.querySelector('table');
      const onRecoverableError = jest.fn();
      const client = createQueryClient();

      const result = render(ui, {
        container,
        hydrate: true,
        onRecoverableError,
        wrapper: ({ children }) => (
          <QueryClientProvider client={client}>
            <Hydrate state={state}>{children}</Hydrate>
          </QueryClientProvider>
        ),
      });

      return { ...result, serverTable, onRecoverableError };
    }

    test('preserves summary and neighbouring tables when configured headers cannot be mapped', () => {
      const logError = jest.spyOn(logger, 'error');
      try {
        const container = renderServerMarkup(
          <>
            <p>Release prose</p>
            <DataBlockTabs
              releaseVersionId="release-1"
              id="broken-block"
              dataBlock={{
                ...testTableDataBlock,
                charts: [],
                table: {
                  tableHeaders: {
                    columns: [],
                    columnGroups: [],
                    rows: [{ type: 'Indicator', value: 'missing-indicator' }],
                    rowGroups: [],
                  },
                },
              }}
              firstTabs={
                <TabsSection title="Summary">
                  <p>Summary content</p>
                </TabsSection>
              }
            />
            <DataBlockTabs
              releaseVersionId="release-1"
              id="valid-block"
              dataBlock={{ ...testTableDataBlock, charts: [] }}
            />
          </>,
          createQueryClient(testChartTableData),
        );

        expect(container).toHaveTextContent('Release prose');
        expect(container).toHaveTextContent('Summary content');
        expect(container).toHaveTextContent('Could not load content');
        expect(within(container).getAllByRole('table')).toHaveLength(1);
        expect(logError).toHaveBeenCalledWith(
          new Error(
            'Could not find matching filter for header: {"type":"Indicator","value":"missing-indicator"}',
          ),
        );
      } finally {
        logError.mockRestore();
      }
    });

    test('preserves independent tabs when table metadata cannot be mapped', () => {
      const logError = jest.spyOn(logger, 'error');
      try {
        const container = renderServerMarkup(
          <DataBlockTabs
            releaseVersionId="release-1"
            dataBlock={testTableDataBlock}
            firstTabs={
              <TabsSection title="Summary">
                <p>Summary content</p>
              </TabsSection>
            }
            lastTabs={
              <TabsSection title="Notes">
                <p>Independent notes</p>
              </TabsSection>
            }
          />,
          createQueryClient({
            ...testChartTableData,
            subjectMeta: { ...testChartTableData.subjectMeta, filters: null! },
          }),
        );

        expect(container).toHaveTextContent('Summary content');
        expect(container).toHaveTextContent('Independent notes');
        expect(container).toHaveTextContent('Could not load content');
        expect(container.querySelector('table')).toBeNull();
        expect(logError).toHaveBeenCalledWith(expect.any(TypeError));
      } finally {
        logError.mockRestore();
      }
    });

    test('hydrates prefetched statistics and tables without replacing or refetching them', () => {
      const queryClient = createQueryClient(testChartTableData);
      queryClient.setQueryData(
        tableBuilderQueries.getKeyStat('release-1', 'stat-1').queryKey,
        { title: 'Statistic title', value: '1,234' },
      );
      const { container, serverTable, onRecoverableError } =
        hydrateServerMarkup(
          <DataBlockTabs
            releaseVersionId="release-1"
            id="test-block"
            dataBlock={testTableDataBlock}
            dataBlockStaleTime={Infinity}
            firstTabs={
              <TabsSection title="Summary">
                <KeyStatDataBlock
                  releaseVersionId="release-1"
                  dataBlockId="stat-1"
                  dataBlockStaleTime={Infinity}
                  guidanceText="Statistic guidance"
                />
              </TabsSection>
            }
          />,
          queryClient,
        );

      expect(within(container).getByRole('table', { hidden: true })).toBe(
        serverTable,
      );
      expect(container).toHaveTextContent('Statistic title');
      expect(container).toHaveTextContent('1,234');
      expect(container).toHaveTextContent('Statistic guidance');
      expect(onRecoverableError).not.toHaveBeenCalled();
      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    });

    test('hydrates a map table without replacing it while GeoJSON loads', () => {
      tableBuilderService.getDataBlockGeoJson.mockReturnValue(
        new Promise(noop),
      );
      const { container, serverTable, onRecoverableError } =
        hydrateServerMarkup(
          <DataBlockTabs
            releaseVersionId="release-1"
            id="test-block"
            dataBlock={testDataBlockMap}
            dataBlockStaleTime={Infinity}
          />,
          createQueryClient(testMapTableData),
        );

      expect(within(container).getByRole('table', { hidden: true })).toBe(
        serverTable,
      );
      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledTimes(1);
      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
      expect(onRecoverableError).not.toHaveBeenCalled();
    });

    test('hydrates the same fallback when cached table headers are invalid', () => {
      const logError = jest.spyOn(logger, 'error');
      try {
        const { container, onRecoverableError } = hydrateServerMarkup(
          <DataBlockTabs
            releaseVersionId="release-1"
            dataBlock={testDataBlock}
            dataBlockStaleTime={Infinity}
            firstTabs={
              <TabsSection title="Summary">
                <p>Summary content</p>
              </TabsSection>
            }
          />,
          createQueryClient({
            ...testChartTableData,
            subjectMeta: { ...testChartTableData.subjectMeta, indicators: [] },
          }),
        );

        expect(container).toHaveTextContent('Summary content');
        expect(container).toHaveTextContent('Could not load content');
        expect(onRecoverableError).not.toHaveBeenCalled();
        expect(logError).toHaveBeenCalled();
      } finally {
        logError.mockRestore();
      }
    });

    test('hydrates independent summary content when prefetch supplied no table data', async () => {
      tableBuilderService.getDataBlockTableData.mockRejectedValue(
        new Error('API unavailable'),
      );
      const { container, onRecoverableError } = hydrateServerMarkup(
        <DataBlockTabs
          releaseVersionId="release-1"
          dataBlock={testDataBlock}
          dataBlockStaleTime={Infinity}
          lazy={false}
          firstTabs={
            <TabsSection title="Summary">
              <p>Summary content</p>
            </TabsSection>
          }
        />,
        createQueryClient(),
      );

      expect(container).toHaveTextContent('Summary content');
      expect(
        await within(container).findByText('Could not load content'),
      ).toBeInTheDocument();
      expect(container).toHaveTextContent('Summary content');
      expect(onRecoverableError).not.toHaveBeenCalled();
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
    });

    test('server-renders the chart and table', () => {
      const container = renderServerMarkup(
        <DataBlockTabs
          releaseVersionId="release-1"
          id="test-block"
          dataBlock={testTableDataBlock}
        />,
        createQueryClient(testChartTableData),
      );

      const tabs = within(container).getAllByRole('link');
      expect(tabs).toHaveLength(2);
      expect(tabs[0]).toHaveTextContent('Chart');
      expect(tabs[1]).toHaveTextContent('Table');

      const chartSection = container.querySelector('#test-block-charts');
      expect(chartSection).not.toHaveAttribute('hidden');
      expect(chartSection).toHaveTextContent('Aggregated results chart');

      const tableSection = container.querySelector(
        '#test-block-tables',
      ) as HTMLElement;
      expect(tableSection).not.toHaveAttribute('hidden');
      expect(within(tableSection).getAllByRole('table')).toHaveLength(1);
      expect(within(tableSection).getAllByRole('cell')).toHaveLength(16);

      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    });

    test('renders the chart and table on mount without fetching them', async () => {
      baseRender(
        <QueryClientProvider client={createQueryClient(testChartTableData)}>
          <DataBlockTabs
            releaseVersionId="release-1"
            id="test-block"
            dataBlock={testTableDataBlock}
            dataBlockStaleTime={Infinity}
          />
        </QueryClientProvider>,
      );

      expect(screen.getByRole('table', { hidden: true })).toBeInTheDocument();
      expect(screen.getByText('Aggregated results chart')).toBeInTheDocument();

      await act(async () => {});

      expect(tableBuilderService.getDataBlockTableData).not.toHaveBeenCalled();
    });

    test('renders the table while the map geoJSON is loading', () => {
      tableBuilderService.getDataBlockGeoJson.mockReturnValue(
        new Promise(noop),
      );

      baseRender(
        <QueryClientProvider client={createQueryClient(testMapTableData)}>
          <DataBlockTabs
            releaseVersionId="release-1"
            id="test-block"
            dataBlock={testDataBlockMap}
            dataBlockStaleTime={Infinity}
          />
        </QueryClientProvider>,
      );

      expect(screen.getByRole('table', { hidden: true })).toBeInTheDocument();

      const chartSection = document.querySelector(
        '#test-block-charts',
      ) as HTMLElement;
      expect(
        within(chartSection).getByTestId('loadingSpinner'),
      ).toBeInTheDocument();
    });

    test('server-renders a placeholder when the table data is not cached', () => {
      const container = renderServerMarkup(
        <DataBlockTabs
          releaseVersionId="release-1"
          id="test-block"
          dataBlock={testTableDataBlock}
        />,
        createQueryClient(),
      );

      expect(
        container.querySelector('[data-testid="Data block - Test data block"]'),
      ).toBeEmptyDOMElement();
      expect(container.querySelector('table')).not.toBeInTheDocument();
    });
  });

  describe('with other tabs', () => {
    const testOtherTabs = (
      <TabsSection title="Summary" id="test-block-summary">
        <p>Summary content</p>
      </TabsSection>
    );

    test('server-renders the other tabs when not lazy, without table data', () => {
      const container = document.createElement('div');
      container.innerHTML = renderToStaticMarkup(
        <QueryClientProvider client={new QueryClient()}>
          <DataBlockTabs
            releaseVersionId="release-1"
            id="test-block"
            dataBlock={testDataBlock}
            firstTabs={testOtherTabs}
            lazy={false}
          />
        </QueryClientProvider>,
      );

      expect(container).toHaveTextContent('Summary content');
      expect(
        container.querySelector(
          '#test-block-tables [data-testid="loadingSpinner"]',
        ),
      ).not.toBeNull();
    });

    test('renders the other tabs while the table data is loading', () => {
      tableBuilderService.getDataBlockTableData.mockReturnValue(
        new Promise(noop),
      );

      render(
        <DataBlockTabs
          releaseVersionId="release-1"
          id="test-block"
          dataBlock={testDataBlock}
          firstTabs={testOtherTabs}
          lazy={false}
        />,
      );

      expect(screen.getByText('Summary content')).toBeInTheDocument();

      const tableSection = document.querySelector(
        '#test-block-tables',
      ) as HTMLElement;
      expect(
        within(tableSection).getByTestId('loadingSpinner'),
      ).toBeInTheDocument();
    });

    test('renders the other tabs if the table data fails to load', async () => {
      tableBuilderService.getDataBlockTableData.mockRejectedValue(
        new Error('Something went wrong'),
      );

      render(
        <DataBlockTabs
          releaseVersionId="release-1"
          id="test-block"
          dataBlock={testDataBlock}
          firstTabs={testOtherTabs}
          lazy={false}
        />,
      );

      const tableSection = document.querySelector(
        '#test-block-tables',
      ) as HTMLElement;

      expect(
        await within(tableSection).findByText('Could not load content'),
      ).toBeInTheDocument();
      expect(screen.getByText('Summary content')).toBeInTheDocument();
    });
  });
});
