import { AxisGroupBy } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

export default function generateDataSetKey(
  dataSet: DataSet,
  excludeGroup?: AxisGroupBy,
): string {
  const filters = (excludeGroup !== 'filters'
    ? [...dataSet.filters]
    : []
  ).sort();

  const location =
    excludeGroup !== 'locations' && dataSet.location
      ? {
          level: dataSet.location.level,
          value: dataSet.location.value,
        }
      : undefined;

  const timePeriod =
    excludeGroup !== 'timePeriod' ? dataSet.timePeriod : undefined;

  return JSON.stringify({
    filters,
    indicator: dataSet.indicator,
    location,
    timePeriod,
  });
}
