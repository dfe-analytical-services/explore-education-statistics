import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import * as React from 'react';
import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { SelectOption } from '@common/components/form/FormSelect';
import Button from '@common/components/Button';

export interface DataUpdatedEvent {
  indicator: string;
  filters: string[];
}

interface Props {
  chartType: ChartDefinition;
  indicatorIds: string[];
  filterIds: string[][];
  metaData: DataBlockMetadata;
  onDataAdded?: (data: DataUpdatedEvent) => void;
  onDataRemoved?: (data: DataUpdatedEvent) => void;
  onDataUpdated?: (data: DataUpdatedEvent[]) => void;
}

const ChartDataSelector = ({
  indicatorIds,
  filterIds,
  metaData,
  onDataUpdated,
  onDataRemoved,
  onDataAdded,
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

  const [selectedList, setSelectedList] = React.useState<DataUpdatedEvent[]>(
    [],
  );

  const removeSelected = (selected: DataUpdatedEvent, index: number) => {
    const [removed] = selectedList.splice(index, 1);
    setSelectedList([...selectedList]);

    if (onDataRemoved) onDataRemoved(removed);
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

      {selectedIndicator && selectedFilters && (
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <Button
              type="button"
              onClick={() => {
                const added = {
                  filters: selectedFilters.split(','),
                  indicator: selectedIndicator,
                };
                selectedList.push(added);
                setSelectedList([...selectedList]);

                if (onDataAdded) onDataAdded(added);
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
              <tr key={`${selected.indicator}_${selected.filters.join(',')}`}>
                <td className="govuk-table__cell">
                  {selected.filters.map(_ => (
                    <span key={_}>{metaData.filters[_].label}</span>
                  ))}
                </td>
                <td className="govuk-table__cell">
                  {metaData.indicators[selected.indicator].label}
                </td>
                <td className="govuk-table__cell">
                  <Button
                    type="button"
                    onClick={() => removeSelected(selected, index)}
                  >
                    remove
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </React.Fragment>
  );
};

export default ChartDataSelector;
