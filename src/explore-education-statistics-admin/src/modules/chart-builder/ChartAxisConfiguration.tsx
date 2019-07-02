import * as React from 'react';
import {
  FormTextInput,
  FormFieldset,
  FormCheckbox,
  FormGroup,
  FormSelect,
} from '@common/components/form';
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
    return ['1', '2', '3'];
    /*
    if (type === 'major') {

      return dataSets
        .map(dataSet => meta.indicators[dataSet.indicator])
        .filter(indicator => indicator !== null)
        .map(indicator => indicator.unit)
        ;
    }*/
  });

  const [selectedUnit, setSelectedUnit] = React.useState<number>(0);

  const [selectedValue, setSelectedValue] = React.useState<string>();

  /*
  React.useEffect(() => {
    setSelectedUnit(0);
  }, [selectableUnits]);
  */

  return (
    <FormFieldset id={id} legend={title}>
      <p>{title} configuration</p>
      <FormGroup>
        <FormCheckbox
          id={`${id}_show`}
          name={`${id}_show`}
          defaultChecked={false}
          label="Show axis?"
          value="show"
        />

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
      </FormGroup>
    </FormFieldset>
  );
};

export default ChartAxisConfiguration;
