import {
  CategoryFilter,
  Filter,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import cartesian from '@common/utils/cartesian';
import { utils, writeFile } from 'xlsx';

export const getCsvData = (fullTable: FullTable): string[][] => {
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
