import ButtonText from '@common/components/ButtonText';
import cartesian from '@common/lib/utils/cartesian';
import {
  PublicationSubjectMeta,
  TableData,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  Filter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod from '@frontend/modules/table-tool/components/types/TimePeriod';
import { saveAs } from 'file-saver';
import Papa from 'papaparse';
import React from 'react';

interface Props {
  publicationSlug: string;
  meta: PublicationSubjectMeta;
  indicators: Indicator[];
  filters: Dictionary<CategoryFilter[]>;
  timePeriods: TimePeriod[];
  locations: LocationFilter[];
  results: TableData['results'];
}

const DownloadCsvButton = ({
  publicationSlug,
  meta,
  indicators,
  filters,
  timePeriods,
  locations,
  results,
}: Props) => {
  const getCsvData = (): string[][] => {
    const filterColumns = Object.entries(filters).map(
      ([key]) => meta.filters[key].legend,
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
        TimePeriod,
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
      Download underlying data (.csv)
    </ButtonText>
  );
};

export default DownloadCsvButton;
