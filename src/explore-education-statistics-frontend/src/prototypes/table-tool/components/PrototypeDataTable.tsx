import groupBy from 'lodash/groupBy';
import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import {
  GroupedFilterOptions,
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import PrototypeGroupedDataTable, {
  HeaderGroup,
  RowGroup,
} from 'src/prototypes/table-tool/components/PrototypeGroupedDataTable';
import { DataTableResult } from 'src/services/tableBuilderService';
import SchoolType from 'src/services/types/SchoolType';

const schoolKeys: {
  [key: string]: string;
} = {
  [SchoolType.Dummy]: 'Dummy',
  [SchoolType.State_Funded_Primary]: 'State funded primary schools',
  [SchoolType.State_Funded_Secondary]: 'State funded secondary schools',
  [SchoolType.Special]: 'Special schools',
  [SchoolType.Total]: 'Total',
};

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

class PrototypeDataTable extends Component<Props> {
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

  private groupByValue(groupedValues: {
    [group: string]: {
      options: { value: string; label: string; unit?: string }[];
    };
  }): any {
    return Object.values(groupedValues)
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

    const characteristicByValue = this.groupByValue(specification
      .categoricalFilters.characteristics.options as GroupedFilterOptions);
    const indicatorsByValue = this.groupByValue(specification.indicators);

    const dataBySchool = groupBy(results, 'schoolType');

    const header: HeaderGroup[] = categorical.schoolTypes
      .filter(schoolType => dataBySchool[schoolType])
      .map(schoolType => {
        return {
          columns: years.map(this.parseYear),
          label: schoolKeys[schoolType],
        };
      });

    const groupedData: RowGroup[] = categorical.characteristics.map(
      characteristic => {
        const rows = indicators.map(indicator => {
          const columnGroups = categorical.schoolTypes.map(schoolType => {
            return years.map(year => {
              const matchingResult = results.find(result => {
                return Boolean(
                  result.indicators[indicator] &&
                    result.characteristic &&
                    result.characteristic.name === characteristic &&
                    result.timePeriod === year &&
                    result.schoolType === schoolType,
                );
              });

              return matchingResult
                ? matchingResult.indicators[indicator]
                : '--';
            });
          });

          return {
            columnGroups,
            label: indicatorsByValue[indicator].label,
          };
        });

        return {
          rows,
          label: characteristicByValue[characteristic].label,
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
          caption="Test caption"
          header={header}
          rowGroups={groupedData}
        />
      </div>
    );
  }
}

export default PrototypeDataTable;
