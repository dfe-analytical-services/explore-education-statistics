import {
  testChartPropsWithData1,
  testChartPropsWithMultipleData,
  testDataBlockResponse,
} from '@common/modules/charts/components/__tests__/__data__/testBlockData';
import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import _dataBlockService, {
  GeographicLevel,
} from '@common/services/dataBlockService';
import {
  Chart,
  DataBlock,
  DataBlockRequest,
} from '@common/services/types/blocks';
import { render, wait } from '@testing-library/react';
import React from 'react';
import DataBlockRenderer from '../DataBlockRenderer';

jest.mock('@common/services/dataBlockService');

jest.mock('recharts/lib/util/LogUtils');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
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
      geographicLevel: GeographicLevel.Country,
      timePeriod: {
        startYear: 2014,
        startCode: 'HT6',
        endYear: 2015,
        endCode: 'HT6',
      },
      filters: ['1', '2'],
      indicators: ['23', '26', '28'],
    },
    charts: [],
    tables: [],
  };
  test('renders horizontal chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      () => Promise.resolve(testDataBlockResponse),
    );

    const { container } = render(
      <DataBlockRenderer
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [
            {
              ...testChartPropsWithData1,
              type: 'horizontalbar',
            } as Chart,
          ],
        }}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(
      testDataBlock.dataBlockRequest,
    );

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testDataBlockResponse);
      },
    );

    const { container } = render(
      <DataBlockRenderer
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [
            {
              ...testChartPropsWithData1,
              type: 'verticalbar',
            } as Chart,
          ],
        }}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(
      testDataBlock.dataBlockRequest,
    );

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders table', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testDataBlockResponse);
      },
    );

    const { container } = render(
      <DataBlockRenderer
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          tables: [
            {
              tableHeaders: {
                rowGroups: [],
                columnGroups: [],
                rows: [
                  {
                    value: '23',
                    label: 'Unauthorised absence rate',
                  },
                ],
                columns: [
                  {
                    value: '2014_HT6',
                    label: '2014/15',
                  },
                  {
                    value: '2015_HT6',
                    label: '2015/16',
                  },
                ],
              },
              indicators: ['23'],
            },
          ],
        }}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(
      testDataBlock.dataBlockRequest,
    );

    expect(container.querySelector('table')).toMatchSnapshot();
  });

  test('renders map instead of chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testDataBlockResponse);
      },
    );

    const { container } = render(
      <DataBlockRenderer
        id="test-datablock"
        dataBlock={{
          ...testDataBlock,
          charts: [
            {
              ...testChartPropsWithMultipleData,
              type: 'map',
            } as ChartRendererProps,
          ],
        }}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(
      testDataBlock.dataBlockRequest,
    );

    expect(container.querySelector('#test-datablock-charts')).toMatchSnapshot();
  });
});
