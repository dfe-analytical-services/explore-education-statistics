import {
  testChartConfiguration,
  testChartTableData,
  testDeprecatedChartConfiguration,
} from '@common/modules/charts/components/__tests__/__data__/testChartData';
import {
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import _tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';
import {
  Chart,
  DataBlock,
  DataBlockRequest,
} from '@common/services/types/blocks';
import { waitFor, screen } from '@testing-library/dom';
import { render } from '@testing-library/react';
import React from 'react';
import DataBlockRenderer from '../DataBlockRenderer';

jest.mock('@common/services/tableBuilderService');

jest.mock('recharts/lib/util/LogUtils');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('DataBlockRenderer', () => {
  const testDataBlock: DataBlock = {
    id: 'test-id',
    type: 'DataBlock',
    heading: '',
    order: 0,
    name: 'Test data block',
    source: '',
    dataBlockRequest: {
      subjectId: '1',
      geographicLevel: 'country',
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
      locations: {},
    },
    charts: [],
    tables: [],
  };

  test('renders line chart', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      () => Promise.resolve(testChartTableData),
    );

    const { container } = render(
      <DataBlockRenderer
        releaseId="test-release-id"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testChartConfiguration],
        }}
      />,
    );

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: false,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(
        container.querySelectorAll('section.govuk-tabs__panel'),
      ).toHaveLength(1);

      expect(container.querySelectorAll('.recharts-line')).toHaveLength(3);
    });
  });

  test('renders horizontal chart', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      () => Promise.resolve(testChartTableData),
    );

    const { container } = render(
      <DataBlockRenderer
        releaseId="test-release-id"
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

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: false,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(
        container.querySelectorAll('section.govuk-tabs__panel'),
      ).toHaveLength(1);

      expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    });
  });

  test('renders vertical chart', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      (_: DataBlockRequest, releaseId?: string) => {
        return Promise.resolve(testChartTableData);
      },
    );

    const { container } = render(
      <DataBlockRenderer
        releaseId="test-release-id"
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

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: false,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(
        container.querySelectorAll('section.govuk-tabs__panel'),
      ).toHaveLength(1);

      expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    });
  });

  test('renders table', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      (_: DataBlockRequest, releaseId?: string) => {
        return Promise.resolve(testChartTableData);
      },
    );

    const fullTable = mapFullTable(testChartTableData);

    render(
      <DataBlockRenderer
        releaseId="test-release-id"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          tables: [
            {
              tableHeaders: mapUnmappedTableHeaders(
                getDefaultTableHeaderConfig(fullTable.subjectMeta),
              ),
              indicators: [
                'authorised-absence-rate',
                'unauthorised-absence-rate',
                'overall-absence-rate',
              ],
            },
          ],
        }}
      />,
    );

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: false,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(screen.getByRole('table')).toBeInTheDocument();
      expect(screen.getAllByRole('row')).toHaveLength(4);
      expect(screen.getAllByRole('cell')).toHaveLength(16);
    });
  });

  test('renders map', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      (_: DataBlockRequest, releaseId?: string) => {
        return Promise.resolve(testMapTableData);
      },
    );

    const { container } = render(
      <DataBlockRenderer
        releaseId="test-release-id"
        id="test-block"
        dataBlock={{
          ...testDataBlock,
          charts: [testMapConfiguration],
        }}
      />,
    );

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: true,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(container.querySelector('.leaflet-container')).toBeInTheDocument();
    });
  });

  test('can render line chart with deprecated `labels` for data sets', async () => {
    const getDataBlockForSubject = tableBuilderService.getTableData.mockImplementation(
      () => Promise.resolve(testChartTableData),
    );

    const { container } = render(
      <DataBlockRenderer
        releaseId="test-release-id"
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [testDeprecatedChartConfiguration],
        }}
      />,
    );

    await waitFor(() => {
      expect(getDataBlockForSubject).toBeCalledWith(
        {
          ...testDataBlock.dataBlockRequest,
          includeGeoJson: false,
        } as TableDataQuery,
        'test-release-id',
      );

      expect(
        container.querySelectorAll('section.govuk-tabs__panel'),
      ).toHaveLength(1);

      expect(container.querySelectorAll('.recharts-line')).toHaveLength(3);

      const legendItems = container.querySelectorAll('.recharts-legend-item');
      expect(legendItems[0]).toHaveTextContent(
        'Unauthorised absence rate (England)',
      );
      expect(legendItems[1]).toHaveTextContent(
        'Authorised absence rate (England)',
      );
      expect(legendItems[2]).toHaveTextContent(
        'Overall absence rate (England)',
      );
    });
  });
});
