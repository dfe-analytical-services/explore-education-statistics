import {
  DataSet,
  DataSetCategory,
  DataSetConfiguration,
  ExpandedDataSet,
} from '@common/modules/charts/types/dataSet';
import {
  LegendItem,
  LegendItemConfiguration,
} from '@common/modules/charts/types/legend';
import {
  DataGroupingConfig,
  DataGroupingType,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { colours } from '@common/modules/charts/util/chartUtils';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import { Filter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import keyBy from 'lodash/keyBy';
import omit from 'lodash/omit';
import uniqBy from 'lodash/uniqBy';

const defaultDataGrouping: DataGroupingConfig = {
  customGroups: [],
  numberOfGroups: 5,
  type: 'EqualIntervals',
};

export interface DataSetCategoryConfig {
  config: LegendItemConfiguration;
  dataKey: string;
  dataSet: ExpandedDataSet;
  rawDataSet: DataSet;
  dataGrouping: DataGroupingConfig;
}

/**
 * Get the data set configurations that are used to
 * style/modify how they look in the chart.
 */
interface Options {
  dataSetCategories: DataSetCategory[];
  dataSetConfigs?: MapDataSetConfig[];
  /**
   * Data classification and data groups are now in `dataSetConfigs`
   * as they are per data set instead of for all data sets (EES-3858).
   * The deprecated versions are to retain backwards compatibility
   * with maps created before this change.
   * @deprecated
   */
  deprecatedDataClassification?: DataGroupingType;
  /**
   * @deprecated
   */
  deprecatedDataGroups?: number;
  groupByFilterGroups?: boolean;
  legendItems: LegendItem[];
  meta: FullTableMeta;
}

export default function getDataSetCategoryConfigs({
  dataSetCategories,
  dataSetConfigs,
  deprecatedDataClassification,
  deprecatedDataGroups,
  groupByFilterGroups = false,
  legendItems,
  meta,
}: Options): DataSetCategoryConfig[] {
  const legendItemsByDataSet = keyBy(legendItems, item =>
    generateDataSetKey(item.dataSet),
  );

  const dataSetConfigsByDataSet = keyBy(dataSetConfigs, item =>
    generateDataSetKey(item.dataSet),
  );

  const deprecatedGrouping: DataGroupingConfig | undefined =
    !dataSetConfigs?.length && deprecatedDataClassification
      ? {
          customGroups: [],
          numberOfGroups: deprecatedDataGroups ?? 5,
          type: deprecatedDataClassification,
        }
      : undefined;

  const dataSets = dataSetCategories.reduce<DataSetCategoryConfig[]>(
    (acc, category) => {
      Object.entries(category.dataSets).forEach(
        ([dataSetKey, dataSetValue]) => {
          acc.push(
            toLegendConfig({
              dataSetConfigsByDataSet,
              dataSetKey,
              dataSetValue,
              deprecatedGrouping,
              filter: groupByFilterGroups ? undefined : category.filter,
              index: acc.length,
              legendItemsByDataSet,
              meta,
            }),
          );
        },
      );

      return acc;
    },
    [],
  );

  return uniqBy(dataSets, dataSet => dataSet.dataKey);
}

function toLegendConfig({
  dataSetValue,
  dataSetConfigsByDataSet,
  dataSetKey,
  deprecatedGrouping,
  filter,
  index,
  legendItemsByDataSet,
  meta,
}: {
  dataSetValue: {
    dataSet: DataSetConfiguration;
    value: number;
  };
  dataSetConfigsByDataSet: Dictionary<MapDataSetConfig>;
  dataSetKey: string;
  deprecatedGrouping?: DataGroupingConfig;
  filter?: Filter;
  index: number;
  legendItemsByDataSet: Dictionary<LegendItem>;
  meta: FullTableMeta;
}): DataSetCategoryConfig {
  const rawDataSet = JSON.parse(dataSetKey);

  const dataSet = expandDataSet(dataSetValue.dataSet, meta);

  const matchingLegendItem = legendItemsByDataSet[dataSetKey];

  const legendItemConfig = matchingLegendItem
    ? omit(matchingLegendItem, 'dataSet')
    : undefined;

  const getDefaultConfig = (): LegendItemConfiguration => {
    return {
      label: generateDefaultDataSetLabel(dataSet, filter),
      colour: colours[index % colours.length],
    };
  };

  // Try to match a config from:
  // - a legend item config
  // - a data set config (for older charts where no legend items)
  // - a default config
  const config =
    legendItemConfig ?? dataSetValue.dataSet.config ?? getDefaultConfig();

  const dataGrouping =
    dataSetConfigsByDataSet[dataSetKey]?.dataGrouping ?? defaultDataGrouping;

  return {
    config,
    dataSet,
    rawDataSet,
    dataKey: dataSetKey,
    dataGrouping: deprecatedGrouping ?? dataGrouping,
  };
}
