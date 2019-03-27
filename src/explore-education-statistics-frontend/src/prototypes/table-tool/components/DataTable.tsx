import groupBy from 'lodash/groupBy';
import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import GroupedDataTable, {
  GroupedDataSet,
} from 'src/modules/table-tool/components/GroupedDataTable';
import {
  CharacteristicsMeta,
  DataTableResult,
  IndicatorsMeta,
} from 'src/services/tableBuilderService';
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
  characteristics: string[];
  characteristicsMeta: CharacteristicsMeta;
  indicators: string[];
  indicatorsMeta: IndicatorsMeta;
  results: DataTableResult[];
  schoolTypes: string[];
  years: number[];
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

  private groupByName(groupedValues: {
    [group: string]: {
      name: string;
    }[];
  }): any {
    return Object.values(groupedValues)
      .flatMap(groups => groups)
      .reduce((acc, indicator) => {
        return {
          ...acc,
          [indicator.name]: {
            ...indicator,
          },
        };
      }, {});
  }

  public render() {
    const {
      characteristics,
      characteristicsMeta,
      indicators,
      indicatorsMeta,
      results,
      schoolTypes,
      years,
    } = this.props;
    const firstYear = this.parseYear(min(years));
    const lastYear = this.parseYear(max(years));

    const characteristicsByName = this.groupByName(characteristicsMeta);
    const indicatorsByName = this.groupByName(indicatorsMeta);

    const dataBySchool = groupBy(results, 'schoolType');

    const schoolGroups = schoolTypes
      .filter(schoolType => dataBySchool[schoolType])
      .map(schoolType => {
        const dataByCharacteristic = groupBy(
          dataBySchool[schoolType],
          'characteristic.name',
        );

        const groupedData: GroupedDataSet[] = characteristics.map(
          characteristic => {
            if (!dataByCharacteristic[characteristic]) {
              return {
                name: characteristicsByName[characteristic].label,
                rows: indicators.map(indicator => ({
                  columns: years.map(() => '--'),
                  name: indicatorsByName[indicator].label,
                })),
              };
            }

            const dataByTimePeriod = groupBy(
              dataByCharacteristic[characteristic],
              'timePeriod',
            );

            return {
              name: characteristicsByName[characteristic].label,
              rows: indicators.map(indicator => ({
                columns: years.map(year => {
                  if (!dataByTimePeriod[year]) {
                    return '--';
                  }

                  if (dataByTimePeriod[year].length > 0) {
                    if (dataByTimePeriod[year][0].indicators[indicator]) {
                      const unit = indicatorsByName[indicator].unit;

                      return `${
                        dataByTimePeriod[year][0].indicators[indicator]
                      }${unit}`;
                    }
                  }

                  return '--';
                }),
                name: indicatorsByName[indicator].label,
              })),
            };
          },
        );

        return {
          data: groupedData,
          name: schoolKeys[schoolType],
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
              key={schoolGroup.name}
              caption={schoolGroup.name}
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
