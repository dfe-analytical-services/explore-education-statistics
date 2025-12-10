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

export interface DataSetCategoryConfig {
  config: LegendItemConfiguration;
  dataKey: string;
  dataSet: ExpandedDataSet;
  rawDataSet: DataSet;
}

/**
 * Get the data set configurations that are used to
 * style/modify how they look in the chart.
 */
export interface GetDataSetCategoryConfigsOptions {
  dataSetCategories: DataSetCategory[];
  groupByFilterGroups?: boolean;
  legendItems: LegendItem[];
  meta: FullTableMeta;
}

export default function getDataSetCategoryConfigs({
  dataSetCategories,
  groupByFilterGroups = false,
  legendItems,
  meta,
}: GetDataSetCategoryConfigsOptions): DataSetCategoryConfig[] {
  const legendItemsByDataSet = keyBy(legendItems, item =>
    generateDataSetKey(item.dataSet),
  );

  const dataSets = dataSetCategories.reduce<DataSetCategoryConfig[]>(
    (acc, category) => {
      Object.entries(category.dataSets).forEach(
        ([dataSetKey, dataSetValue]) => {
          acc.push(
            toLegendConfig({
              dataSetKey,
              dataSetValue,
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
  dataSetKey,
  filter,
  index,
  legendItemsByDataSet,
  meta,
}: {
  dataSetValue: {
    dataSet: DataSetConfiguration;
    value: number | string;
  };
  dataSetKey: string;
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
      colour: colours.map(colour => colour.value)[index % colours.length],
    };
  };

  // Try to match a config from:
  // - a legend item config
  // - a data set config (for older charts where no legend items)
  // - a default config
  const config =
    legendItemConfig ?? dataSetValue.dataSet.config ?? getDefaultConfig();

  return {
    config,
    dataSet,
    rawDataSet,
    dataKey: dataSetKey,
  };
}
