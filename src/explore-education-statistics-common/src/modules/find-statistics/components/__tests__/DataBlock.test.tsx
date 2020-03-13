import testBlockData from '@common/modules/charts/components/__tests__/__data__/testBlockData';
import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import _dataBlockService, {
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import { Chart } from '@common/services/publicationService';
import { render, wait } from '@testing-library/react';
import React from 'react';
import DataBlock from '../DataBlock';

jest.mock('@common/services/dataBlockService');

jest.mock('recharts/lib/util/LogUtils');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;

describe('DataBlock', () => {
  const dataBlockRequest: DataBlockRequest = {
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
  };

  test('renders horizontal chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testBlockData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        charts={[
          {
            ...testBlockData.chartProps1,
            type: 'horizontalbar',
          } as Chart,
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testBlockData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        charts={[
          {
            ...testBlockData.chartProps1,
            type: 'verticalbar',
          } as Chart,
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders table', () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testBlockData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables
      />,
    );

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.querySelector('#test-datablock-table')).toMatchSnapshot();
  });

  test('renders summary', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testBlockData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test-datablock"
        type="databock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        summary={{
          dataKeys: ['23', '26', '28'],
          dataSummary: ['up 10%', 'down 10%', 'up 11%'],
          dataDefinition: ['a', 'b', 'c'],
          dataDefinitionTitle: ['a', 'b', 'c'],
        }}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelector('#test-datablock-summary'),
    ).toMatchSnapshot();
  });

  test('renders map instead of chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testBlockData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test-datablock"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        charts={[
          {
            ...testBlockData.AbstractMultipleChartProps,
            type: 'map',
          } as ChartRendererProps,
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.querySelector('#test-datablock-charts')).toMatchSnapshot();
  });
});
