import { Chart } from '@common/services/types/blocks';
import isEqual from 'lodash/isEqual';
import sortBy from 'lodash/sortBy';

export default function getMapInitialBoundaryLevel(
  charts: Chart[],
): number | undefined {
  if (charts.length === 0 || charts[0].type !== 'map') return undefined;
  const chart = charts[0];

  // data set options are ordered alphabetically based on their label text
  const firstDataSet = sortBy(chart.legend.items, 'label')[0].dataSet;
  const firstDataSetConfig = chart.map?.dataSetConfigs.find(({ dataSet }) => {
    return isEqual(dataSet, firstDataSet);
  });

  return firstDataSetConfig?.boundaryLevel ?? chart.boundaryLevel;
}
