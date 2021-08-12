import {
  CategoryFilter,
  Filter,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { WorkerFullTable } from '@common/modules/table-tool/types/workerFullTable';
import cartesian from '@common/utils/cartesian';
import { utils, writeFile } from 'xlsx';

/**
 * Gets the CSV data for download.
 * This is used in the Web Worker so uses a raw version of the FullTable type (WorkerFullTable). When mapFullTable is refactored to not use classes (EES-2613) we can change this to use the standard FullTable type.
 * IMPORTANT: the filters ids are getters which aren't serialised when passed to the worker so cannot be used here.
 */
export const getCsvData = (fullTable: WorkerFullTable): string[][] => {
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

  const rows = cartesian<Filter>(
    locations,
    timePeriodRange,
    ...Object.values(filters).map(filterGroup => filterGroup.options),
  )
    .map(filterCombination => {
      const [location, timePeriod, ...filterOptions] = filterCombination as [
        LocationFilter,
        TimePeriodFilter,
        ...CategoryFilter[]
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
};

export const downloadCsvFile = (csvData: string[][], fileName: string) => {
  const workBook = utils.book_new();
  workBook.Sheets.Sheet1 = utils.aoa_to_sheet(csvData);
  workBook.SheetNames[0] = 'Sheet1';

  writeFile(workBook, `${fileName}.csv`, {
    type: 'binary',
  });
};
