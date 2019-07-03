import ButtonText from '@common/components/ButtonText';
import cartesian from '@common/lib/utils/cartesian';
import {
  FilterOption,
  IndicatorOption,
  PublicationSubjectMeta,
  TableData,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types';
import { saveAs } from 'file-saver';
import Papa from 'papaparse';
import React from 'react';

interface Props {
  publicationSlug: string;
  meta: PublicationSubjectMeta;
  indicators: IndicatorOption[];
  filters: Dictionary<FilterOption[]>;
  timePeriods: TimePeriod[];
  locations: Dictionary<FilterOption[]>;
  results: TableData['result'];
}

const DownloadCsvButton = ({
  publicationSlug,
  meta,
  indicators,
  filters,
  timePeriods,
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

    const columns = ['Time period', ...filterColumns, ...indicatorColumns];

    const rows = cartesian<FilterOption | TimePeriod>(
      timePeriods,
      ...Object.values(filters),
    ).map(row => {
      const [timePeriod, ...filterOptions] = row as [TimePeriod, FilterOption];

      const indicatorCells = indicators.map(indicator => {
        const matchingResult = results.find(result => {
          return Boolean(
            filterOptions.every(filter =>
              result.filters.includes(filter.value),
            ) &&
              result.timeIdentifier === timePeriod.code &&
              result.year === timePeriod.year,
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
      Download data (.csv)
    </ButtonText>
  );
};

export default DownloadCsvButton;
