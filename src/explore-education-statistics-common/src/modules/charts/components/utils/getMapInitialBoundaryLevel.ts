import { Chart } from '@common/services/types/blocks';
import isEqual from 'lodash/isEqual';
import orderMapLegendItems from './orderMapLegendItems';

export default function getMapInitialBoundaryLevel(
  charts: Chart[],
): number | undefined {
  const chart = charts[0];
  if (chart?.type !== 'map' || chart?.legend === undefined) return undefined;

  const { dataSet: firstDataSet } = orderMapLegendItems(chart.legend)[0];
  const firstDataSetConfig = chart.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstDataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? chart.boundaryLevel;
}
