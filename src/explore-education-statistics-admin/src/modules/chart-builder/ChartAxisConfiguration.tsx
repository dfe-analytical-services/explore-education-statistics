import * as React from 'react';
import { FormFieldset, FormCheckbox, FormGroup } from '@common/components/form';
import FormComboBox from '@common/components/form/FormComboBox';
import { ChartDataSet } from '@common/services/publicationService';
import { DataBlockMetadata } from '@common/services/dataBlockService';

interface Props {
  id: string;
  title: string;
  type: string;
  defaultDataType?: string;
  dataSets: ChartDataSet[];
  meta: DataBlockMetadata;
}

const ChartAxisConfiguration = ({
  id,
  title,
  dataSets,
  meta,
  type,
  defaultDataType,
}: Props) => {
  const [selectableUnits, setSelectableUtils] = React.useState<string[]>(() => {
    if (type === 'value') {
      return dataSets
        .map(dataSet => meta.indicators[dataSet.indicator])
        .filter(indicator => indicator !== null)
        .map(indicator => indicator.unit);
    }
    return [];
  });

  const [selectedUnit, setSelectedUnit] = React.useState<number>(0);

  const [selectedValue, setSelectedValue] = React.useState<string>();

  const [show, setShow] = React.useState<boolean>(true);

  return (
    <FormFieldset id={id} legend={title}>
      <p>{title} configuration</p>
      <FormGroup>
        <FormCheckbox
          id={`${id}_show`}
          name={`${id}_show`}
          defaultChecked
          label="Show axis?"
          checked={show}
          onChange={e => {
            setShow(e.target.checked);
          }}
          value="show"
          conditional={
            <React.Fragment>
              <FormComboBox
                id={`${id}_unit`}
                inputLabel="Unit"
                onInputChange={e => setSelectedValue(e.target.value)}
                inputValue={selectedValue}
                onSelect={selected => {
                  setSelectedValue(selectableUnits[selected]);
                }}
                options={selectableUnits}
                initialOption={selectedUnit}
              />

              {/*
        <FormTextInput
          id={`${id}_name`}
          name={`${id}_name`}
          defaultValue="hello"
          label="hello"
        />*/}
            </React.Fragment>
          }
        />
      </FormGroup>
    </FormFieldset>
  );
};

export default ChartAxisConfiguration;
