/* eslint-disable @typescript-eslint/camelcase */
import {
  AxisConfiguration,
  ChartDataSet,
} from '@common/services/publicationService';
import {
  ChartMetaData,
  createDataForAxis,
  createSortedDataForAxis,
  parseMetaData,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import Data from './__data__/chartFunctionsData';

describe('ChartFunctions', () => {
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
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const minorAxisConfiguration: AxisConfiguration = {
      name: meta.indicators['26'].label,
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_72],
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_HT6',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_HT6',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_HT6',
        '26_1_72_____': '4',
      },
      {
        name: '2015_HT6',
        '26_1_72_____': '4',
      },
      {
        name: '2016_HT6',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis can sort by indicator from single indicator', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const axis: AxisConfiguration = {
      name: meta.indicators['99'].label,
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: '99_1_72_____',
      sortAsc: true,
      dataSets: [dataSet99_1_72],
    };

    const chartData = createSortedDataForAxis(
      axis,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData).toEqual([
      {
        name: '2013_HT6',
        '99_1_72_____': '98',
      },
      {
        name: '2012_HT6',
        '99_1_72_____': '99',
      },
      {
        name: '2015_HT6',
        '99_1_72_____': '100',
      },
      {
        name: '2016_HT6',
        '99_1_72_____': '101',
      },
      {
        name: '2014_HT6',
        '99_1_72_____': '102',
      },
    ]);
  });

  test('createDataForAxis from multiple indicators', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const minorAxisConfiguration: AxisConfiguration = {
      name: meta.indicators['26'].label,
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_72, dataSet23_1_72],
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_HT6',
        '23_1_72_____': '0.8',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_HT6',
        '23_1_72_____': '0.8',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_HT6',
        '23_1_72_____': '0.9',
        '26_1_72_____': '4',
      },
      {
        name: '2015_HT6',
        '23_1_72_____': '0.9',
        '26_1_72_____': '4',
      },
      {
        name: '2016_HT6',
        '23_1_72_____': '1.1',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis from multiple filters', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const minorAxisConfiguration: AxisConfiguration = {
      name: meta.indicators['26'].label,
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_71, dataSet26_1_72],
    };

    const chartData = createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_HT6',
        '26_1_71_____': '9.6',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_HT6',
        '26_1_71_____': '9',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_HT6',
        '26_1_71_____': '9.4',
        '26_1_72_____': '4',
      },
      {
        name: '2015_HT6',
        '26_1_71_____': '9.1',
        '26_1_72_____': '4',
      },
      {
        name: '2016_HT6',
        '26_1_71_____': '9.7',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('createDataForAxis returns full data range if data is missing', () => {
    const meta = Data.responseWithMissingData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    const axisConfig: AxisConfiguration = {
      name: meta.indicators['26'].label,
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [dataSet26_1_71, dataSet26_1_72],
    };

    const chartData = createDataForAxis(
      axisConfig,
      Data.responseWithMissingData.result,
      chartMeta,
    );

    expect(chartData.sort((a, b) => a.name.localeCompare(b.name))).toEqual([
      {
        name: '2012_HT6',
        '26_1_71_____': '9.6',
        '26_1_72_____': '4.7',
      },
      {
        name: '2013_HT6',
        '26_1_71_____': '9',
        '26_1_72_____': '3.9',
      },
      {
        name: '2014_HT6',
      },
      {
        name: '2015_HT6',
        '26_1_71_____': '9.1',
        '26_1_72_____': '4',
      },
      {
        name: '2016_HT6',
        '26_1_71_____': '9.7',
        '26_1_72_____': '4',
      },
    ]);
  });

  test('parseMetaData', () => {
    const meta = Data.responseData.metaData;
    const chartMeta = parseMetaData(meta) as ChartMetaData;

    expect(chartMeta.filters).not.toBeUndefined();
    expect(chartMeta.indicators).not.toBeUndefined();
    expect(chartMeta.locations).not.toBeUndefined();
    expect(chartMeta.timePeriod).not.toBeUndefined();

    expect(chartMeta.filters['1']).not.toBeUndefined();
    expect(chartMeta.filters['1'].label).toEqual('All pupils');
    expect(chartMeta.filters['1'].value).toEqual('1');
  });
});
