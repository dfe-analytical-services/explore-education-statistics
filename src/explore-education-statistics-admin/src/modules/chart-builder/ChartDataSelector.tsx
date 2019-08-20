import ChartDataConfiguration from '@admin/modules/chart-builder/ChartDataConfiguration';
import Button from '@common/components/Button';
import {FormFieldset, FormGroup, FormSelect} from '@common/components/form';
import {SelectOption} from '@common/components/form/FormSelect';
import {
  ChartCapabilities,
  ChartDefinition,
  colours,
  generateKeyFromDataSet,
  symbols,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {DataBlockMetadata} from '@common/services/dataBlockService';
import {
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';

import React from 'react';
import Details from '@common/components/Details';
import {difference} from 'lodash';
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
    ...indicatorIds.map<SelectOption>(id => ({...metaData.indicators[id]})),
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

  const [selectedList, setSelectedList] = React.useState<ChartDataSetAndConfiguration[]>([...selectedData]);

  const [alreadyAdded, setAlreadyAdded] = React.useState(false);

  React.useEffect(() => {
    const filters = selectedFilters.split(',');

    const added =
      selectedList.find(({dataSet}) => {
        return (
          dataSet.indicator === selectedIndicator &&
          difference(dataSet.filters, filters).length === 0
        );
      }) !== undefined;

    setAlreadyAdded(added);
  }, [selectedIndicator, selectedFilters, selectedList]);

  const removeSelected = (selected: SelectedData, index: number) => {
    const [removed] = selectedList.splice(index, 1);
    setSelectedList([...selectedList]);

    if (onDataRemoved) onDataRemoved(removed, index);
  };

  return (
    <React.Fragment>
      <FormGroup className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <FormFieldset id="filter_fieldset" legend="Filters" legendHidden>
            <FormSelect
              id="filters"
              name="filters"
              label="Filters"
              className="govuk-!-width-full"
              options={filterSelectOptions}
              value={selectedFilters}
              onChange={e => setSelectedFilters(e.target.value)}
              order={[]}
            />
          </FormFieldset>
        </div>
        <div className="govuk-grid-column-one-third">
          <FormFieldset
            id="indicator_fieldset"
            legend="Indicators"
            legendHidden
          >
            <FormSelect
              id="indicators"
              name="indicators"
              label="Indicators"
              className="govuk-!-width-full"
              options={indicatorSelectOptions}
              value={selectedIndicator}
              onChange={e => setSelectedIndicator(e.target.value)}
              order={[]}
            />
          </FormFieldset>
        </div>
        {selectedIndicator && selectedFilters && !alreadyAdded && (
          <div className="govuk-grid-column-one-third">
            <Button
              type="button"
              className="govuk-!-margin-bottom-0 govuk-!-margin-top-6"
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
                    unit: metaData.indicators[dataSet.indicator].unit || '',
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
        )}
      </FormGroup>

      {selectedList.length > 0 && (
        <>
          {selectedList.map((selected, index) => (
            <React.Fragment
              key={`${
                selected.dataSet.indicator
                }_${selected.dataSet.filters.join(',')}`}
            >
              <div className={styles.selectedData}>
                <div className={styles.selectedDataRow}>
                  <div className={styles.selectedDataFilter}>
                    <span>
                      {
                        selected.dataSet.filters.map(_ => metaData.filters[_].label).join(", ")
                      }
                    </span>
                  </div>
                  <div className={styles.selectedDataIndicator}>
                    {metaData.indicators[selected.dataSet.indicator].label}
                  </div>

                  <div className={styles.selectedDataAction}>
                    <Button
                      type="button"
                      onClick={() => removeSelected(selected, index)}
                      className="govuk-!-margin-bottom-0 govuk-button--secondary"
                    >
                      Remove
                    </Button>
                  </div>
                </div>
                <div>
                  <Details
                    summary="Change styling"
                    className="govuk-!-margin-bottom-3 govuk-body-s"
                  >
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
                  </Details>
                </div>
              </div>
            </React.Fragment>
          ))}
        </>
      )}
    </React.Fragment>
  );
};

export default ChartDataSelector;
