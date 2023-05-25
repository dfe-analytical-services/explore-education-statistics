import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import {
  getIndicatorPath,
  MeasuresGroupedByDataSet,
} from '@common/modules/table-tool/utils/groupResultMeasuresByDataSet';
import get from 'lodash/get';
import set from 'lodash/set';

/**
 * Group {@param tableDataResults} and their measures by their
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
  tableDataResults: TableDataResult[],
  excludedFilterIds: Set<string> = new Set(),
): Dictionary<unknown> {
  return tableDataResults.reduce<MeasuresGroupedByDataSet>((acc, result) => {
    const {
      geographicLevel,
      filters,
      timePeriod,
      locationId,
      location,
      measures,
    } = result;

    // Default to using the code in the legacy 'location' field that exists for historical Permalinks created prior to EES-2955.
    // Note that EES-3203 added 'locationId' to table results before EES-2955 switched over to using location id's
    // and removed 'location'.
    // The presence of a location id doesn't mean the code should be ignored.
    const locationCodeOrId =
      (location && location[geographicLevel]?.code) || locationId;

    const path = getIndicatorPath({
      filters,
      timePeriod,
      indicator: '',
      location: locationCodeOrId
        ? {
            value: locationCodeOrId,
            level: geographicLevel,
          }
        : undefined,
    });

    const filteredPath = path.filter(id => !excludedFilterIds.has(id));
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
