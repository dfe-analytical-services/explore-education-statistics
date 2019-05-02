import FieldCheckboxArray from '@common/components/form/FieldCheckboxArray';
import FormCheckboxSubGroups, {
  FormCheckboxSubGroupsProps,
} from '@common/components/form/FormCheckboxSubGroups';
import { Omit } from '@common/types/util';
import React from 'react';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = {
  showError?: boolean;
} & Omit<FormCheckboxSubGroupsProps, 'onChange' | 'onAllChange' | 'value'>;

const FormFieldCheckboxSubGroups = <T extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<T>,
) => {
  return (
    <FieldCheckboxArray {...props}>
      {fieldArrayProps => (
        <FormCheckboxSubGroups
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

export default FormFieldCheckboxSubGroups;
