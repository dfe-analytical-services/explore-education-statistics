import FieldCheckboxArray from '@common/components/form/FieldCheckboxArray';
import { Omit } from '@common/types/util';
import React from 'react';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & Omit<FormCheckboxGroupProps, 'onChange' | 'onAllChange' | 'value'>;

const FormFieldCheckboxGroup = <T extends {}>(props: Props<T>) => {
  const { options } = props;

  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => (
        <FormCheckboxGroup
          {...props}
          {...fieldArrayProps}
          options={options}
          onAllChange={onAllChange(fieldArrayProps, options)}
          onChange={onChange(fieldArrayProps)}
        />
      )}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxGroup;
