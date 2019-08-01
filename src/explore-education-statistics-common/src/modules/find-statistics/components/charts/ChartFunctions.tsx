import {
  Axis,
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartDataSet,
  ChartSymbol,
  ChartType,
  DataSetConfiguration,
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

export const colours: string[] = [
  '#4763a5',
  '#f5a450',
  '#005ea5',
  '#800080',
  '#C0C0C0',
];

export const symbols: ChartSymbol[] = [
  'circle',
  'square',
  'triangle',
  'cross',
  'star',
];

export function parseCondensedTimePeriodRange(
  range: string,
  separator: string = '/',
) {
  return [range.substring(0, 4), range.substring(4, 6)].join(separator);
}

export interface ChartProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  labels: Dictionary<DataSetConfiguration>;
  axes: Dictionary<AxisConfiguration>;
  height?: number;
  width?: number;
  legend?: 'none' | 'top' | 'bottom';
  legendHeight?: string;
}

export interface StackedBarProps extends ChartProps {
  stacked?: boolean;
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

export interface ChartCapabilities {
  dataSymbols: boolean;
  stackable: boolean;
  lineStyle: boolean;
  gridLines: boolean;
  canSize: boolean;
}

export interface ChartDefinition {
  type: ChartType;
  name: string;

  capabilities: ChartCapabilities;

  data: {
    type: string;
    title: string;
    entryCount: number | 'multiple';
    targetAxis: string;
  }[];

  axes: {
    id: string;
    title: string;
    type: AxisType;
    defaultDataType?: AxisGroupBy;
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

function filterResultsForDataSet(ds: ChartDataSet) {
  return (result: Result) => {
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
        location.country.code !== ds.location.country.code
      )
        return false;
      if (
        location.region &&
        ds.location.region &&
        location.region.code !== ds.location.region.code
      )
        return false;
      if (
        location.localAuthorityDistrict &&
        ds.location.localAuthorityDistrict &&
        location.localAuthorityDistrict.code !==
        ds.location.localAuthorityDistrict.code
      )
        return false;
      if (
        location.localAuthority &&
        ds.location.localAuthority &&
        location.localAuthority.code !== ds.location.localAuthority.code
      )
        return false;
    }

    if (ds.timePeriod) {
      if (ds.timePeriod !== result.timePeriod) return false;
    }

    return true;
  };
}

export interface ChartDataB {
  name: string;

  [key: string]: string;
}

export function generateKeyFromDataSet(
  dataSet: ChartDataSet,
  ignoringField?: AxisGroupBy,
) {
  const { indicator, filters, location, timePeriod } = {
    ...dataSet,
  };

  const dontIgnoreLocations = ignoringField !== 'locations';

  const joinedLocations = [
    (dontIgnoreLocations &&
      location &&
      location.country &&
      location.country.code) ||
    '',
    (dontIgnoreLocations &&
      location &&
      location.region &&
      location.region.code) ||
    '',
    (dontIgnoreLocations &&
      location &&
      location.localAuthorityDistrict &&
      location.localAuthorityDistrict.code) ||
    '',
    (dontIgnoreLocations &&
      location &&
      location.localAuthority &&
      location.localAuthority.code) ||
    '',
  ];

  return [
    indicator,
    ...(filters || []),

    ...joinedLocations,

    (ignoringField !== 'timePeriods' && timePeriod) || '',
  ].join('_');
}

function generateNameForAxisConfiguration(
  result: Result,
  groupBy?: AxisGroupBy,
): string {
  switch (groupBy) {
    case 'timePeriods':
      return result.timePeriod;
    case 'locations':

      if (result.location.localAuthorityDistrict) return `${result.location.localAuthorityDistrict.code}`;
      if (result.location.localAuthority) return `${result.location.localAuthority.code}`;

      return '';
    default:
      return '';
  }
}

function getChartDataForAxis(
  dataForAxis: Result[],
  dataSet: ChartDataSet,
  meta: DataBlockMetadata,
  groupBy?: AxisGroupBy,
) {
  const source = groupBy && meta[groupBy];

  const initialNames = source && Object.keys(source);

  if (initialNames === undefined || initialNames.length === 0) {
    throw new Error(
      'Invalid grouping specified for the data on the axis, unable to determine the groups',
    );
  }

  const nameDictionary: Dictionary<ChartDataB> = initialNames.reduce(
    (chartdata, n) => ({ ...chartdata, [n]: { name: n } }),
    {},
  );

  return Object.values(
    dataForAxis.reduce<Dictionary<ChartDataB>>((r, result) => {
      const name = generateNameForAxisConfiguration(result, groupBy);

      return {
        ...r,
        [name]: {
          name,
          [generateKeyFromDataSet(dataSet, groupBy)]:
          result.measures[dataSet.indicator] || 'NaN',
        },
      };
    }, nameDictionary),
  );
}

function reduceCombineChartData(
  newCombinedData: ChartDataB[],
  { name, ...valueData }: { name: string },
) {
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
}

export function sortChartData(chartData: ChartDataB[], sortBy: string | undefined, sortAsc: boolean = false) {
  if (sortBy !== undefined) {

    const sortedArray = [...chartData];
    console.log(sortedArray);
    return sortedArray.sort(({ [sortBy]: sortByA }, { [sortBy]: sortByB }) => {

      if (sortByA !== undefined && sortByB !== undefined) {
        return sortAsc ?
          sortByA.localeCompare(sortByB)
          : sortByB.localeCompare(sortByA)
        ;
      }
      return 0;

    });

  }

  return chartData;
}

export function createDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: DataBlockMetadata,
) {
  if (axisConfiguration === undefined || results === undefined) return [];

  return  axisConfiguration.dataSets.reduce<ChartDataB[]>(
    (combinedChartData, dataSetForAxisConfiguration) => {
      return getChartDataForAxis(
        results.filter(filterResultsForDataSet(dataSetForAxisConfiguration)),
        dataSetForAxisConfiguration,
        meta,
        axisConfiguration.groupBy,
      ).reduce(reduceCombineChartData, [...combinedChartData]);
    },
    [],
  );

}

const FindFirstInDictionaries = (
  metaDataObjects: (Dictionary<DataSetConfiguration> | undefined)[],
  name: string,
) => (result: string | undefined, meta?: Dictionary<DataSetConfiguration>) =>
  result || (meta && meta[name] && meta[name].label);


export function mapNameToNameLabel(
  ...metaDataObjects: (Dictionary<DataSetConfiguration> | undefined)[]
) {
  return ({ name, ...otherdata }: { name: string }) => ({
    ...otherdata,
    name:
      metaDataObjects.reduce(
        FindFirstInDictionaries(metaDataObjects, name),
        '',
      ) || name,
  });
}

export function createSortedAndMappedDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: DataBlockMetadata,
  labels: Dictionary<DataSetConfiguration>
) : ChartDataB[] {

  const chartData: ChartDataB[] =
    createDataForAxis(
      axisConfiguration,
      results,
      meta,
    ).map(mapNameToNameLabel(labels, meta.timePeriods, meta.locations));

  return sortChartData(chartData, axisConfiguration.sortBy, axisConfiguration.sortAsc);
}

export function getKeysForChart(chartData: ChartDataB[]) {
  return Array.from(
    chartData.reduce((setOfKeys, { name: _, ...values }) => {
      return new Set([...Array.from(setOfKeys), ...Object.keys(values)]);
    }, new Set<string>()),
  );
}



export function populateDefaultChartProps(
  name: string,
  config: DataSetConfiguration,
) {
  return {
    dataKey: name,
    isAnimationActive: false,
    name: (config && config.label) || name,
    stroke: config && config.colour,
    fill: config && config.colour,
    unit: (config && config.unit) || '',
  };
}

export const conditionallyAdd = (size?: string, add?: number) => {
  if (size) {
    return +size + (add !== undefined ? add : 0);
  }
  return add;
};

