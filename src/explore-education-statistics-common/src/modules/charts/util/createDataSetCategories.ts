import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  ChartData,
  DataSet,
  DataSetCategory,
} from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import groupResultMeasuresByDataSet, {
  getIndicatorPath,
} from '@common/modules/table-tool/utils/groupResultMeasuresByDataSet';
import {
  Filter,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary, Pair } from '@common/types';
import cartesian from '@common/utils/cartesian';
import naturalOrderBy from '@common/utils/array/naturalOrderBy';
import get from 'lodash/get';
import groupBy from 'lodash/groupBy';
import sortBy from 'lodash/sortBy';
import uniq from 'lodash/uniq';

/**
 * We use this form of a data set as it's
 * important for us to retain information
 * about what this data set's parent is.
 *
 * The parent is the original data set definition
 * that the user defined in their configuration.
 */
interface ChildDataSet {
  dataSet: DataSet;
  parent: DataSet;
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
  const { groupBy: axisGroupBy, groupByFilter } = axisConfiguration;

  let filters: Filter[] = [];

  switch (axisGroupBy) {
    case 'filters': {
      const filterGroups = Object.values(meta.filters);

      // We want to try and remove any filter groups with only
      // one filter as these don't really make sense to display
      // on the chart (this is similar to what we do in table tool).
      // Also take into account the specific filter category it's grouped by.
      // If grouped by filters and groupByFilter is empty then group by all filters.
      const filteredFilters = filterGroups
        .filter(filterGroup => {
          if (groupByFilter) {
            return (
              filterGroup.options.length > 1 &&
              filterGroup.name === groupByFilter
            );
          }
          return filterGroup.options.length > 1;
        })
        .flatMap(filterGroup => filterGroup.options);

      // If there are no filtered filters, we need to at least
      // try to show something otherwise the chart will just
      // break due to the major axis having no categories.
      filters =
        filteredFilters.length > 0
          ? filteredFilters
          : filterGroups.flatMap(filterGroup => filterGroup.options);
      break;
    }
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

/**
 * Create a map of data sets to their respective key
 * so they can be used in the chart data.
 */
function createKeyedDataSets(
  dataSets: Pair<ChildDataSet, number>[],
  filter: Filter,
): DataSetCategory['dataSets'] {
  return dataSets.reduce<DataSetCategory['dataSets']>(
    (acc, [childDataSet, value]) => {
      const { dataSet } = childDataSet;
      const key = generateDataSetKey(dataSet, filter);

      acc[key] = {
        dataSet,
        value,
      };

      return acc;
    },
    {},
  );
}

/**
 * Order the categories by the data set `order` property
 * or a natural ordering algorithm based on the label
 * (for backwards compatibility with older charts).
 */
function getSortedDataSetCategoryRange(
  dataSetCategories: DataSetCategory[],
  axisConfiguration: AxisConfiguration,
): DataSetCategory[] {
  const groupOrder = uniq(
    axisConfiguration.dataSets.flatMap(dataSet => {
      switch (axisConfiguration.groupBy) {
        case 'locations':
          return dataSet.location?.value;
        case 'indicators':
          return dataSet.indicator;
        case 'timePeriod':
          return dataSet.timePeriod;
        case 'filters':
          return dataSet.filters;
        default:
          return undefined;
      }
    }),
  );

  // Check if the data sets have an explicit order property.
  // Older charts don't have this property, so we need to
  // order them using the previous ordering algorithm.
  const hasDataSetOrder = axisConfiguration.dataSets.some(
    dataSet => typeof dataSet.order !== 'undefined',
  );

  const sortedDataSetCategories = hasDataSetOrder
    ? sortBy(dataSetCategories, category =>
        groupOrder.indexOf(category.filter.value),
      )
    : naturalOrderBy(dataSetCategories, data =>
        data.filter instanceof TimePeriodFilter
          ? data.filter.order
          : data.filter.label,
      );

  const sortedDataSetCategoriesRange = sortedDataSetCategories.slice(
    axisConfiguration.min ?? 0,
    (axisConfiguration.max ?? dataSetCategories.length) + 1,
  );

  // If no `sortAsc` has been set, we should default
  // to true as it's not really natural to sort in
  // descending order most of the time.
  return axisConfiguration.sortAsc ?? true
    ? sortedDataSetCategoriesRange
    : sortedDataSetCategoriesRange.reverse();
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
  includeNonNumericData = false,
): DataSetCategory[] {
  const categoryFilters = getCategoryFilters(axisConfiguration, meta);
  const childDataSets = getChildDataSets(axisConfiguration, meta);
  const dedupedChildDataSets = dedupeDataSets(childDataSets);

  // Group result measures by data set key first to
  // allow result lookups to be MUCH faster.
  const measuresByDataSet = groupResultMeasuresByDataSet(results);

  const childDataSetsWithValues = dedupedChildDataSets.reduce<
    Pair<ChildDataSet, number>[]
  >((acc, childDataSet) => {
    const { dataSet } = childDataSet;
    const rawValue = get(measuresByDataSet, getIndicatorPath(dataSet));
    const value = Number(rawValue);

    if (includeNonNumericData && rawValue) {
      acc.push([childDataSet, Number.isNaN(value) ? null : value] as Pair<
        ChildDataSet,
        number
      >);
      return acc;
    }

    if (!Number.isNaN(value)) {
      acc.push([childDataSet, value] as Pair<ChildDataSet, number>);
    }

    return acc;
  }, []);

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
        dataSets: createKeyedDataSets(matchingDataSets, filter),
      };
    })
    .filter(category => Object.values(category.dataSets).length > 0);

  return getSortedDataSetCategoryRange(dataSetCategories, axisConfiguration);
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
    name: chartCategory.filter.id,
  };
};
