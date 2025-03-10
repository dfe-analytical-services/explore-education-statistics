import isEqual from 'lodash/isEqual';
import { MapChart } from '../../types/chart';

export default function getMapInitialBoundaryLevel(chart: MapChart): number {
  const firstLegend = chart.legend.items[0];
  if (!firstLegend) {
    return chart.boundaryLevel;
  }

  const firstDataSetConfig = chart.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstLegend.dataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? chart.boundaryLevel;
}
