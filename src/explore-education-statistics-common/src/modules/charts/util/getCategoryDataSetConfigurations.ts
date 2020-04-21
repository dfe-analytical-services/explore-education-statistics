import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  DataSetCategory,
  DataSetConfigurationOptions,
  ExpandedDataSet,
} from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import { Filter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import isEqual from 'lodash/isEqual';
import uniqBy from 'lodash/uniqBy';
import orderBy from 'lodash/orderBy';

export interface CategoryDataSetConfiguration {
  category: Filter;
  config: DataSetConfigurationOptions;
  dataKey: string;
  dataSet: ExpandedDataSet;
}

/**
 * Get the data set configurations that are used to
 * style/modify how they look in the chart.
 */
export default function getCategoryDataSetConfigurations(
  chartCategories: DataSetCategory[],
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): CategoryDataSetConfiguration[] {
  const dataSets = chartCategories.flatMap(category => {
    return orderBy(
      Object.entries(category.dataSets),
      ([, dataSet]) => {
        return dataSet.value;
      },
      ['asc'],
    ).map<CategoryDataSetConfiguration>(([dataSetKey, dataSetConfig]) => {
      const { config, ...fullDataSet } = dataSetConfig.dataSet;
      const dataSet = JSON.parse(dataSetKey);

      const expandedDataSet = expandDataSet(dataSetConfig.dataSet, meta);

      return {
        ...fullDataSet,
        dataSet: expandedDataSet,
        config: {
          ...config,
          // Data sets needs to match exactly otherwise
          // we can get confusing duplicate labels.
          label: isEqual(dataSet, fullDataSet)
            ? config.label
            : generateDefaultDataSetLabel(
                expandedDataSet,
                axisConfiguration.groupBy,
              ),
        },
        category: category.filter,
        dataKey: dataSetKey,
      };
    });
  });

  return uniqBy(dataSets, dataSet => dataSet.dataKey);
}
