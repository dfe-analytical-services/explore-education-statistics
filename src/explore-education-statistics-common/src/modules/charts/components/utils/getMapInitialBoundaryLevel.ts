import isEqual from 'lodash/isEqual';
import { MapChart } from '../../types/chart';
import orderMapLegendItems from './orderMapLegendItems';

export default function getMapInitialBoundaryLevel(
  map: MapChart,
): number | undefined {
  const { dataSet: firstDataSet } = orderMapLegendItems(map.legend)[0];
  const firstDataSetConfig = map.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstDataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? map.boundaryLevel;
}
