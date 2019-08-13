import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import _dataBlockService, {
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import React from 'react';
import { render, wait } from 'react-testing-library';
import DataBlock from '../DataBlock';

jest.mock('@common/services/dataBlockService');

jest.mock('recharts/lib/util/LogUtils');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;

describe('DataBlock', () => {
  const dataBlockRequest: DataBlockRequest = {
    subjectId: 1,
    geographicLevel: GeographicLevel.Country,
    timePeriod: {
      startYear: '2014',
      startCode: 'HT6',
      endYear: '2015',
      endCode: 'HT6',
    },
    filters: ['1', '2'],
    indicators: ['23', '26', '28'],
  };

  test('renders horizontal chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
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
            type: 'horizontalbar',
            stacked: true,
            labels: {
              '23_1_2': {
                name: '23_1_2',
                label: 'Label 23_1_2',
                value: '23_1_2',
                unit: '%',
              },
              '26_1_2': {
                name: '26_1_2',
                label: 'Label 26_1_2',
                value: '26_1_2',
                unit: '%',
              },
              '28_1_2': {
                name: '28_1_2',
                label: 'Label 28_1_2',
                value: '28_1_2',
                unit: '%',
              },
            },
            axes: {
              major: {
                name: 'major',
                type: 'major',
                groupBy: 'timePeriods',
                dataSets: [
                  { indicator: '23', filters: ['1', '2'] },
                  { indicator: '26', filters: ['1', '2'] },
                  { indicator: '28', filters: ['1', '2'] },
                ],
              },
              minor: {
                name: 'minor',
                type: 'minor',
                dataSets: [],
              },
            },
            width: 800,
            height: 600,
          },
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(
      container.querySelector('section.govuk-tabs__panel h3'),
    ).toHaveTextContent('Charts');

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
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
            type: 'verticalbar',
            labels: {
              '23_1_2': {
                name: '23_1_2',
                label: 'Label 23_1_2',
                value: '23_1_2',
                unit: '%',
              },
              '26_1_2': {
                name: '26_1_2',
                label: 'Label 26_1_2',
                value: '26_1_2',
                unit: '%',
              },
              '28_1_2': {
                name: '28_1_2',
                label: 'Label 28_1_2',
                value: '28_1_2',
                unit: '%',
              },
            },

            axes: {
              major: {
                name: 'major',
                type: 'major',
                groupBy: 'timePeriods',
                dataSets: [
                  { indicator: '23', filters: ['1', '2'] },
                  { indicator: '26', filters: ['1', '2'] },
                  { indicator: '28', filters: ['1', '2'] },
                ],
              },
              minor: {
                name: 'minor',
                type: 'minor',
                dataSets: [],
              },
            },
            width: 800,
            height: 600,
          },
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(
      container.querySelector('section.govuk-tabs__panel h3'),
    ).toHaveTextContent('Charts');

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
  });

  test('renders table', () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
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
        return Promise.resolve(testData.response);
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
          description: {
            type: 'MarkDownBlock',
            body: `<div>test</div>`,
          },
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
        return Promise.resolve(testData.response);
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
            type: 'map',
            labels: {
              '23_1_2': {
                name: '23_1_2',
                label: 'Label 23_1_2',
                value: '23_1_2',
                unit: '%',
              },
              '26_1_2': {
                name: '26_1_2',
                label: 'Label 26_1_2',
                value: '26_1_2',
                unit: '%',
              },
              '28_1_2': {
                name: '28_1_2',
                label: 'Label 28_1_2',
                value: '28_1_2',
                unit: '%',
              },
            },

            axes: {
              major: {
                name: 'major',
                type: 'major',
                groupBy: 'locations',
                dataSets: [
                  { indicator: '23', filters: ['1', '2'] },
                  { indicator: '26', filters: ['1', '2'] },
                  { indicator: '28', filters: ['1', '2'] },
                ],
              },
              minor: {
                name: 'minor',
                type: 'minor',
                dataSets: [],
              },
            },
            width: 800,
            height: 600,
          },
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.querySelector('#test-datablock-charts')).toMatchSnapshot();
  });
});
