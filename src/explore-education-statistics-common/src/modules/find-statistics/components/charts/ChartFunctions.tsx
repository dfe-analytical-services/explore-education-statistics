import {
  Axis,
  ChartDataSet,
  ChartType,
  DataLabelConfigurationItem,
  ReferenceLine,
} from '@common/services/publicationService';
import React, { ReactNode } from 'react';
import {
  Label,
  PositionType,
  ReferenceLine as RechartsReferenceLine,
  XAxis,
  XAxisProps,
  YAxis,
  YAxisProps,
} from 'recharts';
import {
  DataBlockData,
  DataBlockMetadata,
  Result,
} from '@common/services/dataBlockService';
import difference from 'lodash/difference';
import { Dictionary } from '@common/types';

export interface ChartProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  dataSets: ChartDataSet[];
  dataLabels: Dictionary<DataLabelConfigurationItem>;

  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  width?: number;
  referenceLines?: ReferenceLine[];
}

export interface ChartData {
  name: string;
  indicator: string | undefined;
  data?: ChartData[];
  value?: string;
}

export interface DataSetResult {
  dataSet: ChartDataSet;

  results: Result[];
}

export interface ChartDefinition {
  type: ChartType;
  name: string;

  data: {
    type: string;
    title: string;
    entryCount: number | 'multiple';
    targetAxis: string;
  }[];

  axes: {
    id: string;
    title: string;
    type: 'major' | 'value' | 'group';
    defaultDataType?: 'indicator' | 'filter' | 'location' | 'timePeriod';
  }[];
}

const ChartFunctions = {
  calculateAxis(
    axis: Axis,
    position: PositionType,
    angle: number = 0,
    titleSize: number = 25,
  ) {
    let size = axis.size || 25;
    let title: ReactNode | '';

    if (axis.title) {
      size += titleSize;
      title = (
        <Label position={position} angle={angle}>
          {axis.title}
        </Label>
      );
    }

    return { size, title };
  },

  calculateMargins(xAxis: Axis, yAxis: Axis, referenceLines?: ReferenceLine[]) {
    const margin = {
      top: 15,
      right: 30,
      left: 60,
      bottom: 25,
    };

    if (referenceLines && referenceLines.length > 0) {
      referenceLines.forEach(line => {
        if (line.x) margin.top = 25;
        if (line.y) margin.left = 75;
      });
    }

    if (xAxis.title) {
      margin.bottom += 25;
    }

    return margin;
  },

  calculateXAxis(xAxis: Axis, axisProps: XAxisProps): ReactNode {
    const { size: height, title } = ChartFunctions.calculateAxis(
      xAxis,
      'insideBottom',
    );
    return (
      <XAxis {...axisProps} height={height}>
        {title}
      </XAxis>
    );
  },

  calculateYAxis(yAxis: Axis, axisProps: YAxisProps): ReactNode {
    const { size: width, title } = ChartFunctions.calculateAxis(
      yAxis,
      'left',
      270,
      90,
    );
    return (
      <YAxis {...axisProps} width={width}>
        {title}
      </YAxis>
    );
  },

  generateReferenceLines(referenceLines: ReferenceLine[]): ReactNode {
    const generateReferenceLine = (line: ReferenceLine, idx: number) => {
      const referenceLineProps = {
        key: `ref_${idx}`,
        ...line,
        stroke: 'black',
        strokeWidth: '2px',

        label: {
          position: 'top',
          value: line.label,
        },
      };

      // Using <Label> in the label property is causing an infinite loop
      // forcing the use of the properties directly as per https://github.com/recharts/recharts/issues/730
      // appears to be a fix, but this is not valid for the types.
      // issue raised https://github.com/recharts/recharts/issues/1710
      // @ts-ignore
      return <RechartsReferenceLine {...referenceLineProps} />;
    };

    return referenceLines.map(generateReferenceLine);
  },

  filterDataByAxis(data: DataBlockData, sourceAxis: Axis) {
    return data.result.filter(result => {
      return (
        sourceAxis.key &&
        sourceAxis.key.length > 0 &&
        sourceAxis.key.some(key => difference(result.filters, key).length === 0)
      );
    });
  },

  filterResultsBySingleDataSet(dataSet: ChartDataSet, results: Result[]) {
    return results.filter(
      r =>
        Object.keys(r.measures).includes(dataSet.indicator) &&
        (dataSet.filters &&
          difference(r.filters, dataSet.filters).length === 0),
    );
  },

  filterNonRelaventDataFromDataSet(dataSet: ChartDataSet, results: Result[]) {
    return results.map(result => {
      return {
        ...result,
        measures: Object.entries(result.measures)
          .filter(([measureId]) => dataSet.indicator === measureId)
          .reduce<Dictionary<string>>(
            (newMeasures, [measureId, measureValue]) => ({
              ...newMeasures,
              [measureId]: measureValue,
            }),
            {},
          ),
      };
    });
  },

  filterResultsByDataSet(dataSets: ChartDataSet[], results: Result[]) {
    return dataSets.map(dataSet => {
      return {
        dataSet,
        results: ChartFunctions.filterNonRelaventDataFromDataSet(
          dataSet,
          ChartFunctions.filterResultsBySingleDataSet(dataSet, results),
        ),
      };
    });
  },

  groupByYear(
    dataSet: ChartDataSet,
    results: Result[],
    meta: DataBlockMetadata,
  ) {
    return results.reduce<ChartData[]>((cd, result) => {
      let name = `${result.year}_${result.timeIdentifier}`;

      name =
        (meta.timePeriods &&
          meta.timePeriods[name] &&
          meta.timePeriods[name].label) ||
        name;

      return [
        ...cd,
        {
          name,
          indicator: name,
          value: result.measures[dataSet.indicator],
        },
      ];
    }, []);
  },

  generateDataGroupedByIndicators(
    dataSetResults: DataSetResult[],
    meta: DataBlockMetadata,
  ) {
    return dataSetResults.reduce<ChartData[]>((cd, { dataSet, results }) => {
      return [
        ...cd,

        {
          name: `${dataSet.indicator}_${dataSet.filters &&
            dataSet.filters.join('_')}`,
          indicator: dataSet.indicator,
          data: ChartFunctions.groupByYear(dataSet, results, meta),
        },
      ];
    }, []);
  },

  flattenChartDataItem(chartData: ChartData) {
    return (chartData.data || []).reduce(
      (cd, next) => ({
        ...cd,
        [next.name]: [next.value],
      }),
      {
        name: chartData.name,
      },
    );
  },

  flattenChartData(chartData: ChartData[]) {
    return chartData.map(cd => ChartFunctions.flattenChartDataItem(cd));
  },

  generateDataGroupedByGroups(
    dataSetResults: DataSetResult[],
    meta: DataBlockMetadata,
  ): ChartData[] {
    const chartMap = dataSetResults.reduce<Dictionary<ChartData[]>>(
      (chartDataMap, { dataSet, results }) => {
        const currentIndicator = dataSet.indicator;
        const newChartDataMap = { ...chartDataMap };

        results.forEach(result => {
          const dataKey = `${result.year}_${result.timeIdentifier}`;

          newChartDataMap[dataKey] = [
            ...(newChartDataMap[dataKey] || []),
            {
              name: `${currentIndicator}_${result.filters.join('_')}`,
              indicator: currentIndicator,
              value: result.measures[currentIndicator],
            },
          ];
        }, {});

        return newChartDataMap;
      },
      {},
    );

    return Object.entries(chartMap).map(([key, value]) => {
      return {
        name:
          (meta.timePeriods &&
            meta.timePeriods[key] &&
            meta.timePeriods[key].label) ||
          key,
        indicator: undefined,
        data: value,

        ...value.reduce((values, v) => {
          return {
            ...values,
            [v.name]: v.value,
          };
        }, {}),
      };
    });
  },
};

export default ChartFunctions;
