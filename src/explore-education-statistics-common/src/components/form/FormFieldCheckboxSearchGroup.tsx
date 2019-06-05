import { OmitStrict } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from './FormCheckboxSearchGroup';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

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
            onAllChange={event => {
              if (props.onAllChange) {
                props.onAllChange(event, options);
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
        );
      }}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxSearchGroup;
