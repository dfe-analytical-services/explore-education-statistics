/* eslint-disable @typescript-eslint/camelcase */
import {ChartDataSet} from '@common/services/publicationService';
import ChartFunctions from '../ChartFunctions';

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

  test('filterResultsBySingleDataSet', () => {
    const dataSetResult = ChartFunctions.filterResultsBySingleDataSet(
      dataSet23_1_72,
      Data.responseData.result,
    );

    expect(dataSetResult.length).toBe(5);

    dataSetResult.forEach(result => {
      expect(result.filters).toContain('1');
      expect(result.filters).toContain('72');

      expect(Object.keys(result.measures).length).toBe(3);

      expect(result.measures['23']).toBeDefined();
    });
  });

  test('filterNonRelaventDataFromDataSet', () => {
    const dataSetResult = ChartFunctions.filterNonRelaventDataFromDataSet(
      dataSet23_1_72,
      ChartFunctions.filterResultsBySingleDataSet(
        dataSet23_1_72,
        Data.responseData.result,
      ),
    );

    expect(dataSetResult.length).toBe(5);

    dataSetResult.forEach(result => {
      expect(result.filters).toContain('1');
      expect(result.filters).toContain('72');

      expect(Object.keys(result.measures).length).toBe(1);

      expect(result.measures['23']).toBeDefined();
    });
  });

  test('filterResultsByDataSet with single indicator', () => {
    const dataSetResults = ChartFunctions.filterResultsByDataSet(
      DataSet_SingleValue,
      Data.responseData.result,
    );

    expect(dataSetResults.length).toBe(1);

    const [dataSetResult] = dataSetResults;

    expect(dataSetResult.dataSet).toBe(DataSet_SingleValue[0]);
    expect(dataSetResult.results.length).toBe(5);
  });

  test('filterResultsByDataSet with multiple indicator', () => {
    const dataSetResults = ChartFunctions.filterResultsByDataSet(
      DataSet_MultipleIndicator,
      Data.responseData.result,
    );

    expect(dataSetResults.length).toBe(2);

    dataSetResults.forEach((dataSetResult, index) => {
      expect(dataSetResults[index].dataSet).toBe(
        DataSet_MultipleIndicator[index],
      );

      expect(dataSetResults[index].results.length).toBe(5);

      dataSetResults[index].results.forEach(result => {
        expect(Object.keys(result.measures).length).toBe(1);
        expect(
          result.measures[DataSet_MultipleIndicator[index].indicator],
        ).toBeDefined();
        expect(result.filters).toEqual(
          DataSet_MultipleIndicator[index].filters,
        );
      });
    });
  });

  test('buildMappedDataSets maps Single indicators', () => {
    const dataSetResults = ChartFunctions.buildMappedDataSets(
      DataSet_SingleValue,
      Data.responseData.result
    );

    expect(dataSetResults.length).toBe(1);

    expect(dataSetResults[0].dataSet.indicator).toBe('23');
    expect(dataSetResults[0].dataSet.filters).toBeDefined();
    // @ts-ignore
    expect(dataSetResults[0].dataSet.filters).toEqual(['1', '72']);

    expect(dataSetResults[0].results.length).toBe(5);
  });

  test('buildMappedDataSets maps Multiple indicators', () => {
    const dataSetResults = ChartFunctions.buildMappedDataSets(
      DataSet_MultipleIndicator,
      Data.responseData.result
    );

    expect(dataSetResults.length).toBe(2);

    expect(dataSetResults[0].dataSet.indicator).toBe('23');
    expect(dataSetResults[1].dataSet.indicator).toBe('26');

    expect(dataSetResults[0].dataSet.filters).toBeDefined();
    // @ts-ignore
    expect(dataSetResults[0].dataSet.filters).toEqual(['1', '72']);
    // @ts-ignore
    expect(dataSetResults[1].dataSet.filters).toEqual(['1', '72']);

    expect(dataSetResults[0].results.length).toBe(5);
  });

  test('buildMappedDataSets maps Multiple filters', () => {
    const dataSetResults = ChartFunctions.buildMappedDataSets(
      DataSet_MultipleFilter,
      Data.responseData.result
    );

    expect(dataSetResults.length).toBe(2);

    expect(dataSetResults[0].dataSet.indicator).toBe('26');
    expect(dataSetResults[1].dataSet.indicator).toBe('26');

    expect(dataSetResults[0].dataSet.filters).toBeDefined();
    // @ts-ignore
    expect(dataSetResults[0].dataSet.filters).toEqual(['1', '71']);
    // @ts-ignore
    expect(dataSetResults[1].dataSet.filters).toEqual(['1', '72']);


    expect(dataSetResults[0].results.length).toBe(5);
  });

  const AxisConfig = [{
    name: "xaxis",
    dataSet: dataSet23_1_72
  }, {
    name: "xaxis",
    dataSet: dataSet26_1_72
  }
  ];

  test('groupFilteredDataForAxis', () => {
    const result = ChartFunctions.groupFilteredDataForAxis(
      Data.responseData.result,
      AxisConfig
    );

    console.log(result);
  });

  test('createChartDataForAxisData', () => {
    const axisData = ChartFunctions.createChartDataForAxisData(
      ChartFunctions.groupFilteredDataForAxis(
        Data.responseData.result,
        AxisConfig
      )
    );

    console.log(axisData);
  })

});
