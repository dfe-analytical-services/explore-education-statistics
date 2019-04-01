import groupBy from 'lodash/groupBy';
import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import GroupedDataTable, {
  GroupedDataSet,
} from 'src/modules/table-tool/components/GroupedDataTable';
import {
  GroupedFilterOptions,
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
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

class DataTable extends Component<Props> {
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
      options: { value: string }[];
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

    const characteristicsByValue = this.groupByValue(specification
      .categoricalFilters.characteristics.options as GroupedFilterOptions);
    const indicatorsByValue = this.groupByValue(specification.indicators);

    const dataBySchool = groupBy(results, 'schoolType');

    const schoolGroups = categorical.schoolTypes
      .filter(schoolType => dataBySchool[schoolType])
      .map(schoolType => {
        const dataByCharacteristic = groupBy(
          dataBySchool[schoolType],
          'characteristic.name',
        );

        // @ts-ignore
        const groupedData: GroupedDataSet[] = categorical.characteristics.map(
          characteristic => {
            if (!dataByCharacteristic[characteristic]) {
              return {
                label: characteristicsByValue[characteristic],
                rows: indicators.map(indicator => ({
                  columns: years.map(() => '--'),
                  label: indicatorsByValue[indicator].label,
                })),
              };
            }

            const dataByTimePeriod = groupBy(
              dataByCharacteristic[characteristic],
              'timePeriod',
            );

            return {
              label: characteristicsByValue[characteristic].label,
              rows: indicators.map(indicator => ({
                columns: years.map(year => {
                  if (!dataByTimePeriod[year]) {
                    return '--';
                  }

                  if (dataByTimePeriod[year].length > 0) {
                    if (dataByTimePeriod[year][0].indicators[indicator]) {
                      const unit = indicatorsByValue[indicator].unit;

                      return `${
                        dataByTimePeriod[year][0].indicators[indicator]
                      }${unit}`;
                    }
                  }

                  return '--';
                }),
                label: indicatorsByValue[indicator].label,
              })),
            };
          },
        );

        return {
          data: groupedData,
          label: schoolKeys[schoolType],
        };
      });

    return (
      <div>
        {firstYear === lastYear && (
          <h3>{`Comparing statistics for ${firstYear}`}</h3>
        )}

        {firstYear !== lastYear && (
          <h3>{`Comparing statistics between ${firstYear} and ${lastYear}`}</h3>
        )}

        {schoolGroups.map(schoolGroup => {
          return (
            <GroupedDataTable
              key={schoolGroup.label}
              caption={schoolGroup.label}
              header={years.map(this.parseYear)}
              groups={schoolGroup.data}
            />
          );
        })}
      </div>
    );
  }
}

export default DataTable;
