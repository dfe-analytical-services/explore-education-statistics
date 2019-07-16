import { OmitStrict } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from './FormCheckboxSearchGroup';
import {
  handleAllChange,
  handleChange,
} from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchGroupProps<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & OmitStrict<FormCheckboxSearchGroupProps, 'value'>;

const FormFieldCheckboxSearchGroup = <T extends {}>(
  props: FormFieldCheckboxSearchGroupProps<T>,
) => {
  const { options } = props;

  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => {
        return (
          <FormCheckboxSearchGroup
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
        );
      }}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxSearchGroup;
