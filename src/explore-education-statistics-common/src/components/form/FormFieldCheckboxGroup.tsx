import { Omit } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & Omit<FormCheckboxGroupProps, 'value'>;

const FormFieldCheckboxGroup = <T extends {}>(props: Props<T>) => {
  const { options } = props;

  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => (
        <FormCheckboxGroup
          {...props}
          {...fieldArrayProps}
          options={options}
          onAllChange={event => {
            if (props.onAllChange) {
              props.onAllChange(event);
            }

            onAllChange(fieldArrayProps, options)(event);
          }}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            onChange(fieldArrayProps)(event);
          }}
        />
      )}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxGroup;
