import ChartDataConfiguration from '@admin/modules/chart-builder/ChartDataConfiguration';
import Button from '@common/components/Button';
import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import {
  ChartCapabilities,
  ChartDefinition,
  colours,
  generateKeyFromDataSet,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import {
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';

import React from 'react';

import styles from './graph-builder.module.scss';

export interface SelectedData {
  dataSet: {
    indicator: string;
    filters: string[];
  };
  configuration: DataSetConfiguration;
}

interface Props {
  chartType: ChartDefinition;
  indicatorIds: string[];
  filterIds: string[][];
  selectedData?: ChartDataSetAndConfiguration[];
  metaData: DataBlockMetadata;
  onDataAdded?: (data: SelectedData) => void;
  onDataRemoved?: (data: SelectedData, index: number) => void;
  onDataChanged?: (data: SelectedData[]) => void;
  capabilities: ChartCapabilities;
}

export interface ChartDataSetAndConfiguration {
  dataSet: ChartDataSet;
  configuration: DataSetConfiguration;
}

function dataName(
  meta: DataBlockMetadata,
  indicator: string,
  filters: string[],
) {
  return [
    meta.indicators[indicator].label,
    '(',
    ...filters.map(filter => meta.filters[filter].label),
    ')',
  ].join(' ');
}

const ChartDataSelector = ({
  indicatorIds,
  filterIds,
  metaData,
  onDataRemoved,
  onDataAdded,
  onDataChanged,
  selectedData = [],
  capabilities,
}: Props) => {
  const indicatorSelectOptions = [
    {
      label: 'Select an indicator...',
      value: '',
    },
    ...indicatorIds.map<SelectOption>(id => ({ ...metaData.indicators[id] })),
  ];

  const filterSelectOptions = filterIds
    .map(ids => ids.map(id => metaData.filters[id]))
    .reduce<SelectOption[]>(
      (combinedFilters, next) => {
        return [
          ...combinedFilters,
          {
            label: next.map(id => id.label).join(', '),
            value: next.map(id => id.value).join(','),
          },
        ];
      },
      [
        {
          label: 'Select a filter...',
          value: '',
        },
      ],
    );

  const [selectedIndicator, setSelectedIndicator] = React.useState<string>('');
  const [selectedFilters, setSelectedFilters] = React.useState<string>('');

  const [selectedList, setSelectedList] = React.useState<
    ChartDataSetAndConfiguration[]
  >([...selectedData]);

  const removeSelected = (selected: SelectedData, index: number) => {
    const [removed] = selectedList.splice(index, 1);
    setSelectedList([...selectedList]);

    if (onDataRemoved) onDataRemoved(removed, index);
  };

  return (
    <React.Fragment>
      <FormGroup className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <FormFieldset id="filter_fieldset" legend="Filters" legendHidden>
            <FormSelect
              id="filters"
              name="filters"
              label="Filters"
              options={filterSelectOptions}
              value={selectedFilters}
              onChange={e => setSelectedFilters(e.target.value)}
              order={[]}
            />
          </FormFieldset>
        </div>
        <div className="govuk-grid-column-one-half">
          <FormFieldset
            id="indicator_fieldset"
            legend="Indicators"
            legendHidden
          >
            <FormSelect
              id="indicators"
              name="indicators"
              label="Indicators"
              options={indicatorSelectOptions}
              value={selectedIndicator}
              onChange={e => setSelectedIndicator(e.target.value)}
              order={[]}
            />
          </FormFieldset>
        </div>
      </FormGroup>

      {selectedIndicator && selectedFilters && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <Button
              type="button"
              onClick={() => {
                const dataSet = {
                  filters: selectedFilters.split(','),
                  indicator: selectedIndicator,
                };

                const added = {
                  dataSet,
                  configuration: {
                    name: dataName(
                      metaData,
                      dataSet.indicator,
                      dataSet.filters,
                    ),
                    value: generateKeyFromDataSet(dataSet),
                    label: dataName(
                      metaData,
                      dataSet.indicator,
                      dataSet.filters,
                    ),
                    colour: colours[selectedList.length % colours.length],
                    symbol: symbols[selectedList.length % symbols.length],
                  },
                };

                selectedList.push(added);
                setSelectedList([...selectedList]);

                if (onDataAdded) onDataAdded(added);
              }}
            >
              Add data
            </Button>
          </div>
        </div>
      )}

      {selectedList.length > 0 && (
        <table className="govuk-table">
          <caption>Selected data</caption>
          <thead>
            <tr>
              <th className="govuk-table__header">Filters</th>
              <th className="govuk-table__header">Indicator</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {selectedList.map((selected, index) => (
              <React.Fragment
                key={`${
                  selected.dataSet.indicator
                }_${selected.dataSet.filters.join(',')}`}
              >
                <tr className={styles.noCellBorder}>
                  <td className="govuk-table__cell">
                    {selected.dataSet.filters.map(_ => (
                      <span key={_}>{metaData.filters[_].label}</span>
                    ))}
                  </td>
                  <td className="govuk-table__cell">
                    {metaData.indicators[selected.dataSet.indicator].label}
                  </td>
                  <td className="govuk-table__cell">
                    <Button
                      type="button"
                      onClick={() => removeSelected(selected, index)}
                      className="govuk-!-margin-bottom-0"
                    >
                      remove
                    </Button>
                  </td>
                </tr>
                <tr>
                  <td colSpan={3}>
                    <ChartDataConfiguration
                      configuration={selected.configuration}
                      capabilities={capabilities}
                      onConfigurationChange={(value: DataSetConfiguration) => {
                        selectedList[index].configuration = value;
                        const newData = [...selectedList];
                        setSelectedList(newData);
                        if (onDataChanged) onDataChanged(newData);
                      }}
                    />
                  </td>
                </tr>
              </React.Fragment>
            ))}
          </tbody>
        </table>
      )}
    </React.Fragment>
  );
};

export default ChartDataSelector;
