import {
  DataBlockData,
  DataBlockMetadata,
  Location,
  Result,
} from '@common/services/dataBlockService';
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
import { Dictionary } from '@common/types';
import difference from 'lodash/difference';
import React, { ReactNode } from 'react';
import {
  AxisDomain,
  Label,
  PositionType,
  ReferenceLine as RechartsReferenceLine,
  TooltipProps,
  XAxis,
  XAxisProps,
  YAxis,
  YAxisProps,
} from 'recharts';

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

export interface AxesConfiguration {
  major: AxisConfiguration;
  minor: AxisConfiguration;
}

export interface ChartProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  labels: Dictionary<DataSetConfiguration>;
  axes: AxesConfiguration;
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

function existAndCodesDoNotMatch(a?: Location, b?: Location) {
  return a !== undefined && b !== undefined && a.code !== b.code;
}

function filterResultsForDataSet(ds: ChartDataSet) {
  return (result: Result) => {
    // fail fast with the two things that are most likely to not match
    if (ds.indicator && !Object.keys(result.measures).includes(ds.indicator))
      return false;

    if (ds.filters) {
      if (difference(ds.filters, result.filters).length > 0) return false;
    }

    if (ds.location) {
      const { location } = result;

      if (existAndCodesDoNotMatch(location.country, ds.location.country))
        return false;

      if (existAndCodesDoNotMatch(location.region, ds.location.region))
        return false;

      if (
        existAndCodesDoNotMatch(
          location.localAuthorityDistrict,
          ds.location.localAuthorityDistrict,
        )
      )
        return false;

      if (
        existAndCodesDoNotMatch(
          location.localAuthority,
          ds.location.localAuthority,
        )
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
      if (result.location.localAuthorityDistrict)
        return `${result.location.localAuthorityDistrict.code}`;
      if (result.location.localAuthority)
        return `${result.location.localAuthority.code}`;

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

export function sortChartData(
  chartData: ChartDataB[],
  sortBy: string | undefined,
  sortAsc: boolean = false,
) {
  if (sortBy === undefined) return chartData;

  return [...chartData].sort(({ [sortBy]: sortByA }, { [sortBy]: sortByB }) => {
    if (sortByA !== undefined && sortByB !== undefined) {
      return sortAsc
        ? sortByA.localeCompare(sortByB)
        : sortByB.localeCompare(sortByA);
    }
    return 0;
  });
}

export function createDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: DataBlockMetadata,
) {
  if (axisConfiguration === undefined || results === undefined) return [];

  return axisConfiguration.dataSets.reduce<ChartDataB[]>(
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
  labels: Dictionary<DataSetConfiguration>,
): ChartDataB[] {
  const chartData: ChartDataB[] = createDataForAxis(
    axisConfiguration,
    results,
    meta,
  ).map(mapNameToNameLabel(labels, meta.timePeriods, meta.locations));

  return sortChartData(
    chartData,
    axisConfiguration.sortBy,
    axisConfiguration.sortAsc,
  );
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

const calculateMinMaxReduce = (
  { min, max }: { min: number; max: number },
  next: string,
) => {
  const nextValue = parseFloat(next);
  if (Number.isNaN(nextValue) && Number.isFinite(nextValue))
    return { min, max };

  return {
    min: nextValue < min ? nextValue : min,
    max: nextValue > max ? nextValue : max,
  };
};

export function calculateDataRange(chartData: ChartDataB[]) {
  // removing the 'name' variable from the object and just keeping the rest of the values
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const allValuesInData = chartData.reduce<string[]>(
    (all, { name, ...values }) => [...all, ...Object.values(values)], // eslint-disable-line
    [],
  );

  return allValuesInData.reduce(calculateMinMaxReduce, {
    min: +Infinity,
    max: -Infinity,
  });
}

const parseNumberOrDefault = (
  number: string | undefined,
  def: number,
): number => {
  const parsed = number === undefined ? undefined : Number.parseFloat(number);

  if (parsed === undefined || Number.isNaN(parsed)) return def;
  return parsed;
};

function calculateMinorTicks(
  config: string | undefined,
  min: number,
  max: number,
  spacing: string = '5',
): number[] | undefined {
  let spacingValue = +spacing;

  if (spacingValue <= 0) spacingValue = 1.0;
  if (
    Number.isNaN(min) ||
    Number.isNaN(max) ||
    !Number.isFinite(min) ||
    !Number.isFinite(max)
  )
    return undefined;

  if (config === 'custom') {
    const result = [];

    let [start, end] = [min, max];
    if (start > end) [start, end] = [end, start];

    for (let i = start; i < end; i += spacingValue) {
      result.push(parseFloat(i.toPrecision(10)));
    }

    result.push(max);

    return result;
  }

  if (config === 'startEnd') {
    return [min, max];
  }
  return undefined;
}

function calculateMajorTicks(
  config: string | undefined,
  categories: string[],
  min: number,
  max: number,
  spacing: string = '1',
): string[] | undefined {
  let spacingValue = parseInt(spacing, 10);

  if (spacingValue <= 0) spacingValue = 1.0;
  if (
    Number.isNaN(min) ||
    Number.isNaN(max) ||
    !Number.isFinite(min) ||
    !Number.isFinite(max)
  )
    return undefined;

  if (config === 'custom') {
    const result = [];

    let [start, end] = [min, max];
    if (start > end) [start, end] = [end, start];

    for (let i = start; i < end; i += spacingValue) {
      result.push(categories[i]);
    }

    result.push(categories[max]);

    return result;
  }

  if (config === 'startEnd') {
    return [categories[min], categories[max]];
  }
  return undefined;
}

export function GenerateMinorAxis(
  chartData: ChartDataB[],
  axis: AxisConfiguration,
) {
  const { min, max } = calculateDataRange(chartData);

  const axisMin = parseNumberOrDefault(axis.min, min);
  const axisMax = parseNumberOrDefault(axis.max, max);

  const domain: [AxisDomain, AxisDomain] = [axisMin, axisMax];

  const ticks = calculateMinorTicks(
    axis.tickConfig,
    axisMin,
    axisMax,
    axis.tickSpacing,
  );
  return { domain, ticks };
}

export function GenerateMajorAxis(
  chartData: ChartDataB[],
  axis: AxisConfiguration,
) {
  const majorAxisCateories = chartData.map(({ name }) => name);

  const min = parseNumberOrDefault(axis.min, 0);
  const max = parseNumberOrDefault(axis.max, majorAxisCateories.length - 1);

  const domain: [AxisDomain, AxisDomain] = [min, max];

  const ticks = calculateMajorTicks(
    axis.tickConfig,
    majorAxisCateories,
    min,
    max,
    axis.tickSpacing,
  );
  return { domain, ticks };
}

export const CustomToolTip = ({ active, payload, label }: TooltipProps) => {
  if (active) {
    return (
      <div className="graph-tooltip">
        <p>{label}</p>
        {payload &&
          payload
            .sort((a, b) => {
              if (typeof b.value === 'number' && typeof a.value === 'number') {
                return b.value - a.value;
              }

              return 0;
            })
            .map((_, index) => {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <p key={index}>
                  {`${payload[index].name} : ${payload[index].value}`}
                </p>
              );
            })}
      </div>
    );
  }

  return null;
};
