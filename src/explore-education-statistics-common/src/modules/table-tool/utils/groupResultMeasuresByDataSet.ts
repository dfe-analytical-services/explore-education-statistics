import { DataSet } from '@common/modules/charts/types/dataSet';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { TableDataResult } from '@common/services/tableBuilderService';
import compact from 'lodash/compact';
import get from 'lodash/get';
import set from 'lodash/set';

export function getIndicatorPath(dataSet: DataSet): string[] {
  const { filters, timePeriod, location, indicator } = dataSet;

  const locationKey = location ? LocationFilter.createId(location) : undefined;
  const sortedFilters = [...filters].sort();

  return compact([locationKey, timePeriod, ...sortedFilters, indicator]);
}

export interface MeasuresGroupedByDataSet {
  [location: string]: {
    // This will nest the more filters there are, so it's
    // not possible to know the data structure beyond this.
    [timePeriod: string]: unknown;
  };
}

/**
 * Group {@param results} and their measures by their
 * associated data set (excluding the indicator).
 *
 * This is convenient for larger result sets where iteration
 * can be really expensive. By converting the results to
 * a dictionary, we can achieve O(1) time complexity.
 */
export default function groupResultMeasuresByDataSet(
  results: TableDataResult[],
): MeasuresGroupedByDataSet {
  return results.reduce<MeasuresGroupedByDataSet>((acc, result) => {
    const { geographicLevel, filters, timePeriod, locationId, measures } =
      result;

    const path = getIndicatorPath({
      filters,
      timePeriod,
      indicator: '',
      location: locationId
        ? {
            value: locationId,
            level: geographicLevel,
          }
        : undefined,
    });

    const existing = get(acc, path);

    if (existing) {
      set(acc, path, {
        ...existing,
        ...measures,
      });
    } else {
      set(acc, path, measures);
    }

    return acc;
  }, {});
}
