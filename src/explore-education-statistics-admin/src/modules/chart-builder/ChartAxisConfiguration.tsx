import * as React from 'react';
import {
  FormTextInput,
  FormFieldset,
  FormCheckbox,
} from '@common/components/form';

interface Props {
  id: string;
  title: string;
}

const ChartAxisConfiguration = ({ id, title }: Props) => {
  return (
    <FormFieldset id={id} legend={title}>
      <p>{title} configuration</p>
      <FormCheckbox
        id={`${id}_show`}
        name={`${id}_show`}
        defaultChecked={false}
        label="Show axis?"
        value="show"
      />
      <FormTextInput
        id={`${id}_name`}
        name={`${id}_name`}
        defaultValue="hello"
        label="hello"
      />
    </FormFieldset>
  );
};

export default ChartAxisConfiguration;
