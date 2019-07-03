/* eslint-disable @typescript-eslint/camelcase */
import {
  AxisConfigurationItem,
  ChartDataSet,
} from '@common/services/publicationService';
import * as ChartFunctions from '../ChartFunctions';

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

  const dataSet26_1_71: ChartDataSet = {
    indicator: '26',
    filters: ['1', '71'],
  };

  const DataSet_SingleValue: ChartDataSet[] = [dataSet23_1_72];
  const DataSet_MultipleIndicator = [dataSet23_1_72, dataSet26_1_72];
  const DataSet_MultipleFilter = [dataSet26_1_71, dataSet26_1_72];

  test('createDataForAxis from single indicator', () => {
    const meta = Data.responseData.metaData;

    const minorAxisConfiguration: AxisConfigurationItem = {
      name: meta.indicators['26'].label,
      groupBy: ['timePeriod'],
      dataSets: [dataSet26_1_72],
    };

    const chartData = ChartFunctions.createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
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

  test('createDataForAxis from multiple indicators', () => {
    const meta = Data.responseData.metaData;

    const minorAxisConfiguration: AxisConfigurationItem = {
      name: meta.indicators['26'].label,
      groupBy: ['timePeriod'],
      dataSets: [dataSet26_1_72, dataSet23_1_72],
    };

    const chartData = ChartFunctions.createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
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

    const minorAxisConfiguration: AxisConfigurationItem = {
      name: meta.indicators['26'].label,
      groupBy: ['timePeriod'],
      dataSets: [dataSet26_1_71, dataSet26_1_72],
    };

    const chartData = ChartFunctions.createDataForAxis(
      minorAxisConfiguration,
      Data.responseData.result,
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
});
