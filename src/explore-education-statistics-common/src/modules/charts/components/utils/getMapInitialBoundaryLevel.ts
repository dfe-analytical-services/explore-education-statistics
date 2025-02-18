import isEqual from 'lodash/isEqual';
import { MapChart } from '../../types/chart';
import orderMapLegendItems from './orderMapLegendItems';

export default function getMapInitialBoundaryLevel(chart: MapChart): number {
  const orderedLegends = orderMapLegendItems(chart.legend);
  const firstLegend = orderedLegends[0];
  if (!firstLegend) {
    return chart.boundaryLevel;
  }

  const firstDataSetConfig = chart.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstLegend.dataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? chart.boundaryLevel;
}
