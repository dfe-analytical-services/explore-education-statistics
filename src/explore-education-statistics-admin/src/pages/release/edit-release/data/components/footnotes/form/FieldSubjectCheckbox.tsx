import React from 'react';
import { Field, FieldProps } from 'formik';
import { get } from 'lodash';
import { FormCheckbox } from '@common/components/form';

export interface FieldCheckboxProps {
  name: string;
  id: string;
  label: string;
  boldLabel?: boolean;
  disabled?: boolean;
  className?: string;
}

const FieldSubjectCheckbox = ({
  id,
  name,
  label,
  boldLabel = false,
  disabled = false,
  className,
}: FieldCheckboxProps) => {
  return (
    <Field name={name}>
      {({ form, field }: FieldProps) => {
        const defaultValue = get(form.values, name);
        return (
          <FormCheckbox
            className={className}
            id={id}
            name={name}
            label={label}
            boldLabel={boldLabel}
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
