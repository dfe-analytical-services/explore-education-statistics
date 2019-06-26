import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import * as React from 'react';
import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { SelectOption } from '@common/components/form/FormSelect';
import Button from '@common/components/Button';

export interface DataAddedEvent {
  indicator: string;
  filters: string[];
}

interface Props {
  chartType: ChartDefinition;
  indicatorIds: string[];
  filterIds: string[][];
  metaData: DataBlockMetadata;
  onDataAdded?: (data: DataAddedEvent) => void;
  onDataUpdated?: (data: DataAddedEvent[]) => void;
}

const ChartDataSelector = ({
  indicatorIds,
  filterIds,
  metaData,
  onDataUpdated,
}: Props) => {
  const indicatorSelectOptions = [
    {
      label: 'Select an indicator...',
      value: '',
    },
    ...indicatorIds.map<SelectOption>(id => metaData.indicators[id]),
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

  const [selectedList, setSelectedList] = React.useState<DataAddedEvent[]>([]);

  const removeSelected = (selected: DataAddedEvent, index: number) => {
    selectedList.splice(index, 1);
    setSelectedList(selectedList);

    if (onDataUpdated) onDataUpdated(selectedList);
  };

  return (
    <React.Fragment>
      <FormGroup className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <FormFieldset id="filter_fieldset" legend="Filters">
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
          <FormFieldset id="indicator_fieldset" legend="Indicators">
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

      <div>
        {selectedList.map((selected, index) => (
          <div key={selected.indicator}>
            <div>{selected.indicator}</div>
            <div>
              {selected.filters.map(_ => (
                <div key={_}>{_}</div>
              ))}
            </div>
            <Button
              type="button"
              onClick={() => removeSelected(selected, index)}
            >
              remove
            </Button>
          </div>
        ))}
      </div>

      {selectedIndicator && selectedFilters && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <Button
              type="button"
              onClick={() => {
                selectedList.push({
                  filters: selectedFilters.split(','),
                  indicator: selectedIndicator,
                });
                setSelectedList([...selectedList]);

                if (onDataUpdated) {
                  onDataUpdated(selectedList);
                }
              }}
            >
              Add data
            </Button>
          </div>
        </div>
      )}
    </React.Fragment>
  );
};

export default ChartDataSelector;
