import ButtonText from '@common/components/ButtonText';
import cartesian from '@common/lib/utils/cartesian';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/full-table/types/filters';
import { saveAs } from 'file-saver';
import Papa from 'papaparse';
import React from 'react';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/full-table/utils/tableHeaders';
import { FullTable } from '@common/modules/full-table/types/fullTable';

interface Props {
  publicationSlug: string;
  fullTable: FullTable;
}

const DownloadCsvButton = ({ publicationSlug, fullTable }: Props) => {
  const { subjectMeta, results } = fullTable;
  const {
    indicators,
    filters: metaFilters,
    timePeriodRange: timePeriods,
    locations,
  } = subjectMeta;

  const filters = transformTableMetaFiltersToCategoryFilters(metaFilters);

  const getCsvData = (): string[][] => {
    const filterColumns = Object.entries(metaFilters).map(
      ([key]) => metaFilters[key].legend,
    );

    const indicatorColumns = indicators.map(indicator => {
      const unit = indicator.unit ? ` (${indicator.unit})` : '';
      return `${indicator.label}${unit}`;
    });

    const columns = [
      'Location',
      'Time period',
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
        return matchingResult.measures[indicator.value];
      });

      return [...row.map(column => column.label), ...indicatorCells];
    });

    return [columns, ...rows];
  };

  return (
    <ButtonText
      onClick={() => {
        const data = new Blob([Papa.unparse(getCsvData())], {
          type: 'text/csv;charset=utf-8',
        });

        const filename = `data-${publicationSlug}.csv`;

        saveAs(data, filename, {
          autoBom: true,
        });
      }}
    >
      Download the underlying data of this table (.CSV)
    </ButtonText>
  );
};

export default DownloadCsvButton;
