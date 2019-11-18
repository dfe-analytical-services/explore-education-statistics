import React from 'react';
import { Field, FieldProps } from 'formik';
import { get } from 'lodash';
import { FormCheckbox } from '@common/components/form';

export interface FieldCheckboxProps {
  name: string;
  id: string;
  label: string;
  disabled?: boolean;
}

const FieldSubjectCheckbox = ({
  id,
  name,
  label,
  disabled = false,
}: FieldCheckboxProps) => {
  return (
    <Field name={name}>
      {({ form, field }: FieldProps) => {
        const defaultValue = get(form.values, name);
        return (
          <FormCheckbox
            id={id}
            name={name}
            label={label}
            {...field}
            defaultChecked={defaultValue}
            disabled={disabled}
          />
        );
      }}
    </Field>
  );
};

export default FieldSubjectCheckbox;
