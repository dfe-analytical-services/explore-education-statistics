import {
  DataSet,
  DataSetCategory,
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
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
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
export default function getDataSetCategoryConfigs(
  dataSetCategories: DataSetCategory[],
  legendItems: LegendItem[],
  meta: FullTableMeta,
): DataSetCategoryConfig[] {
  const legendItemsByDataSet = keyBy(legendItems, item =>
    generateDataSetKey(item.dataSet),
  );

  const dataSets = dataSetCategories.flatMap(category => {
    const dataSetCategoryEntries = Object.entries(category.dataSets);

    return dataSetCategoryEntries.map(
      ([dataSetKey, dataSetCategory], index) => {
        const rawDataSet = JSON.parse(dataSetKey);
        const dataSet = expandDataSet(dataSetCategory.dataSet, meta);

        const matchingLegendItem = legendItemsByDataSet[dataSetKey];

        const legendItemConfig = matchingLegendItem
          ? omit(matchingLegendItem, 'dataSet')
          : undefined;

        const getDefaultConfig = (): LegendItemConfiguration => {
          return {
            label: generateDefaultDataSetLabel(dataSet, category.filter),
            colour: colours[index % colours.length],
          };
        };

        // Try to match a config from:
        // - a legend item config
        // - a data set config (for older charts where no legend items)
        // - a default config
        const config =
          legendItemConfig ??
          dataSetCategory.dataSet.config ??
          getDefaultConfig();

        return {
          config,
          dataSet,
          rawDataSet,
          dataKey: dataSetKey,
        };
      },
    );
  });

  return uniqBy(dataSets, dataSet => dataSet.dataKey);
}
