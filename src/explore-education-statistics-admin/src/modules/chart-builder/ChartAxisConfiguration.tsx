import * as React from 'react';
import { FormFieldset, FormCheckbox, FormGroup } from '@common/components/form';
import FormComboBox from '@common/components/form/FormComboBox';
import {
  AxisConfigurationItem,
  AxisGroupBy,
} from '@common/services/publicationService';
import { DataBlockMetadata } from '@common/services/dataBlockService';

interface Props {
  id: string;
  defaultDataType?: AxisGroupBy;
  axisConfiguration: AxisConfigurationItem;
  meta: DataBlockMetadata;
}

const ChartAxisConfiguration = ({ id, axisConfiguration, meta }: Props) => {
  const [selectableUnits] = React.useState<string[]>(() => {
    return axisConfiguration.dataSets
      .map(dataSet => meta.indicators[dataSet.indicator])
      .filter(indicator => indicator !== null)
      .map(indicator => indicator.unit);
  });

  const [selectedUnit] = React.useState<number>(0);

  const [selectedValue, setSelectedValue] = React.useState<string>();

  const [show, setShow] = React.useState<boolean>(true);

  return (
    <FormFieldset id={id} legend={axisConfiguration.title}>
      <p>{axisConfiguration.name} configuration</p>
      <FormGroup>
        <FormCheckbox
          id={`${id}_show`}
          name={`${id}_show`}
          label="Show axis?"
          checked={show}
          onChange={e => {
            setShow(e.target.checked);
          }}
          value="show"
          conditional={
            <React.Fragment>
              {axisConfiguration.type === 'major' && (
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
              )}

              <p>Add / remove grid lines DFE-1008</p>
              <p>Add / remove / edit series labels & range DFE-1018 1017</p>
              <p>Restrict range of series (years only?) DFE-1009</p>

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
