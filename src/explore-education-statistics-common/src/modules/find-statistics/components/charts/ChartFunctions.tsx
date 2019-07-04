import {
  Axis,
  ChartDataSet,
  ChartType,
  DataLabelConfigurationItem,
  ReferenceLine,
  AxisConfigurationItem,
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
  DataBlockLocation,
  DataBlockMetadata,
  Result,
} from '@common/services/dataBlockService';
import difference from 'lodash/difference';
import { Dictionary } from '@common/types';

export interface ChartProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  dataLabels: Dictionary<DataLabelConfigurationItem>;
  axes: Dictionary<AxisConfigurationItem>;
  height?: number;
  width?: number;
  referenceLines?: ReferenceLine[];
}

export interface ChartDataOld {
  name: string;
  indicator: string | undefined;
  data?: ChartDataOld[];
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
    type: 'major' | 'minor';
    defaultDataType?: 'timePeriod' | 'location' | 'filters' | 'indicator';
  }[];
}

function calculateAxis(
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
}

export function calculateMargins(
  xAxis: Axis,
  yAxis: Axis,
  referenceLines?: ReferenceLine[],
) {
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
}

export function calculateXAxis(xAxis: Axis, axisProps: XAxisProps): ReactNode {
  const { size: height, title } = calculateAxis(xAxis, 'insideBottom');
  return (
    <XAxis {...axisProps} height={height}>
      {title}
    </XAxis>
  );
}

export function calculateYAxis(yAxis: Axis, axisProps: YAxisProps): ReactNode {
  const { size: width, title } = calculateAxis(yAxis, 'left', 270, 90);
  return (
    <YAxis {...axisProps} width={width}>
      {title}
    </YAxis>
  );
}

export function generateReferenceLines(
  referenceLines: ReferenceLine[],
): ReactNode {
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
}

export function filterResultsBySingleDataSet(
  dataSet: ChartDataSet,
  results: Result[],
) {
  return results.filter(
    r =>
      dataSet.indicator &&
      Object.keys(r.measures).includes(dataSet.indicator) &&
      (dataSet.filters && difference(r.filters, dataSet.filters).length === 0),
  );
}

function filterNonRelaventDataFromDataSet(
  dataSet: ChartDataSet,
  results: Result[],
) {
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
}

function resultsForDataSet(results: Result[], ds: ChartDataSet) {
  return results.filter(result => {
    // fail fast with the two things that are most likely to not match
    if (ds.indicator && !Object.keys(result.measures).includes(ds.indicator))
      return false;

    if (ds.filters) {
      if (difference(ds.filters, result.filters).length !== 0) return false;
    }

    if (ds.location) {
      const { location } = result;
      if (
        location.country &&
        ds.location.country &&
        location.country.country_code !== ds.location.country.country_code
      )
        return false;
      if (
        location.region &&
        ds.location.region &&
        location.region.region_code !== ds.location.region.region_code
      )
        return false;
      if (
        location.localAuthorityDistrict &&
        ds.location.localAuthorityDistrict &&
        location.localAuthorityDistrict.sch_lad_code !==
          ds.location.localAuthorityDistrict.sch_lad_code
      )
        return false;
      if (
        location.localAuthority &&
        ds.location.localAuthority &&
        location.localAuthority.new_la_code !==
          ds.location.localAuthority.new_la_code
      )
        return false;
    }

    if (ds.timePeriod) {
      if (ds.timePeriod !== `${result.year}_${result.timeIdentifier}`)
        return false;
    }

    return true;
  });
}

export interface ChartDataB {
  name: string;

  [key: string]: string;
}

function generateKeyFromAxisConfiguration(
  dataSet: ChartDataSet,
  groupBy: string[],
) {
  const { indicator, filters, location, timePeriod } = dataSet;
  return [
    indicator,
    ...(filters || []),
    location && location.country && location.country.country_code,
    location && location.region && location.region.region_code,
    location &&
      location.localAuthorityDistrict &&
      location.localAuthorityDistrict.sch_lad_code,
    location && location.localAuthority && location.localAuthority.new_la_code,
    (!groupBy.includes('timePeriod') && timePeriod) || '',
  ].join('_');
}

function generateNameForAxisConfiguration(groupBy: string[], result: Result) {
  return groupBy
    .map(identifier => {
      switch (identifier) {
        case 'timePeriod':
          return `${result.year}_${result.timeIdentifier}`;
        default:
          return '';
      }
    })
    .join('_');
}

function getChartDataForAxis(
  dataForAxis: Result[],
  dataSet: ChartDataSet,
  groupBy: string[],
) {
  return dataForAxis.reduce<ChartDataB[]>(
    (r: ChartDataB[], result) => [
      ...r,
      {
        name: generateNameForAxisConfiguration(groupBy, result),
        [generateKeyFromAxisConfiguration(dataSet, groupBy)]:
          result.measures[dataSet.indicator] || 'NaN',
      },
    ],
    [],
  );
}

function combineChartData(
  chartDataForAxis: ChartDataB[],
  combinedChartData: ChartDataB[],
) {
  return chartDataForAxis.reduce(
    (newCombinedData, { name, ...valueData }) => {
      // find and remove the existing matching (by name) entry from the list of data, or create a new one empty one
      const existingDataIndex = newCombinedData.findIndex(
        axisValue => axisValue.name === name,
      );
      const [existingData] =
        existingDataIndex >= 0
          ? newCombinedData.splice(existingDataIndex, 1)
          : [{ name }];

      // put the new entry into the array with any existing and new values added to it
      return [
        ...newCombinedData,
        {
          ...existingData,
          ...valueData,
        },
      ];
    },
    [...combinedChartData],
  );
}

export function createDataForAxis(
  axisConfiguration: AxisConfigurationItem,
  results: Result[],
) {
  return axisConfiguration.dataSets.reduce<ChartDataB[]>(
    (combinedChartData, dataSet) => {
      const resultsForAxis = resultsForDataSet(results, dataSet);

      const chartDataForAxis = getChartDataForAxis(
        resultsForAxis,
        dataSet,
        axisConfiguration.groupBy,
      );

      return combineChartData(chartDataForAxis, combinedChartData);
    },
    [],
  );
}

export function getKeysForChart(chartData: ChartDataB[]) {
  return Array.from(
    chartData.reduce((setOfKeys, { name, ...values }) => {
      return new Set([...Array.from(setOfKeys), ...Object.keys(values)]);
    }, new Set<string>()),
  );
}
