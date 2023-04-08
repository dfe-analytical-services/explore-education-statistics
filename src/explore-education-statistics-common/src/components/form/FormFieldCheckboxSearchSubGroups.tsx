/* eslint-disable react/destructuring-assignment */
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from './FormCheckboxSearchSubGroups';
import handleAllChange from './util/handleAllCheckboxChange';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSearchSubGroupsProps, FormValues>,
  'formGroup'
>;

function FormFieldCheckboxSearchSubGroups<FormValues>(
  props: FormFieldCheckboxSearchSubGroupsProps<FormValues>,
) {
  return (
    <FormField<string[]> {...props}>
      {({ id, field, helpers, meta }) => {
        return (
          <FormCheckboxSearchSubGroups
            {...props}
            {...field}
            id={id}
            small
            onAllChange={(event, checked, options) => {
              if (event.isDefaultPrevented()) {
                return;
              }

              handleAllChange({
                checked,
                meta,
                helpers,
                options: options.flatMap(group => group.options),
              });
            }}
            onSubGroupAllChange={(event, checked, groupOptions) => {
              if (props.onSubGroupAllChange) {
                props.onSubGroupAllChange(event, checked, groupOptions);
              }

              if (event.isDefaultPrevented()) {
                return;
              }

              handleAllChange({
                checked,
                meta,
                helpers,
                options: groupOptions,
              });
            }}
            onChange={(event, option) => {
              if (props.onChange) {
                props.onChange(event, option);
              }

              if (event.isDefaultPrevented()) {
                return;
              }

              field.onChange(event);
            }}
          />
        );
      }}
    </FormField>
  );
}

export default FormFieldCheckboxSearchSubGroups;
