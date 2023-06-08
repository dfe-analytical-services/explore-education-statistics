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
  const { onAllChange, onChange } = props;

  return (
    <FormField<string[]> {...props}>
      {({ id, field, helpers, meta }) => {
        return (
          <FormCheckboxSearchGroup
            {...props}
            {...field}
            id={id}
            onAllChange={(event, checked, options) => {
              onAllChange?.(event, checked, options);

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
              onChange?.(event, option);

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
