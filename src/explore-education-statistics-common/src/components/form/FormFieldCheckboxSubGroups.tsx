import { OmitStrict } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxSubGroups, {
  FormCheckboxSubGroupsProps,
} from './FormCheckboxSubGroups';
import { onAllChange, onChange } from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = {
  showError?: boolean;
} & OmitStrict<FormCheckboxSubGroupsProps, 'value'>;

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
          onAllChange={(event, checked, groupOptions) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked, groupOptions);
            }

            onAllChange(fieldArrayProps, groupOptions)(event, checked);
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

export default FormFieldCheckboxSubGroups;
