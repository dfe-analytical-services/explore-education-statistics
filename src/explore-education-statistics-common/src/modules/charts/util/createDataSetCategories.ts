import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  ChartData,
  DataSet,
  DataSetCategory,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  Filter,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary, Pair } from '@common/types';
import cartesian from '@common/utils/cartesian';
import parseNumber from '@common/utils/number/parseNumber';
import difference from 'lodash/difference';
import groupBy from 'lodash/groupBy';
import isEqual from 'lodash/isEqual';
import orderBy from 'lodash/orderBy';

/**
 * We use this form of a data set as it's
 * important for us to retain information
 * about what this data set's parent is.
 *
 * The parent is the original data set definition
 * that the user defined in their configuration.
 */
interface ChildDataSet {
  dataSet: DataSetConfiguration;
  parent: DataSetConfiguration;
}

function dedupeDataSets(dataSets: ChildDataSet[]): ChildDataSet[] {
  const groups = groupBy(dataSets, dataSet =>
    generateDataSetKey(dataSet.dataSet),
  );

  return Object.values(groups).reduce<ChildDataSet[]>(
    (acc, groupedDataSets) => {
      const bestMatchingDataSet = groupedDataSets.reduce(
        (prevDataSet, nextDataSet) => {
          if (nextDataSet.parent.timePeriod && nextDataSet.parent.location) {
            return nextDataSet;
          }

          if (nextDataSet.parent.timePeriod && !prevDataSet.parent.timePeriod) {
            return nextDataSet;
          }

          if (nextDataSet.parent.location && !prevDataSet.parent.location) {
            return nextDataSet;
          }

          return prevDataSet;
        },
      );

      if (bestMatchingDataSet) {
        acc.push(bestMatchingDataSet);
      }

      return acc;
    },
    [],
  );
}

/**
 * Get data sets from an {@param axisConfiguration}.
 *
 * As the user may not select every single option for
 * a data set, we have to expand out the complete set
 * of data sets with all possible combinations of
 * filters, locations, indicators and time periods
 * using the {@param meta}.
 *
 * These are represented as {@see ChildDataSet},
 * as we need to retain information on what the
 * original data sets were.
 */
function getChildDataSets(
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): ChildDataSet[] {
  const { dataSets } = axisConfiguration;

  return dataSets.flatMap(dataSet => {
    const filterTypes: Filter[][] = [
      !dataSet.location ? meta.locations : [],
      !dataSet.timePeriod ? meta.timePeriodRange : [],
    ].filter(filters => filters.length > 0);

    if (!filterTypes.length) {
      return {
        dataSet,
        parent: dataSet,
      };
    }

    // Could be slow to rebuild filter combinations
    // for every data, so a performance optimization
    // might be to pre-create the combinations.
    // For simplicity, we'll use the current implementation.
    const filterCombinations = cartesian(...filterTypes);

    return filterCombinations.map(filterCombination => {
      return filterCombination.reduce<ChildDataSet>(
        (childDataSet, filter) => {
          if (filter instanceof LocationFilter) {
            // eslint-disable-next-line no-param-reassign
            childDataSet.dataSet.location = LocationFilter.parseCompositeId(
              filter.id,
            );
          }

          if (filter instanceof TimePeriodFilter) {
            // eslint-disable-next-line no-param-reassign
            childDataSet.dataSet.timePeriod = filter.id;
          }

          return childDataSet;
        },
        {
          dataSet: {
            ...dataSet,
          },
          parent: dataSet,
        },
      );
    }, []);
  });
}

function getCategoryFilters(
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): Filter[] {
  const { groupBy: axisGroupBy } = axisConfiguration;

  let filters: Filter[] = [];

  switch (axisGroupBy) {
    case 'filters':
      filters = Object.values(meta.filters).flatMap(
        filterGroup => filterGroup.options,
      );
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

/**
 * Create a map of data sets to their respective key
 * so they can be used in the chart data.
 */
function createKeyedDataSets(
  dataSets: Pair<ChildDataSet, number>[],
  filter: Filter,
  axisConfiguration: AxisConfiguration,
): DataSetCategory['dataSets'] {
  return dataSets.reduce<DataSetCategory['dataSets']>(
    (acc, [childDataSet, value]) => {
      const { dataSet, parent } = childDataSet;

      let expandedParent: DataSet = {
        ...parent,
      };

      // This part is a little confusing as it's not
      // particularly clear why we're doing this.
      //
      // We're basically trying to figure out if a
      // data set should use it's parent (or original)
      // data set's key or not.
      //
      // The parent data set may not include the location
      // or time period, so we have to fill these in
      // using the current category's filter.
      // Failing to fill this in, we just fallback to
      // using the expanded data set with all of its
      // details filled out.
      if (filter instanceof LocationFilter) {
        expandedParent.location = {
          value: filter.value,
          level: filter.level,
        };
      } else if (filter instanceof TimePeriodFilter) {
        expandedParent.timePeriod = filter.value;
      } else {
        expandedParent = {
          ...dataSet,
        };
      }

      // If we're using the parent key, we don't bother
      // excluding the groupBy as we want to create a
      // unique data point in the chart.
      const key = isEqual(expandedParent, parent)
        ? generateDataSetKey(parent)
        : generateDataSetKey(dataSet, axisConfiguration.groupBy);

      acc[key] = {
        dataSet,
        value,
      };

      return acc;
    },
    {},
  );
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
      if (sortBy === 'name') {
        if (data.filter instanceof TimePeriodFilter) {
          return data.filter.order;
        }

        return data.filter.label;
      }

      return parseNumber(data.dataSets[sortBy]) ?? 0;
    },
    [sortAsc ? 'asc' : 'desc'],
  );
}

/**
 * Create chart data that has been categorised by
 * filters, locations, time periods or indicators.
 *
 * This is quite a complicated algorithm and has multiple
 * steps as we have to generate the complete set of data
 * based on the user-defined data set configurations.
 *
 * We currently do the following:
 *
 * 1. Expands data sets so that we have the complete set of
 *    data sets to form the chart's data points. This is
 *    analogous to cartesian combinations of table headers
 *    in table tool.
 *
 *    We structure these as {@see ChildDataSet} so that we
 *    maintain reference to the original 'parent' data
 *    set that the user configured.
 *
 * 2. De-duplicate the expanded data sets, so we should
 *    only have unique data sets. These data sets are then
 *    matched with the correct result from the data.
 *
 * 3. Categorise these data sets based on the axis `groupBy`
 *    configuration. These form the ticks of our major axis.
 *
 * 4. Create the data set keys that form the actual data
 *    points in our chart. This is quite a weird step as we
 *    need to know about the parent (or original) data set
 *    to figure out what the final data set should be.
 *
 * 5. Sort and filter out any invalid or empty categories.
 */
export default function createDataSetCategories(
  axisConfiguration: AxisConfiguration,
  results: TableDataResult[],
  meta: FullTableMeta,
): DataSetCategory[] {
  const categoryFilters = getCategoryFilters(axisConfiguration, meta);

  const childDataSets = getChildDataSets(axisConfiguration, meta);
  const dedupedChildDataSets = dedupeDataSets(childDataSets);

  const childDataSetsWithValues: Pair<
    ChildDataSet,
    number
  >[] = dedupedChildDataSets
    .map(childDataSet => {
      const matchingResult = results.find(result =>
        isResultForDataSet(result, childDataSet.dataSet),
      );

      const value = Number(
        matchingResult?.measures[childDataSet.dataSet.indicator],
      );

      return [childDataSet, value] as Pair<ChildDataSet, number>;
    })
    .filter(([, value]) => !Number.isNaN(value));

  const dataSetCategories = categoryFilters
    .map(filter => {
      const matchingDataSets = childDataSetsWithValues.filter(
        ([childDataSet]) => {
          const { dataSet } = childDataSet;

          switch (axisConfiguration.groupBy) {
            case 'locations':
              return (
                dataSet.location &&
                LocationFilter.createId(dataSet.location) === filter.id
              );
            case 'indicators':
              return dataSet.indicator === filter.id;
            case 'filters':
              return dataSet.filters.some(
                dataSetFilter => dataSetFilter === filter.id,
              );
            case 'timePeriod':
              return dataSet.timePeriod === filter.id;
            default:
              return false;
          }
        },
      );

      return {
        filter,
        dataSets: createKeyedDataSets(
          matchingDataSets,
          filter,
          axisConfiguration,
        ),
      };
    })
    .filter(category => Object.values(category.dataSets).length > 0);

  return sortDataSetCategories(dataSetCategories, axisConfiguration).slice(
    axisConfiguration.min ?? 0,
    (axisConfiguration.max ?? dataSetCategories.length) + 1,
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
