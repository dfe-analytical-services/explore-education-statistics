import { OmitStrict } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';
import {
  handleAllChange,
  handleChange,
} from './util/checkboxGroupFieldHelpers';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & OmitStrict<FormCheckboxGroupProps, 'value'>;

const FormFieldCheckboxGroup = <T extends {}>(props: Props<T>) => {
  const { options } = props;

  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => (
        <FormCheckboxGroup
          {...props}
          {...fieldArrayProps}
          options={options}
          onAllChange={(event, checked) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked);
            }

            handleAllChange(fieldArrayProps, options)(event, checked);
          }}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            handleChange(fieldArrayProps)(event);
          }}
        />
      )}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxGroup;
