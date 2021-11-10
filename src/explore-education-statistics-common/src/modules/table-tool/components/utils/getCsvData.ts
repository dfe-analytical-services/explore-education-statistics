import {
  WorkerCategoryFilter,
  WorkerFilter,
  WorkerLocationFilter,
  WorkerTimePeriodFilter,
  WorkerFullTable,
} from '@common/modules/table-tool/types/workerFullTable';
import cartesian from '@common/utils/cartesian';

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
          const { geographicLevel } = result;

          return (
            filterOptions.every(filter =>
              result.filters.includes(filter.value),
            ) &&
            result.location[geographicLevel] &&
            result.location[geographicLevel].code === location.value &&
            location.level === geographicLevel &&
            result.timePeriod === timePeriod.value
          );
        });

        if (!matchingResult) {
          return 'n/a';
        }

        return matchingResult.measures[indicator.value] ?? 'n/a';
      });

      if (indicatorCells.every(cell => cell === 'n/a')) {
        return [];
      }

      return [
        location.label,
        location.value,
        location.level,
        timePeriod.label.replace(/\//g, ''),
        ...filterOptions.map(column => column.label),
        ...indicatorCells,
      ];
    })
    .filter(row => row.length > 0);

  return [columns, ...rows];
}
