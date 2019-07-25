import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React from 'react';
import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { SelectOption } from '@common/components/form/FormSelect';
import Button from '@common/components/Button';

export interface SelectedData {
  indicator: string;
  filters: string[];
}

interface Props {
  chartType: ChartDefinition;
  indicatorIds: string[];
  filterIds: string[][];
  selectedData?: SelectedData[];
  metaData: DataBlockMetadata;
  onDataAdded?: (data: SelectedData) => void;
  onDataRemoved?: (data: SelectedData, index: number) => void;
  onDataUpdated?: (data: SelectedData[]) => void;
}

const ChartDataSelector = ({
  indicatorIds,
  filterIds,
  metaData,
  onDataUpdated,
  onDataRemoved,
  onDataAdded,
  selectedData = [],
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

  const [selectedList, setSelectedList] = React.useState<SelectedData[]>([
    ...selectedData,
  ]);

  const removeSelected = (selected: SelectedData, index: number) => {
    const [removed] = selectedList.splice(index, 1);
    setSelectedList([...selectedList]);

    if (onDataRemoved) onDataRemoved(removed, index);
    if (onDataUpdated) onDataUpdated(selectedList);
  };

  return (
    <React.Fragment>
      <FormGroup className="govuk-grid-row govuk-!-margin-0">
        <FormFieldset id="filter_fieldset" legend="">
          <div className="govuk-grid-column-one-third">
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
          </div>
          <div className="govuk-grid-column-one-third">
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
          </div>
          {selectedIndicator && selectedFilters && (
            <div className="govuk-grid-column-one-third">
              <Button
                type="button"
                className="govuk-!-margin-top-6"
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
          )}
        </FormFieldset>
      </FormGroup>

      {selectedList.length > 0 && (
        <table className="govuk-table govuk-!-margin-left-3 ">
          <caption>Selected data</caption>
          <thead>
            <tr>
              <th className="govuk-table__header govuk-!-width-one-third">
                Filter
              </th>
              <th className="govuk-table__header govuk-!-width-one-third">
                Indicator
              </th>
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
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  <Button
                    className="govuk-!-margin-0 govuk-button--secondary"
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
