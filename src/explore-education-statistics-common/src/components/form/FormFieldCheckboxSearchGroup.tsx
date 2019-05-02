import FieldCheckboxArray from '@common/components/form/FieldCheckboxArray';
import { Omit } from '@common/types/util';
import React from 'react';
import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from './FormCheckboxSearchGroup';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchGroupProps<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & Omit<FormCheckboxSearchGroupProps, 'onChange' | 'onAllChange' | 'value'>;

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
            onAllChange={onAllChange(fieldArrayProps, options)}
            onChange={onChange(fieldArrayProps)}
          />
        );
      }}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxSearchGroup;
