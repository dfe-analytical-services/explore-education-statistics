import {
  AxisConfiguration,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { mapCategoricalDataColours } from '@common/modules/charts/util/chartUtils';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import getMapDataSetCategoryConfigs, {
  MapDataSetCategoryConfig,
} from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { uniq } from 'lodash';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';

export default function getMapDataSetConfigs({
  axisMajor,
  data,
  legend,
  mapDataSetConfigs,
  meta,
  options,
}: {
  axisMajor: AxisConfiguration;
  data: TableDataResult[];
  legend?: LegendConfiguration;
  mapDataSetConfigs?: MapDataSetConfig[];
  meta: FullTableMeta;
  options?: ChartOptions;
}): MapDataSetConfig[] {
  const dataSetCategories = createDataSetCategories({
    axisConfiguration: {
      ...axisMajor,
      groupBy: 'locations',
    },
    data,
    includeNonNumericData: true,
    meta,
  });

  const dataSetCategoryConfigs = getMapDataSetCategoryConfigs({
    dataSetCategories,
    dataSetConfigs: mapDataSetConfigs,
    legendItems: legend?.items ?? [],
    meta,
    deprecatedDataClassification: options?.dataClassification,
    deprecatedDataGroups: options?.dataGroups,
  });

  return dataSetCategoryConfigs.map(config => {
    const categoricalDataConfig = getCategoricalDataConfig({
      dataSetCategoryConfig: config,
      dataSetCategories: dataSetCategories.filter(
        category => category.dataSets[config.dataKey],
      ),
    });

    return {
      boundaryLevel: config.boundaryLevel,
      ...(categoricalDataConfig && { categoricalDataConfig }),
      dataSet: config.rawDataSet,
      dataSetKey: generateDataSetKey(config.rawDataSet),
      dataGrouping: config.dataGrouping,
    };
  });
}

function getCategoricalDataConfig({
  dataSetCategoryConfig,
  dataSetCategories,
}: {
  dataSetCategoryConfig: MapDataSetCategoryConfig;
  dataSetCategories: DataSetCategory[];
}) {
  if (dataSetCategoryConfig.categoricalDataConfig?.length) {
    return dataSetCategoryConfig.categoricalDataConfig;
  }

  const values = dataSetCategories.map(
    category => category.dataSets[dataSetCategoryConfig.dataKey]?.value,
  );

  const isCategoricalData = values.every(value => !Number.isFinite(value));

  return isCategoricalData
    ? uniq(values).map((value, i) => {
        return {
          colour:
            mapCategoricalDataColours[i] ??
            `#${Math.floor(Math.random() * 16777215)
              .toString(16)
              .padStart(6, '0')}`,
          value: value.toString(),
        };
      })
    : undefined;
}
