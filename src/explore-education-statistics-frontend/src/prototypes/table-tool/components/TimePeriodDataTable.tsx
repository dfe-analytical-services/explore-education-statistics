import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from 'src/prototypes/table-tool/components/FixedHeaderGroupedDataTable';
import {
  FilterOption,
  IndicatorOption,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import parseAcademicYear from 'src/prototypes/table-tool/components/utils/parseAcademicYear';
import { DataTableResult } from 'src/services/tableBuilderService';

interface Props {
  filters: {
    indicators: IndicatorOption[];
    categorical: {
      [key: string]: FilterOption[];
    };
    years: number[];
  };
  results: DataTableResult[];
}

class TimePeriodDataTable extends Component<Props> {
  public render() {
    const { filters, results } = this.props;
    const { categorical, indicators, years } = filters;

    const firstYear = parseAcademicYear(min(filters.years));
    const lastYear = parseAcademicYear(max(filters.years));

    const header: HeaderGroup[] = categorical.schoolTypes.map(columnGroup => {
      return {
        columns: years.map(parseAcademicYear),
        label: columnGroup.label,
      };
    });

    const groupedData: RowGroup[] = categorical.characteristics.map(
      rowGroup => {
        const rows = indicators.map(indicator => {
          const columnGroups = categorical.schoolTypes.map(schoolType =>
            years.map(year => {
              const matchingResult = results.find(result => {
                return Boolean(
                  result.indicators[indicator.value] !== undefined &&
                    result.characteristic &&
                    result.characteristic.name === rowGroup.value &&
                    result.timePeriod === year &&
                    result.schoolType === schoolType.value,
                );
              });

              if (!matchingResult) {
                return '--';
              }

              const value = Number(matchingResult.indicators[indicator.value]);

              if (Number.isNaN(value)) {
                return '--';
              }

              return `${value.toLocaleString('en-GB')}${indicator.unit}`;
            }),
          );

          return {
            columnGroups,
            label: indicator.label,
          };
        });

        return {
          rows,
          label: rowGroup.label,
        };
      },
    );

    const caption =
      firstYear !== lastYear
        ? `Comparing statistics for ${firstYear}`
        : `Comparing statistics between ${firstYear} and ${lastYear}`;

    return (
      <div>
        <FixedHeaderGroupedDataTable
          caption={caption}
          headers={header}
          rowGroups={groupedData}
        />
      </div>
    );
  }
}

export default TimePeriodDataTable;
