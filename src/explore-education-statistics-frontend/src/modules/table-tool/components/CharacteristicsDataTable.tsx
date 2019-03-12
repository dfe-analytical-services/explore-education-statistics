import groupBy from 'lodash/groupBy';
import max from 'lodash/max';
import min from 'lodash/min';
import React, { Component } from 'react';
import {
  AttributesMeta,
  CharacteristicsMeta,
  DataTableResult,
  SchoolType,
} from '../../../services/tableBuilderService';
import GroupedDataTable, { GroupedDataSet } from './GroupedDataTable';

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
  attributes: string[];
  attributesMeta: AttributesMeta;
  characteristics: string[];
  characteristicsMeta: CharacteristicsMeta;
  results: DataTableResult[];
  schoolTypes: string[];
  years: number[];
}

class CharacteristicsDataTable extends Component<Props> {
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
      .reduce((acc, attribute) => {
        return {
          ...acc,
          [attribute.name]: {
            ...attribute,
          },
        };
      }, {});
  }

  public render() {
    const {
      attributes,
      attributesMeta,
      characteristics,
      characteristicsMeta,
      results,
      schoolTypes,
      years,
    } = this.props;
    const firstYear = this.parseYear(min(years));
    const lastYear = this.parseYear(max(years));

    const attributesByName = this.groupByName(attributesMeta);
    const characteristicsByName = this.groupByName(characteristicsMeta);

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
                rows: attributes.map(attribute => ({
                  columns: years.map(() => '--'),
                  name: attributesByName[attribute].label,
                })),
              };
            }

            const dataByYear = groupBy(
              dataByCharacteristic[characteristic],
              'year',
            );

            return {
              name: characteristicsByName[characteristic].label,
              rows: attributes.map(attribute => ({
                columns: years.map(year => {
                  if (!dataByYear[year]) {
                    return '--';
                  }

                  if (dataByYear[year].length > 0) {
                    if (dataByYear[year][0].attributes[attribute]) {
                      const unit = attributesByName[attribute].unit;

                      return `${
                        dataByYear[year][0].attributes[attribute]
                      }${unit}`;
                    }
                  }

                  return '--';
                }),
                name: attributesByName[attribute].label,
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

export default CharacteristicsDataTable;
