import parseNumber from '@common/lib/utils/number/parseNumber';
import {
  AxisConfiguration,
  AxisGroupBy,
  ChartDataSet,
  ChartMetaData,
  ChartSymbol,
  DataSetConfiguration,
  LabelConfiguration,
} from '@common/modules/charts/types/chart';
import {
  FilterOption,
  PublicationSubjectMeta,
} from '@common/modules/table-tool/services/tableBuilderService';
import {
  DataBlockMetadata,
  Location,
  Result,
} from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import difference from 'lodash/difference';
import omit from 'lodash/omit';
import { AxisDomain } from 'recharts';

export const colours: string[] = [
  '#4763a5',
  '#f5a450',
  '#1d70b8',
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

export interface ChartData {
  name: string;
  [key: string]: string;
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

    (ignoringField !== 'timePeriod' && timePeriod) || '',
  ].join('_');
}

function generateNameForAxisConfiguration(
  result: Result,
  dataSet: ChartDataSet,
  groupBy?: AxisGroupBy,
): string {
  switch (groupBy) {
    case 'timePeriod':
      return result.timePeriod;
    case 'indicators':
      return `${dataSet.indicator}`;
    case 'filters':
      return generateKeyFromDataSet(
        { ...dataSet, filters: result.filters },
        groupBy,
      );
    case 'locations':
      if (
        result.location.localAuthorityDistrict &&
        result.location.localAuthorityDistrict.code &&
        result.location.localAuthorityDistrict.code !== ''
      )
        return `${result.location.localAuthorityDistrict.code}`;

      if (
        result.location.localAuthority &&
        result.location.localAuthority.code &&
        result.location.localAuthority.code !== ''
      )
        return `${result.location.localAuthority.code}`;

      if (
        result.location.region &&
        result.location.region.code &&
        result.location.region.code !== ''
      )
        return `${result.location.region.code}`;

      if (result.location.country) return `${result.location.country.code}`;

      return '';
    default:
      return '';
  }
}

function getChartDataForAxis(
  dataForAxis: Result[],
  dataSet: ChartDataSet,
  meta: ChartMetaData,
  groupBy?: AxisGroupBy,
) {
  const source = groupBy && meta[groupBy];

  const initialNames = source && Object.keys(source);

  if (initialNames === undefined || initialNames.length === 0) {
    throw new Error(
      'Invalid grouping specified for the data on the axis, unable to determine the groups',
    );
  }

  const nameDictionary: Dictionary<ChartData> = initialNames.reduce(
    (chartdata, n) => ({ ...chartdata, [n]: { name: n } }),
    {},
  );

  return Object.values(
    dataForAxis.reduce<Dictionary<ChartData>>((acc, result) => {
      const name = generateNameForAxisConfiguration(result, dataSet, groupBy);

      acc[name] = {
        [generateKeyFromDataSet(dataSet, groupBy)]: result.measures[
          dataSet.indicator
        ],
        name,
      } as ChartData;

      return acc;
    }, nameDictionary),
  );
}

function reduceCombineChartData(
  newCombinedData: ChartData[],
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
  chartData: ChartData[],
  sortBy: string | undefined,
  sortAsc: boolean,
) {
  if (sortBy === undefined) return chartData;

  const mappedValueAndData = chartData.map(data => ({
    value:
      data[sortBy] === undefined
        ? undefined
        : Number.parseFloat(data[sortBy] as string),
    data,
  }));

  return mappedValueAndData
    .sort(({ value: valueA }, { value: valueB }) => {
      if (valueA !== undefined && valueB !== undefined) {
        return sortAsc ? valueA - valueB : valueB - valueA;
      }
      return 0;
    })
    .map(({ data }) => data);
}

export function createDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: ChartMetaData,
) {
  if (axisConfiguration === undefined || results === undefined) return [];

  return axisConfiguration.dataSets.reduce<ChartData[]>(
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

export function pairFiltersByValue(
  filters: PublicationSubjectMeta['filters'],
): Dictionary<FilterOption> {
  return Object.values(filters)
    .flatMap(filter =>
      Object.values(filter.options).flatMap(filterGroup => filterGroup.options),
    )
    .reduce<Dictionary<FilterOption>>((acc, filter) => {
      acc[filter.value] = filter;
      return acc;
    }, {});
}

const findFirstInDictionaries = (
  metaDataObjects: (Dictionary<LabelConfiguration> | undefined)[],
  name: string,
) => (result: string | undefined, meta?: Dictionary<LabelConfiguration>) =>
  result || (meta && meta[name] && meta[name].label);

export function mapNameToNameLabel(
  keepOriginalValue = false,
  ...metaDataObjects: (Dictionary<LabelConfiguration> | undefined)[]
) {
  return ({ name, ...otherdata }: { name: string }) => ({
    ...(keepOriginalValue ? { __name: name } : {}),
    name:
      metaDataObjects.reduce(
        findFirstInDictionaries(metaDataObjects, name),
        '',
      ) || name,
    ...otherdata,
  });
}

export function createSortedDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: ChartMetaData,
  mapFunction: (data: ChartData) => ChartData = data => data,
): ChartData[] {
  const chartData: ChartData[] = createDataForAxis(
    axisConfiguration,
    results,
    meta,
  ).map(mapFunction);

  const sortedData = sortChartData(
    chartData,
    axisConfiguration.sortBy,
    axisConfiguration.sortAsc !== false,
  );

  return sortedData.slice(
    axisConfiguration.min ?? 0,
    axisConfiguration.max ?? sortedData.length,
  );
}

export function createSortedAndMappedDataForAxis(
  axisConfiguration: AxisConfiguration,
  results: Result[],
  meta: ChartMetaData,
  labels: Dictionary<DataSetConfiguration>,
  keepOriginalValue = false,
): ChartData[] {
  return createSortedDataForAxis(
    axisConfiguration,
    results,
    meta,
    mapNameToNameLabel(
      keepOriginalValue,
      labels,
      meta.timePeriod,
      meta.locations,
      pairFiltersByValue(meta.filters),
      meta.indicators,
    ),
  );
}

export function getKeysForChart(chartData: ChartData[]) {
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

export function calculateDataRange(chartData: ChartData[]) {
  const allValuesInData = chartData.reduce<string[]>((acc, data) => {
    // removing the 'name' variable from the object and just keeping the rest of the values
    acc.push(...(Object.values(omit(data, ['name'])) as string[]));

    return acc;
  }, []);

  return allValuesInData.reduce(calculateMinMaxReduce, {
    min: +Infinity,
    max: -Infinity,
  });
}

export function getNiceMaxValue(maxValue: number) {
  if (maxValue === 0) {
    return 0;
  }

  const maxIsLessThanOne = Math.abs(maxValue) < 1;
  let max = maxValue;
  if (maxIsLessThanOne) {
    max = maxValue * 100;
  }

  const numberOf0s = 10 ** Math.floor(Math.log10(Math.abs(max)));
  const maxReducedToLessThan10 = Math.ceil(max / numberOf0s);

  if (maxReducedToLessThan10 % 2 && maxReducedToLessThan10 % 5) {
    return maxIsLessThanOne
      ? ((maxReducedToLessThan10 + 1) * numberOf0s) / 100
      : (maxReducedToLessThan10 + 1) * numberOf0s;
  }
  return maxIsLessThanOne
    ? (maxReducedToLessThan10 * numberOf0s) / 100
    : maxReducedToLessThan10 * numberOf0s;
}

function calculateMinorTicks(
  config: string | undefined,
  min: number,
  max: number,
  spacing = 5,
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
    const minimumSpacingValue = getNiceMaxValue(
      Math.floor((Number(max) - Number(min)) / 100),
    );
    if (spacingValue < minimumSpacingValue) {
      spacingValue = minimumSpacingValue;
    }

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
  spacing = 1,
): string[] | undefined {
  let spacingValue = spacing;

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

export function generateMinorAxis(
  chartData: ChartData[],
  axis: AxisConfiguration,
) {
  const { min, max } = calculateDataRange(chartData);

  const axisMin = parseNumber(axis.min) ?? min;
  const axisMax = parseNumber(axis.max) ?? getNiceMaxValue(max);

  const domain: [AxisDomain, AxisDomain] = [axisMin, axisMax];

  const ticks = calculateMinorTicks(
    axis.tickConfig,
    axisMin,
    axisMax,
    axis.tickSpacing,
  );
  return { domain, ticks };
}

export function generateMajorAxis(
  chartData: ChartData[],
  axis: AxisConfiguration,
) {
  const majorAxisCategories = chartData.map(({ name }) => name);

  const min = parseNumber(axis.min) ?? 0;
  const max = parseNumber(axis.max) ?? majorAxisCategories.length - 1;

  const domain: [AxisDomain, AxisDomain] = [min, max];

  const ticks = calculateMajorTicks(
    axis.tickConfig,
    majorAxisCategories,
    min,
    max,
    axis.tickSpacing,
  );
  return { domain, ticks };
}

export function parseMetaData(
  metaData?: DataBlockMetadata,
): ChartMetaData | undefined {
  if (metaData === undefined) return undefined;

  return {
    filters: metaData.filters,
    indicators: metaData.indicators,
    locations: metaData.locations,
    boundaryLevels: metaData.boundaryLevels,
    timePeriod: Object.entries(metaData.timePeriod).reduce(
      (timePeriod, [value, data]) => ({
        ...timePeriod,
        [value]: {
          ...data,
          value,
        },
      }),
      {},
    ),
  };
}
