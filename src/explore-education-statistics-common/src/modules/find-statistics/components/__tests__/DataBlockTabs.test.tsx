import render from '@common-test/render';
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
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import _tableBuilderService from '@common/services/tableBuilderService';
import { DataBlock } from '@common/services/types/blocks';
import { screen, waitFor, within } from '@testing-library/dom';
import userEvent from '@testing-library/user-event';
import { AxiosError } from 'axios';
import { forceVisible } from 'react-lazyload';
import React, { ReactNode } from 'react';
import { act } from '@testing-library/react';

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
    dataBlockParentId: 'block-1-parent',
    dataSetName: 'Test data set',
    dataSetId: 'test-data-set',
    type: 'DataBlock',
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
    dataBlockParentId: 'block-1-parent',
    dataSetId: 'test-data-set',
    dataSetName: 'Test data set',
    type: 'DataBlock',
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
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
      );

      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
        'release-1',
        'block-1-parent',
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
        'block-1-parent',
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
        'block-1-parent',
      );

      expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
        'release-1',
        'block-1-parent',
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
      'block-1-parent',
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
            dataBlockParentId: 'block-2-parent-id',
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
});
