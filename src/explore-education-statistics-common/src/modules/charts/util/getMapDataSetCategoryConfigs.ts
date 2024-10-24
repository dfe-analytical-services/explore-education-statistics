import {
  DataGroupingConfig,
  DataGroupingType,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import getDataSetCategoryConfigs, {
  DataSetCategoryConfig,
  GetDataSetCategoryConfigsOptions,
} from '@common/modules/charts/util/getDataSetCategoryConfigs';
import keyBy from 'lodash/keyBy';

const defaultDataGrouping: DataGroupingConfig = {
  customGroups: [],
  numberOfGroups: 5,
  type: 'EqualIntervals',
};

export interface MapDataSetCategoryConfig extends DataSetCategoryConfig {
  boundaryLevel?: number;
  dataGrouping: DataGroupingConfig;
}

interface GetMapDataSetCategoryConfigsOptions
  extends GetDataSetCategoryConfigsOptions {
  dataSetConfigs?: MapDataSetConfig[];
  defaultBoundaryLevel?: number;
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
}

export default function getMapDataSetCategoryConfigs({
  dataSetConfigs = [],
  defaultBoundaryLevel,
  deprecatedDataClassification,
  deprecatedDataGroups,
  ...options
}: GetMapDataSetCategoryConfigsOptions): MapDataSetCategoryConfig[] {
  const dataSetConfigsByDataSet = keyBy(dataSetConfigs, item =>
    generateDataSetKey(item.dataSet),
  );

  const deprecatedGrouping: DataGroupingConfig | undefined =
    !dataSetConfigs?.length && deprecatedDataClassification
      ? {
          customGroups: [],
          numberOfGroups:
            deprecatedDataGroups ?? defaultDataGrouping.numberOfGroups,
          type: deprecatedDataClassification,
        }
      : undefined;

  const dataSetCategoryConfigs = getDataSetCategoryConfigs(options);

  return dataSetCategoryConfigs.map(dataSetCategoryConfig => {
    const dataSetConfig =
      dataSetConfigsByDataSet[dataSetCategoryConfig.dataKey];

    const dataGrouping = dataSetConfig?.dataGrouping ?? defaultDataGrouping;

    return {
      ...dataSetCategoryConfig,
      boundaryLevel: defaultBoundaryLevel ?? dataSetConfig?.boundaryLevel,
      dataGrouping: deprecatedGrouping ?? dataGrouping,
    };
  });
}
