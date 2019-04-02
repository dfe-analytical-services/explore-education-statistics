import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import {
  FilterOption,
  GroupedFilterOptions,
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import PrototypeGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from 'src/prototypes/table-tool/components/PrototypeGroupedDataTable';
import { DataTableResult } from 'src/services/tableBuilderService';

interface Props {
  filters: {
    indicators: string[];
    categorical: {
      [key: string]: string[];
    };
    years: number[];
  };
  specification: MetaSpecification;
  results: DataTableResult[];
}

class TimePeriodDataTable extends Component<Props> {
  private parseYear(year?: number): string {
    if (!year) {
      return '';
    }

    const yearString = year.toString();

    if (yearString.length === 6) {
      return `${yearString.substring(0, 4)}/${yearString.substring(4, 6)}`;
    }

    return `${year}/${Number(yearString.substring(2, 4)) + 1}`;
  }

  private groupByValue(
    options: GroupedFilterOptions | FilterOption[],
  ): { [key: string]: FilterOption } {
    if (Array.isArray(options)) {
      return options.reduce((acc, option) => {
        return {
          ...acc,
          [option.value]: {
            ...option,
          },
        };
      }, {});
    }

    return Object.values(options)
      .flatMap(group => group.options)
      .reduce((acc, option) => {
        return {
          ...acc,
          [option.value]: {
            ...option,
          },
        };
      }, {});
  }

  public render() {
    const { filters, results, specification } = this.props;
    const { categorical, indicators, years } = filters;

    const firstYear = this.parseYear(min(filters.years));
    const lastYear = this.parseYear(max(filters.years));

    const columnGroupsByValue = this.groupByValue(
      specification.categoricalFilters.schoolTypes.options,
    );
    const rowGroupsByValue = this.groupByValue(
      specification.categoricalFilters.characteristics.options,
    );

    const indicatorsByValue = this.groupByValue(specification.indicators);

    const header: HeaderGroup[] = categorical.schoolTypes.map(columnGroup => {
      return {
        columns: years.map(this.parseYear),
        label: columnGroupsByValue[columnGroup].label,
      };
    });

    const groupedData: RowGroup[] = categorical.characteristics.map(
      rowGroup => {
        const rows = indicators.map(indicator => {
          const columnGroups = categorical.schoolTypes.map(schoolType =>
            years.map(year => {
              const matchingResult = results.find(result => {
                return Boolean(
                  result.indicators[indicator] !== undefined &&
                    result.characteristic &&
                    result.characteristic.name === rowGroup &&
                    result.timePeriod === year &&
                    result.schoolType === schoolType,
                );
              });

              return matchingResult
                ? `${matchingResult.indicators[indicator]}${indicatorsByValue[
                    indicator
                  ].unit || ''}`
                : '--';
            }),
          );

          return {
            columnGroups,
            label: indicatorsByValue[indicator].label,
          };
        });

        return {
          rows,
          label: rowGroupsByValue[rowGroup].label,
        };
      },
    );

    return (
      <div>
        {firstYear === lastYear && (
          <h3>{`Comparing statistics for ${firstYear}`}</h3>
        )}
        {firstYear !== lastYear && (
          <h3>{`Comparing statistics between ${firstYear} and ${lastYear}`}</h3>
        )}
        <PrototypeGroupedDataTable
          caption=""
          headers={header}
          rowGroups={groupedData}
        />
      </div>
    );
  }
}

export default TimePeriodDataTable;
