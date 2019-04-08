import React, { Component } from 'react';
import FixedHeaderGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from 'src/prototypes/table-tool/components/FixedHeaderGroupedDataTable';
import {
  FilterOption,
  IndicatorOption,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import TableDisplayOptionsForm from 'src/prototypes/table-tool/components/TableDisplayOptionsForm';
import { DataTableResult } from 'src/services/tableBuilderService';
import TimePeriod from 'src/services/types/TimePeriod';

interface Props {
  filters: {
    indicators: IndicatorOption[];
    categorical: {
      [key: string]: FilterOption[];
    };
    timePeriods: TimePeriod[];
  };
  results: DataTableResult[];
}

class TimePeriodDataTable extends Component<Props> {
  public render() {
    const { filters, results } = this.props;
    const { categorical, indicators, timePeriods } = filters;

    const startLabel = timePeriods[0].label;
    const endLabel = timePeriods[timePeriods.length - 1].label;

    const caption =
      startLabel !== endLabel
        ? `Comparing statistics for ${startLabel}`
        : `Comparing statistics between ${startLabel} and ${endLabel}`;

    const header: HeaderGroup[] = categorical.schoolTypes.map(columnGroup => {
      return {
        columns: timePeriods.map(timePeriod => timePeriod.label),
        label: columnGroup.label,
      };
    });

    // TODO: Remove this when timePeriod API finalised
    const formatToAcademicYear = (year: number) => {
      const nextYear = year + 1;
      return parseInt(`${year}${`${nextYear}`.substring(2, 4)}`, 0);
    };

    const groupedData: RowGroup[] = categorical.characteristics.map(
      rowGroup => {
        const rows = indicators.map(indicator => {
          const columnGroups = categorical.schoolTypes.map(schoolType =>
            timePeriods.map(timePeriod => {
              const matchingResult = results.find(result => {
                return Boolean(
                  result.indicators[indicator.value] !== undefined &&
                    result.characteristic &&
                    result.characteristic.name === rowGroup.value &&
                    result.timePeriod ===
                      formatToAcademicYear(timePeriod.year) &&
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
