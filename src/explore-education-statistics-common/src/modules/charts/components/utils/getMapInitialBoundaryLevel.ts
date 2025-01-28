import { MapChartConfig } from '@common/modules/charts/types/chart';
import isEqual from 'lodash/isEqual';
import orderMapLegendItems from './orderMapLegendItems';

export default function getMapInitialBoundaryLevel(
  map: MapChartConfig,
): number | undefined {
  const { dataSet: firstDataSet } = orderMapLegendItems(map.legend)[0];
  const firstDataSetConfig = map.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstDataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? map.boundaryLevel;
}
