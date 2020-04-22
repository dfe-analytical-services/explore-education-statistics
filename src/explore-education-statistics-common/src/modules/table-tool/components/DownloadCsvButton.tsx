import ButtonText from '@common/components/ButtonText';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import {
  CategoryFilter,
  Filter,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import cartesian from '@common/utils/cartesian';
import React from 'react';
import { utils, writeFile } from 'xlsx';

export const getCsvData = (fullTable: FullTable): string[][] => {
  const { subjectMeta, results } = fullTable;
  const {
    indicators,
    filters: metaFilters,
    timePeriodRange: timePeriods,
    locations,
  } = subjectMeta;

  const filters = transformTableMetaFiltersToCategoryFilters(metaFilters);

  const filterColumns = Object.entries(metaFilters).map(
    ([key]) => metaFilters[key].name,
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
    timePeriods,
    ...Object.values(filters),
  ).map(row => {
    // TODO: Remove ignore when Prettier stops adding trailing comma to tuple type
    // prettier-ignore
    const [location, timePeriod, ...filterOptions] = row as [
      LocationFilter,
      TimePeriodFilter,
      ...CategoryFilter[]
    ];

    const indicatorCells = indicators.map(indicator => {
      const matchingResult = results.find(result => {
        return Boolean(
          filterOptions.every(filter =>
            result.filters.includes(filter.value),
          ) &&
            result.timePeriod === timePeriod.value &&
            result.location[location.level] &&
            result.location[location.level].code === location.value,
        );
      });

      if (!matchingResult) {
        return 'n/a';
      }

      return matchingResult.measures[indicator.value] ?? 'n/a';
    });

    return [
      location.label,
      location.value,
      location.level,
      timePeriod.label.replace(/\//g, ''),
      ...filterOptions.map(column => column.label),
      ...indicatorCells,
    ];
  });

  return [columns, ...rows];
};

interface Props {
  publicationSlug: string;
  fullTable: FullTable;
}

const DownloadCsvButton = ({ publicationSlug, fullTable }: Props) => {
  return (
    <ButtonText
      onClick={() => {
        const workBook = utils.book_new();
        workBook.Sheets.Sheet1 = utils.aoa_to_sheet(getCsvData(fullTable));
        workBook.SheetNames[0] = 'Sheet1';

        writeFile(workBook, `data-${publicationSlug}.csv`, {
          type: 'binary',
        });
      }}
    >
      Download the underlying data of this table (CSV)
    </ButtonText>
  );
};

export default DownloadCsvButton;
