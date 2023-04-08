/* eslint-disable react/destructuring-assignment */
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from './FormCheckboxSearchGroup';
import handleAllChange from './util/handleAllCheckboxChange';

export type FormFieldCheckboxSearchGroupProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSearchGroupProps, FormValues>,
  'formGroup'
>;

function FormFieldCheckboxSearchGroup<FormValues>(
  props: FormFieldCheckboxSearchGroupProps<FormValues>,
) {
  return (
    <FormField<string[]> {...props}>
      {({ id, field, helpers, meta }) => {
        return (
          <FormCheckboxSearchGroup
            {...props}
            {...field}
            id={id}
            onAllChange={(event, checked, options) => {
              if (props.onAllChange) {
                props.onAllChange(event, checked, options);
              }

              if (event.isDefaultPrevented()) {
                return;
              }

              handleAllChange({
                checked,
                helpers,
                meta,
                options,
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

export default FormFieldCheckboxSearchGroup;
