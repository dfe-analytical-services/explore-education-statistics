import FieldCheckboxArray from '@common/components/form/FieldCheckboxArray';
import { Omit } from '@common/types/util';
import React from 'react';
import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from './FormCheckboxSearchSubGroups';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = {
  showError?: boolean;
} & Omit<
  FormCheckboxSearchSubGroupsProps,
  'onChange' | 'onAllChange' | 'value'
>;

const FormFieldCheckboxSearchSubGroups = <T extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<T>,
) => {
  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => (
        <FormCheckboxSearchSubGroups
          {...props}
          {...fieldArrayProps}
          small
          onAllChange={(event, options) =>
            onAllChange(fieldArrayProps, options)(event)
          }
          onChange={onChange(fieldArrayProps)}
        />
      )}
    </FieldCheckboxArray>
  );
};

export default FormFieldCheckboxSearchSubGroups;
