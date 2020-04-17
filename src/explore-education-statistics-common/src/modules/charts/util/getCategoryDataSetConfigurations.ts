import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  DataSetCategory,
  DataSetConfiguration,
  ExpandedDataSet,
} from '@common/modules/charts/types/dataSet';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDefaultDataSetLabel from '@common/modules/charts/util/generateDefaultDataSetLabel';
import {
  Filter,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { OmitStrict, Pair } from '@common/types';
import generateHslColour from '@common/utils/colour/generateHslColour';
import { maxBy } from 'lodash';
import uniqBy from 'lodash/uniqBy';

export interface CategoryDataSetConfiguration
  extends OmitStrict<DataSetConfiguration, 'dataSet'> {
  dataKey: string;
  dataSet: ExpandedDataSet;
  filter: Filter;
}

/**
 * Get the data set configurations that are used to
 * style/modify how they look in the chart.
 */
export default function getCategoryDataSetConfigurations(
  chartCategories: DataSetCategory[],
  labelConfigs: DataSetConfiguration[],
  axisConfiguration: AxisConfiguration,
  meta: FullTableMeta,
): CategoryDataSetConfiguration[] {
  const dataSets = chartCategories.flatMap(category => {
    return Object.entries(category.dataSets).map(([dataSetKey]) => {
      const dataSet = JSON.parse(dataSetKey);

      // Label configurations may have missing details like
      // location/time period. We sort these configurations
      // by how well they match the data set so that we can
      // set sensible defaults if things are missing.
      const matchingConfigs = labelConfigs
        .filter(
          config =>
            config.dataSet.filters.every(filter =>
              dataSet.filters.includes(filter),
            ) && config.dataSet.indicator === dataSet.indicator,
        )
        .map<Pair<DataSetConfiguration, boolean>>(config => {
          const hasMatchingLocation =
            !!config.dataSet.location &&
            !!dataSet.location &&
            LocationFilter.createId(config.dataSet.location) ===
              LocationFilter.createId(dataSet.location);

          return [
            config,
            hasMatchingLocation &&
              config.dataSet.timePeriod === dataSet.timePeriod,
          ];
        });

      const [matchedConfig, isExactMatch] = maxBy(
        matchingConfigs,
        ([, matches]) => matches,
      ) ?? [undefined, 0];

      const expandedDataSet = expandDataSet(dataSet, meta);

      return {
        ...matchedConfig,
        // Don't need an exactly matching config as duplicate
        // colours aren't necessarily confusing to the user.
        colour: matchedConfig
          ? matchedConfig.colour
          : generateHslColour(JSON.stringify(dataSet)),
        // Config needs to match exactly otherwise we
        // can get confusing duplicate labels.
        label:
          matchedConfig && isExactMatch
            ? matchedConfig.label
            : generateDefaultDataSetLabel(
                expandedDataSet,
                axisConfiguration.groupBy,
              ),
        dataSet: expandedDataSet,
        filter: category.filter,
        dataKey: dataSetKey,
      } as CategoryDataSetConfiguration;
    });
  });

  return uniqBy(dataSets, dataSet => dataSet.dataKey);
}
