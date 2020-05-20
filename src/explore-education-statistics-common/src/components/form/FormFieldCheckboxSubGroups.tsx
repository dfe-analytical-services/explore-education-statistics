import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import handleAllChange from '@common/components/form/util/handleAllCheckboxChange';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxSubGroups, {
  FormCheckboxSubGroupsProps,
} from './FormCheckboxSubGroups';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSubGroupsProps, FormValues>,
  'formGroup'
>;

const FormFieldCheckboxSubGroups = <FormValues extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<FormValues>,
) => {
  return (
    <FormField<string[]> {...props}>
      {({ field, helpers }) => (
        <FormCheckboxSubGroups
          {...props}
          {...field}
          small
          onAllChange={(event, checked, groupOptions) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked, groupOptions);
            }

            handleAllChange({
              helpers,
              options: groupOptions,
              event,
              checked,
              value: field.value,
            });
          }}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            field.onChange(event);
          }}
        />
      )}
    </FormField>
  );
};

export default FormFieldCheckboxSubGroups;
