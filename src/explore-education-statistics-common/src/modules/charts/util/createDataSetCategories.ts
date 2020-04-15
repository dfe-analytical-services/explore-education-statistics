import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  ChartData,
  DataSet,
  DataSetCategory,
} from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary, Pair } from '@common/types';
import cartesian from '@common/utils/cartesian';
import parseNumber from '@common/utils/number/parseNumber';
import difference from 'lodash/difference';
import orderBy from 'lodash/orderBy';

/**
 * Get data sets expanded from an {@param axisConfiguration}.
 *
 * As the user may not select every single option for
 * a data set, we have to expand out the complete set
 * of data sets with all possible combinations of
 * filters, locations, indicators and time periods
 * using the {@param meta}.
 */
function getDataSets(
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): DataSet[] {
  const { dataSets } = axisConfiguration;

  const filterTypes: Filter[][] = [meta.locations, meta.timePeriodRange].filter(
    filters => filters.length >= 1,
  );

  if (!filterTypes.length) {
    return dataSets;
  }

  // Create new data set combinations for time periods and
  // locations as the user may not have defined these.
  const filterCombinations = cartesian<Filter>(...filterTypes);

  const locationTimePeriodDataSets = filterCombinations.reduce<
    Partial<DataSet>[]
  >((acc, filters) => {
    const newDataSet = filters.reduce<Partial<DataSet>>((dataSet, filter) => {
      if (filter instanceof LocationFilter) {
        // eslint-disable-next-line no-param-reassign
        dataSet.location = LocationFilter.parseCompositeId(filter.id);
      }

      if (filter instanceof TimePeriodFilter) {
        // eslint-disable-next-line no-param-reassign
        dataSet.timePeriod = filter.id;
      }

      return dataSet;
    }, {});

    acc.push(newDataSet);

    return acc;
  }, []);

  return dataSets.reduce<DataSet[]>((acc, dataSet) => {
    locationTimePeriodDataSets.forEach(locationTimePeriodDataSet => {
      acc.push({
        ...dataSet,
        ...locationTimePeriodDataSet,
      });
    });

    return acc;
  }, []);
}

function getCategoryFilters(
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): Filter[] {
  const { groupBy: axisGroupBy } = axisConfiguration;

  let filters: Filter[] = [];

  switch (axisGroupBy) {
    case 'filters':
      filters = Object.values(meta.filters).flat();
      break;
    case 'timePeriod':
      filters = meta.timePeriodRange;
      break;
    case 'indicators':
    case 'locations':
      filters = meta[axisGroupBy];
      break;
    default:
      throw new Error(
        `Cannot get categories for invalid group: '${axisGroupBy}'`,
      );
  }

  if (filters.length === 0) {
    throw new Error(`Need at least one category for group: '${axisGroupBy}'`);
  }

  return filters;
}

function isResultForDataSet(
  result: TableDataResult,
  dataSet: DataSet,
): boolean {
  if (typeof result.measures[dataSet.indicator] === 'undefined') {
    return false;
  }

  if (difference(dataSet.filters, result.filters).length !== 0) {
    return false;
  }

  if (
    dataSet.location &&
    dataSet.location.value !== result.location?.[dataSet.location.level]?.code
  ) {
    return false;
  }

  if (dataSet.timePeriod && dataSet.timePeriod !== result.timePeriod) {
    return false;
  }

  return true;
}

function sortDataSetCategories(
  dataSetCategories: DataSetCategory[],
  axisConfiguration: AxisConfiguration,
): DataSetCategory[] {
  const { sortBy, sortAsc } = axisConfiguration;

  if (!sortBy) {
    return dataSetCategories;
  }

  return orderBy(
    dataSetCategories,
    data => {
      return parseNumber(data.dataSets[sortBy]) ?? 0;
    },
    [sortAsc ? 'asc' : 'desc'],
  );
}

/**
 * Create chart data that has been categorised
 * by filters, locations, time periods or indicators.
 */
export default function createDataSetCategories(
  axisConfiguration: AxisConfiguration,
  results: TableDataResult[],
  meta: FullTableMeta,
): DataSetCategory[] {
  const categoryFilters = getCategoryFilters(axisConfiguration, meta);

  const dataSetsWithValues: Pair<DataSet, number>[] = getDataSets(
    axisConfiguration,
    meta,
  )
    .map(dataSet => {
      const matchingResult = results.find(result =>
        isResultForDataSet(result, dataSet),
      );

      const value = Number(matchingResult?.measures[dataSet.indicator]);

      return [dataSet, value] as Pair<DataSet, number>;
    })
    .filter(([, value]) => !Number.isNaN(value));

  const dataSetCategories = categoryFilters
    .map(filter => {
      const dataSets = dataSetsWithValues
        .filter(([dataSet]) => {
          switch (filter.constructor) {
            case LocationFilter:
              return (
                dataSet.location &&
                LocationFilter.createId(dataSet.location) === filter.id
              );
            case Indicator:
              return dataSet.indicator === filter.id;
            case CategoryFilter:
              return dataSet.filters.some(
                dataSetFilter => dataSetFilter === filter.id,
              );
            case TimePeriodFilter:
              return dataSet.timePeriod === filter.id;
            default:
              return false;
          }
        })
        .reduce<Dictionary<{ dataSet: DataSet; value: number }>>(
          (acc, [dataSet, value]) => {
            const key = generateDataSetKey(dataSet, axisConfiguration.groupBy);
            acc[key] = {
              dataSet,
              value,
            };

            return acc;
          },
          {},
        );

      return {
        filter,
        dataSets,
      };
    })
    .filter(category => Object.values(category.dataSets).length > 0);

  return sortDataSetCategories(dataSetCategories, axisConfiguration).slice(
    axisConfiguration.min ?? 0,
    axisConfiguration.max ?? dataSetCategories.length,
  );
}

export const toChartData = (chartCategory: DataSetCategory): ChartData => {
  const dataSets = Object.entries(chartCategory.dataSets).reduce<
    Dictionary<number>
  >((acc, [key, dataSet]) => {
    acc[key] = dataSet.value;

    return acc;
  }, {});

  return {
    ...dataSets,
    name: chartCategory.filter.label,
  };
};
