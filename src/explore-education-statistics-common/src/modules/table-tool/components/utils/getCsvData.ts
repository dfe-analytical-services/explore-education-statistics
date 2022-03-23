import {
  WorkerCategoryFilter,
  WorkerFilter,
  WorkerLocationFilter,
  WorkerTimePeriodFilter,
  WorkerFullTable,
} from '@common/modules/table-tool/types/workerFullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import cartesian from '@common/utils/cartesian';

const EMPTY_CELL_TEXT = 'no data';

/**
 * Gets the CSV data for download.
 *
 * This is used in the Web Worker so uses a Worker version of
 * the {@see FullTable} type {@see WorkerFullTable}.
 * When {@see mapFullTable} is refactored to not use classes (EES-2613)
 * we can change this to use the standard {@see FullTable} type.
 *
 * IMPORTANT: the filters ids are getters which aren't serialised when
 * passed to the worker so cannot be used here.
 */
export default function getCsvData(fullTable: WorkerFullTable): string[][] {
  const { subjectMeta, results } = fullTable;
  const { indicators, filters, timePeriodRange, locations } = subjectMeta;

  const filterColumns = Object.values(filters).map(
    filterGroup => filterGroup.name,
  );

  const indicatorColumns = indicators.map(indicator => indicator.name);

  const columns = [
    'location',
    'location_code',
    'geographic_level',
    'time_period',
    ...filterColumns,
    ...indicatorColumns,
  ];

  const resultMatchesLocationFilter = (
    result: TableDataResult,
    location: WorkerLocationFilter,
  ) => {
    // Attempt to match on the legacy 'location' field that exists in table results
    // of historical Permalinks created prior to EES-2955.
    if (result.location) {
      const { geographicLevel } = result;
      return (
        result.location[geographicLevel]?.code === location.value &&
        geographicLevel === location.level
      );
    }

    return result.locationId === location.value;
  };

  const rows = cartesian<WorkerFilter>(
    locations,
    timePeriodRange,
    ...Object.values(filters).map(filterGroup => filterGroup.options),
  )
    .map(filterCombination => {
      const [location, timePeriod, ...filterOptions] = filterCombination as [
        WorkerLocationFilter,
        WorkerTimePeriodFilter,
        ...WorkerCategoryFilter[]
      ];

      const indicatorCells = indicators.map(indicator => {
        const matchingResult = results.find(result => {
          return (
            filterOptions.every(filter =>
              result.filters.includes(filter.value),
            ) &&
            resultMatchesLocationFilter(result, location) &&
            result.timePeriod === timePeriod.value
          );
        });

        if (!matchingResult) {
          return EMPTY_CELL_TEXT;
        }

        return matchingResult.measures[indicator.value] ?? EMPTY_CELL_TEXT;
      });

      if (indicatorCells.every(cell => cell === EMPTY_CELL_TEXT)) {
        return [];
      }

      return [
        location.label,
        location.code,
        location.level,
        timePeriod.label.replace(/\//g, ''),
        ...filterOptions.map(column => column.label),
        ...indicatorCells,
      ];
    })
    .filter(row => row.length > 0);

  return [columns, ...rows];
}
