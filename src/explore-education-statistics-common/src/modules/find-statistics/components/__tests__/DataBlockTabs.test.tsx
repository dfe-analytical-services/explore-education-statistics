import {
  testChartConfiguration,
  testChartTableData,
} from '@common/modules/charts/components/__tests__/__data__/testChartData';
import {
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import _tableBuilderService from '@common/services/tableBuilderService';
import { Chart, DataBlock } from '@common/services/types/blocks';
import { screen, waitFor } from '@testing-library/dom';
import { render } from '@testing-library/react';
import { AxiosError } from 'axios';
import React from 'react';
import { forceVisible } from 'react-lazyload';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';

jest.mock('@common/services/tableBuilderService');

jest.mock('recharts/lib/util/LogUtils');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataBlockTabs', () => {
  const testDataBlock: DataBlock = {
    id: 'block-1',
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
      indicators: [],
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
      indicators: [],
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
        releaseId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
        'release-1',
        'block-1',
      );

      expect(screen.getAllByText('Could not load content')).toHaveLength(2);
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
        releaseId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
        'release-1',
        'block-1',
      );

      expect(
        screen.queryByText('Could not load content'),
      ).not.toBeInTheDocument();
      expect(screen.queryByRole('table')).not.toBeInTheDocument();
    });
  });

  test('renders line chart', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const { container } = render(
      <DataBlockTabs
        releaseId="release-1"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
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
        releaseId="release-1"
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

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
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
        releaseId="release-1"
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

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
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
        releaseId="release-1"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          table: {
            tableHeaders: mapUnmappedTableHeaders(
              getDefaultTableHeaderConfig(fullTable),
            ),
            indicators: [
              'authorised-absence-rate',
              'unauthorised-absence-rate',
              'overall-absence-rate',
            ],
          },
        }}
      />,
    );

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
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

    const { container } = render(
      <DataBlockTabs
        releaseId="release-1"
        id="test-block"
        dataBlock={testDataBlockMap}
      />,
    );

    forceVisible();

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toBeCalledWith(
        'release-1',
        'block-1',
      );

      expect(container.querySelector('.leaflet-container')).toBeInTheDocument();
    });
  });

  test('re-rendering with new data block does not throw error', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testChartTableData,
    );

    const fullTable = mapFullTable(testChartTableData);

    const { rerender } = render(
      <DataBlockTabs
        id="test-block"
        releaseId="release-1"
        dataBlock={{
          ...testDataBlock,
          table: {
            tableHeaders: mapUnmappedTableHeaders(
              getDefaultTableHeaderConfig(fullTable),
            ),
            indicators: [
              'authorised-absence-rate',
              'unauthorised-absence-rate',
              'overall-absence-rate',
            ],
          },
        }}
      />,
    );

    forceVisible();

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
          releaseId="release-1"
          dataBlock={{
            ...testDataBlock,
            id: 'block-2-id',
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
              indicators: ['authorised-absence-sessions'],
            },
          }}
        />,
      );
    }).not.toThrowError();

    await waitFor(() => {
      expect(screen.getByRole('table')).toBeInTheDocument();
      expect(screen.getAllByRole('row')).toHaveLength(2);
      expect(screen.getAllByRole('cell')).toHaveLength(2);
    });
  });
});
