/* eslint-disable @typescript-eslint/camelcase */
import {
  AxisConfiguration,
  ChartDataSet,
  ChartMetaData,
} from '@common/modules/charts/types/chart';

import Data from '@common/modules/charts/util/__tests__/__data__/chartUtils.data';
import {
  createDataForAxis,
  createSortedDataForAxis,
  parseMetaData,
} from '@common/modules/charts/util/chartUtils';

describe('chartUtils', () => {
  const dataSet23_1_72: ChartDataSet = {
    indicator: '23',
    filters: ['1', '72'],
  };

  const dataSet26_1_72: ChartDataSet = {
    indicator: '26',
    filters: ['1', '72'],
  };

  const dataSet99_1_72: ChartDataSet = {
    indicator: '99',
    filters: ['1', '72'],
  };

  const dataSet26_1_71: ChartDataSet = {
    indicator: '26',
    filters: ['1', '71'],
  };

  test('createDataForAxis from single indicator', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta);

    const minorAxisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_72],
      visible: true,
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_AY',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_AY',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_AY',
        '26_1_72_____': '4',
      },
      {
        name: '2015_AY',
        '26_1_72_____': '4',
      },
      {
        name: '2016_AY',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis can sort by indicator from single indicator', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const axis: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: '99_1_72_____',
      sortAsc: true,
      dataSets: [dataSet99_1_72],
      visible: true,
    };

    const chartData = createSortedDataForAxis(
      axis,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData).toEqual([
      {
        name: '2013_AY',
        '99_1_72_____': '98',
      },
      {
        name: '2012_AY',
        '99_1_72_____': '99',
      },
      {
        name: '2015_AY',
        '99_1_72_____': '100',
      },
      {
        name: '2016_AY',
        '99_1_72_____': '101',
      },
      {
        name: '2014_AY',
        '99_1_72_____': '102',
      },
    ]);
  });

  test('createDataForAxis from multiple indicators', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta);

    const minorAxisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_72, dataSet23_1_72],
      visible: true,
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_AY',
        '23_1_72_____': '0.8',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_AY',
        '23_1_72_____': '0.8',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_AY',
        '23_1_72_____': '0.9',
        '26_1_72_____': '4',
      },
      {
        name: '2015_AY',
        '23_1_72_____': '0.9',
        '26_1_72_____': '4',
      },
      {
        name: '2016_AY',
        '23_1_72_____': '1.1',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis from multiple filters', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta);

    const minorAxisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_71, dataSet26_1_72],
      visible: true,
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_AY',
        '26_1_71_____': '9.6',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_AY',
        '26_1_71_____': '9',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_AY',
        '26_1_71_____': '9.4',
        '26_1_72_____': '4',
      },
      {
        name: '2015_AY',
        '26_1_71_____': '9.1',
        '26_1_72_____': '4',
      },
      {
        name: '2016_AY',
        '26_1_71_____': '9.7',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis returns full data range if data is missing', () => {
    const meta = Data.responseWithMissingData.metaData;
    const chartMeta = parseMetaData(meta);

    const axisConfig: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_71, dataSet26_1_72],
      visible: true,
    };

    const chartData = createDataForAxis(
      axisConfig,
      Data.responseWithMissingData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_AY',
        '26_1_71_____': '9.6',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_AY',
        '26_1_71_____': '9',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_AY',
      },
      {
        name: '2015_AY',
        '26_1_71_____': '9.1',
        '26_1_72_____': '4',
      },
      {
        name: '2016_AY',
        '26_1_71_____': '9.7',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('parseMetaData', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta);

    expect(chartMeta.filters).not.toBeUndefined();
    expect(chartMeta.indicators).not.toBeUndefined();
    expect(chartMeta.locations).not.toBeUndefined();
    expect(chartMeta.timePeriod).not.toBeUndefined();
  });
});
