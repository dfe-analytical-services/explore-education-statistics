import { AxisGroupBy } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

/**
 * Generate a key that aggregates all of the
 * filter/indicator options chosen for a data set.
 *
 * This is technical debt as it's an absolutely
 * horrid way to generate data set keys.
 *
 * For the time being, we have to keep this due
 * to chart labels stored in the backend using
 * this key format for their value.
 *
 * We will aim to phase this out ASAP.
 *
 * @deprecated
 */
export function generateDeprecatedDataSetKey(
  dataSet: DataSet,
  ignoreGroup?: AxisGroupBy,
) {
  const { indicator, filters = [], timePeriod } = {
    ...dataSet,
  };

  const joinedLocations = [
    '', // country
    '', // region
    '', // local authority district
    '', // local authority
  ];

  return [
    indicator,
    ...filters,
    ...joinedLocations,
    ignoreGroup !== 'timePeriod' ? timePeriod : '',
  ].join('_');
}

/**
 * Convert a deprecated data set key to a {@see DataSet}.
 *
 * We need this to provide compatibility with existing
 * data set keys that may be persisted in the database.
 *
 * @deprecated
 */
export function parseDeprecatedDataSetKey(key: string): DataSet {
  const [indicator, ...parts] = key.split('_');

  return {
    indicator,
    filters: parts.slice(0, -5),
  };
}
