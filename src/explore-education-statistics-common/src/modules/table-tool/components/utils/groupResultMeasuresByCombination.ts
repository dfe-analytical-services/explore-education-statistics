import {
  getIndicatorPath,
  MeasuresGroupedByDataSet,
} from '@common/modules/charts/util/groupResultMeasuresByDataSet';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import get from 'lodash/get';
import set from 'lodash/set';

/**
 * Group {@param results} and their measures by their
 * associated combination of filters at each level
 * of a nested dictionary.
 *
 * {@param excludedFilterIds} should be used to prevent filters
 * that have been stripped out of the table headers from being
 * included in the path. Otherwise, it will not be possible
 * to resolve all combinations to a result.
 *
 * This is convenient for larger result sets where iteration
 * can be really expensive. By grouping the results in such a
 * way, we can achieve O(1) time complexity.
 *
 * Note that we cannot specify a return type as it's not
 * possible to define one due to the dynamic nature of the result.
 */
export default function groupResultMeasuresByCombination(
  results: TableDataResult[],
  excludedFilterIds: string[] = [],
): Dictionary<unknown> {
  return results.reduce<MeasuresGroupedByDataSet>((acc, result) => {
    const { geographicLevel, filters, timePeriod, location, measures } = result;

    const path = getIndicatorPath({
      filters,
      timePeriod,
      indicator: '',
      location: location[geographicLevel]
        ? {
            value: location[geographicLevel].code,
            level: geographicLevel,
          }
        : undefined,
    });

    const filteredPath = path.filter(id => !excludedFilterIds.includes(id));
    const existing = get(acc, filteredPath);

    if (existing) {
      set(acc, filteredPath, {
        ...existing,
        ...measures,
      });
    } else {
      set(acc, filteredPath, measures);
    }

    return acc;
  }, {});
}
